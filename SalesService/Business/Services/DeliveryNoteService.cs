using SalesService.Business.Interfaces;
using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;
using SalesService.Business.Models.DeliveryNote;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;
using Microsoft.EntityFrameworkCore;


namespace SalesService.Business.Services
{
    public class DeliveryNoteService : IDeliveryNoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockServiceClient _stockServiceClient;
        private readonly IPurchaseServiceClient _purchaseServiceClient;
        private readonly ICatalogServiceClient _catalogServiceClient;

        public DeliveryNoteService(
            IUnitOfWork unitOfWork,
            IStockServiceClient stockServiceClient,
            IPurchaseServiceClient purchaseServiceClient,
            ICatalogServiceClient catalogServiceClient)
        {
            _unitOfWork = unitOfWork;
            _stockServiceClient = stockServiceClient;
            _purchaseServiceClient = purchaseServiceClient;
            _catalogServiceClient = catalogServiceClient;
        }

        public async Task<DeliveryNoteDTO> CreateAsync(int saleId, DeliveryNoteCreateDTO dto, int userId)
        {
            var sale = await _unitOfWork.SaleRepository.GetIncludingAsync(
                saleId,
                query => query
                    .Include(s => s.Articles)
                    .Include(s => s.DeliveryNotes)
                        .ThenInclude(dn => dn.Articles)
            );

            if (sale == null)
                throw new ArgumentException("Venta no encontrada.");


            var existingWithSameCode = await _unitOfWork.DeliveryNoteRepository.FindAsync(dn => dn.Code == dto.Code);
            if (existingWithSameCode.Any())
                throw new InvalidOperationException("Ya existe un remito con el mismo número.");


            var deliveryNote = new DeliveryNote
            {
                SaleId = saleId,
                WarehouseId = dto.WarehouseId,
                TransportId = dto.TransportId,
                Date = dto.Date,
                Observations = dto.Observations,
                DeclaredValue = dto.DeclaredValue,
                NumberOfPackages = dto.NumberOfPackages,
                Code = dto.Code
            };


            var inputDTO = new CommitedStockInputDTO
            {
                SaleId = saleId,
                WarehouseId = dto.WarehouseId,
                Articles = dto.Articles.Select(a => new CommitedStockArticleInputDTO
                {
                    ArticleId = a.ArticleId,
                    Quantity = a.Quantity
                }).ToList()
            };

            CommitedStockEntryOutputDTO outputDTO = await _stockServiceClient.RegisterCommitedStockConsolidatedAsync(inputDTO, userId);

            foreach (var articleDispatch in outputDTO.Dispatches)
            {
                deliveryNote.Articles.Add(new DeliveryNote_Article
                {
                    ArticleId = articleDispatch.ArticleId,
                    Quantity = articleDispatch.Quantity,
                    DispatchId = articleDispatch.DispatchId,
                    DispatchCode = articleDispatch.DispatchId.HasValue
                        ? await GetDispatchCode(articleDispatch.DispatchId.Value)
                        : null

                });
            }

            await _unitOfWork.DeliveryNoteRepository.AddAsync(deliveryNote);

            var allArticlesDelivered = sale.Articles.All(sa =>
            {
                var totalDelivered = sale.DeliveryNotes
                    .SelectMany(dn => dn.Articles)
                    //.Concat(deliveryNote.Articles)
                    .Where(dna => dna.ArticleId == sa.ArticleId)
                    .Sum(dna => dna.Quantity);

                return totalDelivered >= sa.Quantity;
            });

            if (allArticlesDelivered)
            {
                sale.IsFullyDelivered = true;
                _unitOfWork.SaleRepository.Update(sale);
            }
            


            await _unitOfWork.SaveAsync();

            //deliveryNote.Code = $"RN-{deliveryNote.Id:D8}";
            //_unitOfWork.DeliveryNoteRepository.Update(deliveryNote);
            await _unitOfWork.SaveAsync();

            return await MapToDTO(deliveryNote);
        }
        public async Task<IEnumerable<DeliveryNoteDTO>> GetAllBySaleIdAsync(int saleId)
        {
            var notes = await _unitOfWork.DeliveryNoteRepository.FindIncludingAsync(
                dn => dn.SaleId == saleId,
                dn => dn.Articles,
                dn => dn.Sale
            );

            var result = new List<DeliveryNoteDTO>();
            foreach (var dn in notes)
            {
                result.Add(await MapToDTO(dn));
            }

            return result;
        }
        public async Task<DeliveryNoteDTO> GetByIdAsync(int id)
        {
            var dn = await _unitOfWork.DeliveryNoteRepository.GetIncludingAsync(
                id,
                d => d.Articles,
                d => d.Sale
            );

            if (dn == null)
                throw new KeyNotFoundException("Remito no encontrado.");

            return await MapToDTO(dn);
        }
        private async Task<string?> GetDispatchCode(int dispatchId)
        {
            var dispatch = await _purchaseServiceClient.GetDispatchByIdAsync(dispatchId);
            var response = "";
            if (dispatch != null)
            {
                response = "Orig: " + dispatch.Origin + " Desp: " + dispatch.Code;
            }
            return response;
        }
        private async Task<DeliveryNoteDTO> MapToDTO(DeliveryNote dn)
        {
            CustomerDTO customerDTO = (await _catalogServiceClient.GetCustomerByIdAsync(dn.Sale.CustomerId));
            var warehouseName = (await _catalogServiceClient.GetWarehouseByIdAsync(dn.WarehouseId)).Description;
            TransportDTO transportDTO = (await _catalogServiceClient.GetTransportByIdAsync(dn.TransportId));

            var articleDtos = new List<DeliveryNoteArticleDTO>();
            foreach (var a in dn.Articles)
            {
                var articleName = (await _catalogServiceClient.GetArticleByIdAsync(a.ArticleId)).Description;
                articleDtos.Add(new DeliveryNoteArticleDTO
                {
                    ArticleId = a.ArticleId,
                    ArticleName = articleName,
                    Quantity = a.Quantity,
                    DispatchCode = a.DispatchCode
                });
            }

            return new DeliveryNoteDTO
            {
                Id = dn.Id,
                Code = dn.Code!,
                Date = dn.Date,
                Observations = dn.Observations,
                WarehouseName = warehouseName,
                DeclaredValue = dn.DeclaredValue,
                NumberOfPackages = dn.NumberOfPackages,
                Articles = articleDtos,
                CustomerId = customerDTO.Id,
                CustomerName = customerDTO.CompanyName,
                CustomerAddress = customerDTO.Address,
                CustomerPostalCode = customerDTO.PostalCode,
                CustomerCity = customerDTO.City,
                CustomerTaxId = customerDTO.TaxId,
                CustomerSellCondition = customerDTO.SellCondition,
                CustomerDeliveryAddress = customerDTO.DeliveryAddress,
                CustomerIVAType = customerDTO.IVAType,
                TransportId = transportDTO.Id,
                TransportName = transportDTO.Name,
                TransportAddress = transportDTO.Address,
                TransportCity = transportDTO.City,
                TransportTaxId = transportDTO.TaxId,
                TransportPhone = transportDTO.PhoneNumber
            };
        }
    }
}
