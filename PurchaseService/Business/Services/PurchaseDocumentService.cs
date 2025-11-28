using PurchaseService.Business.Exceptions;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Mapping;
using PurchaseService.Business.Models;
using PurchaseService.Business.Models.Clients;
using PurchaseService.Infrastructure.Interfaces;
using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Business.Services
{
    public class PurchaseDocumentService : IPurchaseDocumentService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IPurchaseDocumentRepository _documentRepository;
        private readonly IAccountingServiceClient _accountingServiceClient;

        public PurchaseDocumentService(
            IPurchaseRepository purchaseRepository,
            IPurchaseDocumentRepository documentRepository,
            IAccountingServiceClient accountingServiceClient)
        {
            _purchaseRepository = purchaseRepository;
            _documentRepository = documentRepository;
            _accountingServiceClient = accountingServiceClient;
        }

        public async Task<PurchaseDocumentDTO> CreateAsync(int purchaseId, PurchaseDocumentCreateDTO dto, int currentUserId)
        {
            if (dto.Currency == "ARS") dto.FxRate = 1m;
            if (dto.Currency == "USD" && dto.FxRate <= 1m)
                throw new DomainException("INVALID_FXRATE", "Para USD, FxRate debe ser ARS por 1 USD (ej: 1450).");


            // 1) Traer la compra
            var purchase = await _purchaseRepository.GetByIdAsync(purchaseId);
            if (purchase == null)
                throw new DomainException("PURCHASE_NOT_FOUND", "Purchase not found.");

            // 2) Reglas por tipo
            if (dto.Type == PurchaseDocumentType.Invoice)
            {
                var invoiceExists = await _documentRepository.ExistsInvoiceAsync(purchaseId, onlyActive: false);
                if (invoiceExists)
                    throw new DomainException("INVOICE_ALREADY_EXISTS", "An invoice already exists for this purchase.");
            }
            else
            {
                var activeInvoice = await _documentRepository.GetInvoiceByPurchaseIdAsync(purchaseId, onlyActive: true);
                if (activeInvoice == null)
                    throw new DomainException("INVOICE_REQUIRED_FOR_NOTES", "An active invoice is required before creating notes.");
            }

            // Duplicado de número por tipo (solo activos)
            var numberExists = await _documentRepository.ExistsByNumberAsync(purchaseId, dto.Type, dto.Number, onlyActive: true);
            if (numberExists)
                throw new DomainException("DOCUMENT_NUMBER_DUPLICATED", "A document with the same type and number already exists.");

            // 3) Crear entidad y persistir
            var entity = PurchaseDocumentMapper.FromCreateDTO(dto, purchaseId);
            await _documentRepository.AddAsync(entity);

            // 4) Hook a Cuentas Corrientes del Proveedor
            try
            {
                var accountingPayload = new AccountingExternalUpsertDTO
                {
                    PartyId = purchase.SupplierId,
                    Kind = dto.Type.ToString(),
                    ExternalRefId = entity.Id,
                    ExternalRefNumber = dto.Number,
                    DocumentDate = dto.Date,
                    Currency = dto.Currency,
                    FxRate = dto.FxRate,
                    AmountARS = dto.TotalAmount * dto.FxRate
                };

                await _accountingServiceClient.UpsertExternalAsync(accountingPayload);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error notifying accounting.", ex);
            }



            // 5) Retorno
            var document = PurchaseDocumentMapper.ToDTO(entity);
            document.AllocationAdvice = await GetAllocationAdviceAsync(purchaseId, document, CancellationToken.None);
            return document;
        }

        public async Task<IReadOnlyList<PurchaseDocumentDTO>> GetByPurchaseIdAsync(int purchaseId, bool onlyActive = false)
        {
            // (opcional) validar existencia de la compra
            var purchase = await _purchaseRepository.GetByIdAsync(purchaseId);
            if (purchase == null)
                throw new DomainException("PURCHASE_NOT_FOUND", "Purchase not found.");

            var list = await _documentRepository.GetByPurchaseIdAsync(purchaseId, onlyActive);
            return list.Select(PurchaseDocumentMapper.ToDTO).ToList();
        }

        public async Task CancelAsync(int purchaseId, int documentId, PurchaseDocumentCancelDTO dto, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new DomainException("CANCEL_REASON_REQUIRED", "Cancel reason is required.");


            var doc = await _documentRepository.GetByIdAsync(documentId);
            if (doc == null || doc.PurchaseId != purchaseId)
                throw new DomainException("DOCUMENT_NOT_FOUND", "Document not found.");

            if (doc.IsCanceled)
                return; // idempotente

            PurchaseDocumentMapper.Cancel(doc, dto.Reason);
            await _documentRepository.UpdateAsync(doc);

            // Hook de cancelación (pendiente)
            try
            {
                await _accountingServiceClient.VoidExternalAsync(doc.Type.ToString(), doc.Id, "supplier");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error notifying accounting of cancellation.", ex);
            }
        }

        private async Task<AllocationAdviceDTO?> GetAllocationAdviceAsync(int purchaseId, PurchaseDocumentDTO document, CancellationToken ct)
        {
            var purchase = await _purchaseRepository.GetByIdAsync(purchaseId);
            if (purchase == null)
                return null;

            // Traer recibos con saldo (Accounting)
            var receipts = await _accountingServiceClient.GetReceiptCreditsAsync(purchase.SupplierId, ct);
            var totalCredit = receipts.Sum(r => r.PendingARS);


            // Armado
            // Si el total de recibos no cubre la factura, no ofrecemos imputación
            if (totalCredit < document.TotalAmount)
                return null;
            return new AllocationAdviceDTO
            {
                CanCoverWithReceipts = true,
                InvoiceExternalRefId = document.Id,     // el ExternalRefId que Accounting guardó para la factura
                InvoicePendingARS = document.TotalAmount,
                Candidates = receipts.ToList()
            };

        }

        public async Task CoverInvoiceWithReceiptsAsync(int purchaseId, CoverInvoiceRequest request, CancellationToken ct = default)
        {
            var purchase = await _purchaseRepository.GetByIdAsync(purchaseId);
            if (purchase is null) throw new KeyNotFoundException($"Purchase {purchaseId} not found.");



            if (request.PartyId != purchase.SupplierId)
                throw new InvalidOperationException("Cover request party does not match purchase supplier.");

            request.PartyType = "supplier";

            await _accountingServiceClient.CoverInvoiceWithReceiptsAsync(request, ct);
        }
    }
}
