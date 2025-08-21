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

            // dentro de CreateAsync:
            var existingWithSameCode = !string.IsNullOrWhiteSpace(dto.Code)
                ? await _unitOfWork.DeliveryNoteRepository.FindAsync(dn => dn.Code == dto.Code)
                : Enumerable.Empty<DeliveryNote>();
            if (existingWithSameCode.Any())
                throw new InvalidOperationException("Ya existe un remito con el mismo número.");

            // check warehouse
            var warehouse = await _catalogServiceClient.GetWarehouseByIdAsync(dto.WarehouseId);
            if (warehouse == null)
                throw new ArgumentException("Depósito no encontrado.");

            // check transport
            if (dto.TransportId.HasValue)
            {
                if (dto.TransportId == 0)
                {
                    dto.TransportId = null; // si es 0, lo dejamos como null
                }
                var transport = await _catalogServiceClient.GetTransportByIdAsync(dto.TransportId.Value);
                if (transport == null)
                    throw new ArgumentException("Transporte no encontrado.");
            }


            var deliveryNote = new DeliveryNote
            {
                SaleId = saleId,
                WarehouseId = dto.WarehouseId,
                // ✅ Transport puede ser null
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
                }).ToList(),
                IsQuick = false,
            };

            CommitedStockEntryOutputDTO outputDTO =
                await _stockServiceClient.RegisterCommitedStockConsolidatedAsync(inputDTO, userId);

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

            

            // Recalcula entregas (usa solo remitos previos; si querés contar el actual, sumalo explícitamente)
            var allArticlesDelivered = sale.Articles.All(sa =>
            {
                var totalPrev = sale.DeliveryNotes
                    .SelectMany(dn => dn.Articles)
                    .Where(dna => dna.ArticleId == sa.ArticleId)
                    .Sum(dna => dna.Quantity);

                var totalWithCurrent = totalPrev + deliveryNote.Articles
                    .Where(a => a.ArticleId == sa.ArticleId)
                    .Sum(a => a.Quantity);

                return totalWithCurrent >= sa.Quantity;
            });

            if (allArticlesDelivered)
            {
                sale.IsFullyDelivered = true;
                _unitOfWork.SaleRepository.Update(sale);
            }

            await _unitOfWork.DeliveryNoteRepository.AddAsync(deliveryNote);

            await _unitOfWork.SaveAsync();

            return await MapToDTO(deliveryNote);
        }

        // Create Quick Delivery Note
        public async Task<DeliveryNoteDTO> CreateQuickAsync(int userId, DeliveryNoteCreateDTO dto)
        {
            var sale = await _unitOfWork.SaleRepository.GetIncludingAsync(
                dto.SaleId,
                query => query
                    .Include(s => s.Articles)
                    .Include(s => s.DeliveryNotes)
                        .ThenInclude(dn => dn.Articles)
            );

            if (sale == null)
                throw new ArgumentException("Venta no encontrada.");

            
            var deliveryNote = new DeliveryNote
            {
                SaleId = dto.SaleId,
                WarehouseId = dto.WarehouseId,
                // ✅ Transport puede ser null
                TransportId = null,
                Date = dto.Date,
                Observations = dto.Observations,
                DeclaredValue = dto.DeclaredValue,
                NumberOfPackages = dto.NumberOfPackages,
                Code = dto.Code
            };

            var inputDTO = new CommitedStockInputDTO
            {
                SaleId = sale.Id,
                WarehouseId = dto.WarehouseId,
                Articles = dto.Articles.Select(a => new CommitedStockArticleInputDTO
                {
                    ArticleId = a.ArticleId,
                    Quantity = a.Quantity
                }).ToList(),
                IsQuick = true,
            };

            CommitedStockEntryOutputDTO outputDTO =
                await _stockServiceClient.RegisterCommitedStockConsolidatedAsync(inputDTO, userId);

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

  
            sale.IsFullyDelivered = true;
            _unitOfWork.SaleRepository.Update(sale);
            

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


        // GetDispatchCode: devolver null si no hay
        private async Task<string?> GetDispatchCode(int dispatchId)
        {
            var dispatch = await _purchaseServiceClient.GetDispatchByIdAsync(dispatchId);
            return dispatch == null ? null : "Orig: " + dispatch.Origin + " Desp: " + dispatch.Code;
        }

        // MapToDTO (versión segura con Transport nullable)
        private async Task<DeliveryNoteDTO> MapToDTO(DeliveryNote dn)
        {
            // ----- Cliente -----
            CustomerDTO customerDTO = new CustomerDTO();
            if (!dn.Sale.IsFinalConsumer)
            {
                customerDTO = await _catalogServiceClient.GetCustomerByIdAsync(dn.Sale.CustomerId!.Value);
            }
            else
            {
                var postalCode = await _catalogServiceClient.GetPostalCodeByIdAsync(dn.Sale.CustomerPostalCodeId!.Value);
                customerDTO.CompanyName = dn.Sale.CustomerName ?? "Consumidor Final";
                customerDTO.Address = "";
                customerDTO.PostalCode = postalCode?.Code ?? "";
                customerDTO.City = postalCode?.City ?? "Sin ciudad";         // ✅ null-safe
                customerDTO.TaxId = dn.Sale.CustomerTaxId ?? "Sin CUIT";
                customerDTO.SellCondition = "Contado";
                customerDTO.DeliveryAddress = "";
                customerDTO.IVAType = "Consumidor Final";
            }

            // ----- Depósito -----
            var warehouse = await _catalogServiceClient.GetWarehouseByIdAsync(dn.WarehouseId);
            var warehouseName = warehouse?.Description ?? "";                // ✅ null-safe

            // ----- Transporte (puede ser null) -----
            TransportDTO? transportDTO = null;
            if (dn.TransportId.HasValue)
            {
                transportDTO = await _catalogServiceClient.GetTransportByIdAsync(dn.TransportId.Value);
            }

            // ----- Artículos -----
            var articleDtos = new List<DeliveryNoteArticleDTO>();
            foreach (var a in dn.Articles)
            {
                var articleInfo = await _catalogServiceClient.GetArticleByIdAsync(a.ArticleId);
                var articleName = articleInfo?.Description ?? $"Artículo {a.ArticleId}";
                articleDtos.Add(new DeliveryNoteArticleDTO
                {
                    ArticleId = a.ArticleId,
                    ArticleName = articleName,
                    Quantity = a.Quantity,
                    DispatchCode = a.DispatchCode
                });
            }

            // ----- DTO -----
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

                // ✅ si tu DTO ya es int? => TransportId = dn.TransportId,
                TransportId = dn.TransportId ?? 0,
                TransportName = transportDTO?.Name ?? "",
                TransportAddress = transportDTO?.Address ?? "",
                TransportCity = transportDTO?.City ?? "",
                TransportTaxId = transportDTO?.TaxId ?? "",
                TransportPhone = transportDTO?.PhoneNumber ?? ""
            };
        }

    }
}
