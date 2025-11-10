using PurchaseService.Business.Exceptions;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Mapping;
using PurchaseService.Business.Models;
using PurchaseService.Infrastructure.Interfaces;
using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Business.Services
{
    public class PurchaseDocumentService : IPurchaseDocumentService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IPurchaseDocumentRepository _documentRepository;

        public PurchaseDocumentService(
            IPurchaseRepository purchaseRepository,
            IPurchaseDocumentRepository documentRepository)
        {
            _purchaseRepository = purchaseRepository;
            _documentRepository = documentRepository;
        }

        public async Task<PurchaseDocumentDTO> CreateAsync(int purchaseId, PurchaseDocumentCreateDTO dto, int currentUserId)
        {
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

            // 4) Hook a Cuentas Corrientes del Proveedor (pendiente)
            // TODO: Llamar al SupplierAccountServiceClient.CreateSupplierLedgerDocumentAsync(...)
            //       Payload esperado:
            //       - SupplierId: purchase.SupplierId
            //       - PurchaseId: purchase.Id
            //       - Type/Number/Date/Currency/FxRate/TotalAmount/FileUrl
            //       - CreatedBy: currentUserId

            // 5) Retorno
            return PurchaseDocumentMapper.ToDTO(entity);
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

            var purchase = await _purchaseRepository.GetByIdAsync(purchaseId);
            if (purchase == null)
                throw new DomainException("PURCHASE_NOT_FOUND", "Purchase not found.");

            var doc = await _documentRepository.GetByIdAsync(documentId);
            if (doc == null || doc.PurchaseId != purchaseId)
                throw new DomainException("DOCUMENT_NOT_FOUND", "Document not found for this purchase.");

            if (doc.IsCanceled)
                return; // idempotente

            PurchaseDocumentMapper.Cancel(doc, dto.Reason);
            await _documentRepository.UpdateAsync(doc);

            // Hook de cancelación (pendiente)
            // TODO: Llamar al SupplierAccountServiceClient.CancelSupplierLedgerDocumentAsync(...)
            //       Payload esperado:
            //       - SupplierId: purchase.SupplierId
            //       - PurchaseId: purchase.Id
            //       - DocumentId: doc.Id
            //       - Type/Number/Reason
            //       - CanceledBy: currentUserId
        }
    }
}
