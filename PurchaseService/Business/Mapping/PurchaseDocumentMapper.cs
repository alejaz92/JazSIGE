using System;
using PurchaseService.Business.Models;
using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Business.Mapping
{
    public static class PurchaseDocumentMapper
    {
        // Domain -> DTO
        public static PurchaseDocumentDTO ToDTO(PurchaseDocument src)
        {
            return new PurchaseDocumentDTO
            {
                Id = src.Id,
                PurchaseId = src.PurchaseId,
                Type = src.Type,
                Number = src.Number,
                Date = src.Date,
                Currency = src.Currency,
                FxRate = src.FxRate,
                TotalAmount = src.TotalAmount,
                FileUrl = src.FileUrl,
                IsCanceled = src.IsCanceled,
                CanceledAt = src.CanceledAt,
                CanceledReason = src.CanceledReason,
                CreatedAt = src.CreatedAt,
                UpdatedAt = src.UpdatedAt
            };
        }

        // CreateDTO -> Domain (sin PurchaseId; lo setea el Service)
        public static PurchaseDocument FromCreateDTO(PurchaseDocumentCreateDTO src, int purchaseId)
        {
            var now = DateTime.UtcNow;

            return new PurchaseDocument
            {
                PurchaseId = purchaseId,
                Type = src.Type,
                Number = src.Number.Trim(),
                Date = src.Date,                 // asumir ya normalizado
                Currency = src.Currency,
                FxRate = src.FxRate,
                TotalAmount = src.TotalAmount,
                FileUrl = src.FileUrl.Trim(),
                IsCanceled = false,
                CanceledAt = null,
                CanceledReason = null,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        // Actualización de metadatos mínimos (por si luego permitís update del doc)
        public static void TouchUpdatedAt(PurchaseDocument entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
        }

        // Anulación
        public static void Cancel(PurchaseDocument entity, string reason)
        {
            entity.IsCanceled = true;
            entity.CanceledAt = DateTime.UtcNow;
            entity.CanceledReason = reason?.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
