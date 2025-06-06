﻿using SalesService.Business.Models.SalesQuote;

namespace SalesService.Business.Interfaces
{
    public interface ISalesQuoteService
    {
        Task<SalesQuoteDTO> CreateAsync(SalesQuoteCreateDTO dto);
        Task<IEnumerable<SalesQuoteListDTO>> GetAllAsync();
        Task<SalesQuoteDTO> GetByIdAsync(int id);
    }
}
