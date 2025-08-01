﻿using FiscalDocumentationService.Infrastructure.Models;

namespace FiscalDocumentationService.Infrastructure.Interfaces
{
    public interface IFiscalDocumentRepository
    {
        Task<FiscalDocument> CreateAsync(FiscalDocument document);
        Task<FiscalDocument?> GetByIdAsync(int id);
        Task<FiscalDocument?> GetBySalesOrderIdAsync(int salesOrderId);
    }
}
