﻿using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IStockServiceClient
    {
        Task<List<DispatchStockDetailDTO>> RegisterMovementAsync(StockMovementCreateDTO dto, int userId);
    }
}
