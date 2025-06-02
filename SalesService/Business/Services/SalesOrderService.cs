using SalesService.Business.Interfaces;
using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.SalesOrder;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.SalesOrder;

namespace SalesService.Business.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatalogServiceClient _catalogServiceClient;
        private readonly IUserServiceClient _userServiceClient;
        public SalesOrderService(
            IUnitOfWork unitOfWork,
            ICatalogServiceClient catalogServiceClient,
            IUserServiceClient userServiceClient)
        {
            _unitOfWork = unitOfWork;
            _catalogServiceClient = catalogServiceClient;
            _userServiceClient = userServiceClient;
        }

        public async Task<SalesOrderDTO> CreateAsync(SalesOrderCreateDTO dto)
        {
            var customer = await _catalogServiceClient.GetCustomerByIdAsync(dto.CustomerId)
                ?? throw new Exception($"Customer {dto.CustomerId} not found");

            var seller = await _userServiceClient.GetUserByIdAsync(dto.SellerId)
                ?? throw new Exception($"Seller {dto.SellerId} not found");

            var transport = await _catalogServiceClient.GetTransportByIdAsync(dto.TransportId)
                ?? throw new Exception($"Transport {dto.TransportId} not found");

            var priceList = await _catalogServiceClient.GetPriceListByIdAsync(dto.PriceListId)
                ?? throw new Exception($"PriceList {dto.PriceListId} not found");

            var warehouse = await _catalogServiceClient.GetWarehouseByIdAsync(dto.WarehouseId)
                ?? throw new Exception($"Warehouse {dto.WarehouseId} not found");

            var salesOrder = new SalesOrder
            {
                Date = dto.Date,
                CustomerId = dto.CustomerId,
                SellerId = dto.SellerId,
                PriceListId = dto.PriceListId,
                TransportId = dto.TransportId,
                WarehouseId = dto.WarehouseId,
                Notes = dto.Notes,
                ExchangeRate = dto.ExchangeRate,
                Subtotal = dto.Subtotal,
                Total = dto.Total,
                IsDelivered = false,
                Articles = dto.Articles.Select(a => new SalesOrder_Article
                {
                    ArticleId = a.ArticleId,
                    Quantity = a.Quantity,
                    UnitPrice = a.UnitPrice,
                    DiscountPercent = a.DiscountPercent,
                    IVA = a.IVA,
                    Total = a.Total
                }).ToList()
            };

            await _unitOfWork.SalesOrderRepository.AddAsync(salesOrder);
            await _unitOfWork.SaveAsync();

            return await GetByIdAsync(salesOrder.Id); // Devolver el DTO completo con enriquecimiento
        }
        public async Task<SalesOrderDTO> GetByIdAsync(int id)
        {
            var order = await _unitOfWork.SalesOrderRepository.GetIncludingAsync(id, o => o.Articles)
                ?? throw new Exception($"SalesOrder {id} not found");

            var customer = await _catalogServiceClient.GetCustomerByIdAsync(order.CustomerId)
                ?? throw new Exception($"Customer {order.CustomerId} not found");

            var seller = await _userServiceClient.GetUserByIdAsync(order.SellerId)
                ?? throw new Exception($"Seller {order.SellerId} not found");
            
            var transport = order.TransportId.HasValue
                ? await _catalogServiceClient.GetTransportByIdAsync(order.TransportId.Value)
                : null;

            var warehouse = await _catalogServiceClient.GetWarehouseByIdAsync(order.WarehouseId)
                ?? throw new Exception($"Warehouse {order.WarehouseId} not found");

            var articleDTOs = new List<SalesOrder_ArticleDTO>();

            foreach (var item in order.Articles)
            {
                var article = await _catalogServiceClient.GetArticleByIdAsync(item.ArticleId)
                    ?? throw new Exception($"Article {item.ArticleId} not found");

                articleDTOs.Add(new SalesOrder_ArticleDTO
                {
                    ArticleId = item.ArticleId,
                    ArticleDescription = article.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    DiscountPercent = item.DiscountPercent,
                    IVA = item.IVA,
                    Total = item.Total
                });
            }

            return new SalesOrderDTO
            {
                Id = order.Id,
                Date = order.Date,
                CustomerId = customer.Id,
                CustomerName = customer.CompanyName,
                SellerId = seller.Id,
                SellerName = $"{seller.FirstName} {seller.LastName}",
                TransportId = transport?.Id,
                TransportName = transport?.Name ?? "Unknown",
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Description,
                Notes = order.Notes,
                ExchangeRate = order.ExchangeRate,
                Subtotal = order.Subtotal,
                Total = order.Total,
                IsDelivered = order.IsDelivered,
                Articles = articleDTOs
            };
        }
        public async Task<IEnumerable<SalesOrderListDTO>> GetAllAsync()
        {
            var orders = await _unitOfWork.SalesOrderRepository.GetAllIncludingAsync(x => x.Articles);
            var list = new List<SalesOrderListDTO>();

            foreach (var order in orders)
            {
                var customer = await _catalogServiceClient.GetCustomerByIdAsync(order.CustomerId);
                var seller = await _userServiceClient.GetUserByIdAsync(order.SellerId);

                list.Add(new SalesOrderListDTO
                {
                    Id = order.Id,
                    Date = order.Date,
                    CustomerName = customer?.CompanyName ?? "Unknown",
                    SellerName = seller != null ? $"{seller.FirstName} {seller.LastName}" : "Unknown",
                    Total = order.Total,
                    ExchangeRate = order.ExchangeRate,
                    IsDelivered = order.IsDelivered
                });
            }

            return list.OrderByDescending(x => x.Date);
        }
    }
}
