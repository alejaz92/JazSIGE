﻿using SalesService.Business.Interfaces;
using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;
using SalesService.Business.Models.DeliveryNote;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;

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
            var sale = await _unitOfWork.SaleRepository.GetIncludingAsync(saleId, s => s.Articles);
            if (sale == null)
                throw new ArgumentException("Venta no encontrada.");

            var deliveryNote = new DeliveryNote
            {
                SaleId = saleId,
                WarehouseId = dto.WarehouseId,
                TransportId = dto.TransportId,
                Date = dto.Date,
                Observations = dto.Observations
            };

            foreach (var item in dto.Articles)
            {
                // Descontar stock por venta
                var breakdown = await _stockServiceClient.RegisterMovementAsync(new StockMovementCreateDTO
                {
                    ArticleId = item.ArticleId,
                    Quantity = item.Quantity,
                    MovementType = 4, // codigo del stockMovement Type para Sale
                    FromWarehouseId = dto.WarehouseId,
                    Reference = $"Remito por venta #{sale.Id}"
                }, userId);

                foreach (var b in breakdown)
                {
                    deliveryNote.Articles.Add(new DeliveryNote_Article
                    {
                        ArticleId = item.ArticleId,
                        Quantity = b.Quantity,
                        DispatchId = b.DispatchId,
                        DispatchCode = b.DispatchId.HasValue ? await GetDispatchCode(b.DispatchId.Value) : null
                    });
                }
            }

            await _unitOfWork.DeliveryNoteRepository.AddAsync(deliveryNote);
            await _unitOfWork.SaveAsync();

            deliveryNote.Code = $"RN-{deliveryNote.Id:D8}";
            _unitOfWork.DeliveryNoteRepository.Update(deliveryNote);
            await _unitOfWork.SaveAsync();

            return await MapToDTO(deliveryNote);
        }
        public async Task<IEnumerable<DeliveryNoteDTO>> GetAllBySaleIdAsync(int saleId)
        {
            var notes = await _unitOfWork.DeliveryNoteRepository.GetAllIncludingAsync(dn => dn.SaleId == saleId, dn => dn.Articles, dn => dn.Sale);
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
            return dispatch?.Code;
        }
        private async Task<DeliveryNoteDTO> MapToDTO(DeliveryNote dn)
        {
            var customerName = (await _catalogServiceClient.GetCustomerByIdAsync(dn.Sale.CustomerId)).CompanyName;
            var warehouseName = (await _catalogServiceClient.GetWarehouseByIdAsync(dn.WarehouseId)).Description;
            var transportName = (await _catalogServiceClient.GetTransportByIdAsync(dn.TransportId)).Name;

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
                CustomerName = customerName,
                WarehouseName = warehouseName,
                TransportName = transportName,
                Articles = articleDtos
            };
        }
    }
}
