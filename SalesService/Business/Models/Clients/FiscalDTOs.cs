﻿namespace SalesService.Business.Models.Clients
{
    public class FiscalDocumentCreateDTO
    {
        public int PointOfSale { get; set; }
        public int InvoiceType { get; set; }
        public int BuyerDocumentType { get; set; }
        public long BuyerDocumentNumber { get; set; }

        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal ExemptAmount { get; set; }
        public decimal NonTaxableAmount { get; set; }
        public decimal OtherTaxesAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public int? SalesOrderId { get; set; }

        public List<FiscalDocumentItemDTO> Items { get; set; } = new();
    }

    public class FiscalDocumentItemDTO
    {
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int VatId { get; set; } = 5;
        public decimal VatBase { get; set; }
        public decimal VatAmount { get; set; }
        public string? DispatchCode { get; set; } = null;
        public int Warranty { get; set; } = 0; // Default warranty period in months
    }

    public class FiscalDocumentResponseDTO
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public int InvoiceType { get; set; }
        public int PointOfSale { get; set; }
        public DateTime Date { get; set; }

        public string Cae { get; set; } = string.Empty;
        public DateTime CaeExpiration { get; set; }

        public int BuyerDocumentType { get; set; }
        public long BuyerDocumentNumber { get; set; }

        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal ExemptAmount { get; set; }
        public decimal NonTaxableAmount { get; set; }
        public decimal OtherTaxesAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public int? SalesOrderId { get; set; }

        public List<FiscalDocumentItemDTO> Items { get; set; } = new();
    }
}
