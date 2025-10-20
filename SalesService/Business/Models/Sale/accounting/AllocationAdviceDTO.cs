using SalesService.Business.Models.Clients;

namespace SalesService.Business.Models.Sale.accounting
{
    public class AllocationAdviceDTO
    {
        public bool CanCoverWithReceipts { get; init; }
        public int InvoiceExternalRefId { get; init; }
        public decimal InvoicePendingARS { get; init; }
        public List<ReceiptCreditDTO> Candidates { get; init; } = new();
    }
}
