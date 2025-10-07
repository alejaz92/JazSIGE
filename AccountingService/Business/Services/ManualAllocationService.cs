using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static AccountingService.Business.Models.Ledger.ManualAllocationDTO;

namespace AccountingService.Business.Services
{
    public sealed class ManualAllocationService : IManualAllocationService
    {
        private readonly IUnitOfWork _uow;

        public ManualAllocationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        private sealed record ReceiptLite(
            int Id, PartyType PartyType, int PartyId, bool IsVoided, decimal TotalBase
        );

        private sealed record CreditDocLite(
            int Id, PartyType PartyType, int PartyId,
            LedgerDocumentStatus Status, LedgerDocumentKind Kind, decimal TotalBase
        );


        public async Task<ManualAllocationPreviewDTO> PreviewAsync(ManualAllocationExecuteDTO req, CancellationToken ct = default)
        {
            var preview = new ManualAllocationPreviewDTO
            {
                CustomerId = req.CustomerId,
                Debits = req.Debits
                    .Select(d => new ManualAllocationDebitSet
                    {
                        DebitDocumentId = d.DebitDocumentId,
                        Sources = d.Sources.Select(s => s with { AmountBase = Round2(s.AmountBase) }).ToList()
                    })
                    .ToList()
            };

            var warnings = await ValidateAsync(req, dryRun: true, ct);
            preview.Warnings.AddRange(warnings);
            return preview;
        }

        public async Task<ManualAllocationPreviewDTO> ExecuteAsync(ManualAllocationExecuteDTO req, CancellationToken ct = default)
        {
            var warnings = await ValidateAsync(req, dryRun: true, ct);
            var result = new ManualAllocationPreviewDTO
            {
                CustomerId = req.CustomerId,
                Debits = req.Debits,
                Warnings = warnings.ToList()
            };

            if (result.Warnings.Count > 0)
                return result; // No ejecuta si no cumple exact cover

            // Ejecutar
            using var tx = await _uow.BeginTransactionAsync(ct);

            foreach (var debitSet in req.Debits)
            {
                foreach (var src in debitSet.Sources)
                {
                    var amount = Round2(src.AmountBase);
                    if (amount <= 0) continue;

                    var allocation = new Allocation
                    {
                        Source = src.SourceKind == ManualAllocationSourceKind.Receipt
                            ? AllocationSource.Receipt
                            : AllocationSource.CreditDocument,
                        ReceiptId = src.SourceKind == ManualAllocationSourceKind.Receipt ? src.SourceId : null,
                        CreditDocumentId = src.SourceKind == ManualAllocationSourceKind.CreditDocument ? src.SourceId : null,
                        DebitDocumentId = debitSet.DebitDocumentId,
                        AmountBase = amount,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _uow.Allocations.AddAsync(allocation);
                }
            }

            await _uow.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return await PreviewAsync(req, ct); // Devuelve forma homogénea y sin warnings
        }

        // ----------------- Validaciones de negocio (sin FIFO, exact cover, consistencia) -----------------

        private async Task<List<string>> ValidateAsync(ManualAllocationExecuteDTO req, bool dryRun, CancellationToken ct)
        {
            var errors = new List<string>();

            if (req.Debits.Count == 0)
            {
                errors.Add("Debe especificar al menos un documento débito.");
                return errors;
            }

            // 1) Traer debits (facturas/ND) y validar pertenencia/estado/kind
            var debitIds = req.Debits.Select(d => d.DebitDocumentId).Distinct().ToList();
            var debits = await _uow.LedgerDocuments.Query()
                .Where(d => debitIds.Contains(d.Id))
                .Select(d => new { d.Id, d.PartyType, d.PartyId, d.Status, d.Kind, d.TotalBase })
                .ToListAsync(ct);

            if (debits.Count != debitIds.Count)
                errors.Add("Uno o más documentos débito no existen.");

            foreach (var d in debits)
            {
                if (d.PartyType != PartyType.Customer || d.PartyId != req.CustomerId)
                    errors.Add($"El débito {d.Id} no pertenece al cliente {req.CustomerId}.");

                if (d.Status != LedgerDocumentStatus.Active)
                    errors.Add($"El débito {d.Id} no está activo.");

                if (d.Kind != LedgerDocumentKind.Invoice && d.Kind != LedgerDocumentKind.DebitNote)
                    errors.Add($"El documento {d.Id} no es un débito (Factura/ND).");
            }

            if (errors.Count > 0) return errors;

            // 2) Cálculo de pendiente actual de cada débito
            var appliedByDebit = await _uow.Allocations.GetAppliedByDocumentsAsync(debitIds, ct);
            var pendingByDebit = debits.ToDictionary(
                d => d.Id,
                d => Round2(d.TotalBase - (appliedByDebit.TryGetValue(d.Id, out var ap) ? ap : 0m))
            );

            // 3) Reunir todos los sources (recibos + NC) y validar disponibilidad y pertenencia
            var receiptIds = req.Debits.SelectMany(d => d.Sources)
                .Where(s => s.SourceKind == ManualAllocationSourceKind.Receipt)
                .Select(s => s.SourceId).Distinct().ToList();

            var creditDocIds = req.Debits.SelectMany(d => d.Sources)
                .Where(s => s.SourceKind == ManualAllocationSourceKind.CreditDocument)
                .Select(s => s.SourceId).Distinct().ToList();

            var receipts = await _uow.Receipts.Query()
                .Where(r => receiptIds.Contains(r.Id))
                .Select(r => new ReceiptLite(r.Id, r.PartyType, r.PartyId, r.IsVoided, r.TotalBase))
                .ToListAsync(ct);


            if (receipts.Count != receiptIds.Count)
                errors.Add("Uno o más recibos no existen.");

            foreach (var r in receipts)
            {
                if (r.PartyType != PartyType.Customer || r.PartyId != req.CustomerId)
                    errors.Add($"El recibo {r.Id} no pertenece al cliente {req.CustomerId}.");
                if (r.IsVoided)
                    errors.Add($"El recibo {r.Id} está anulado.");
            }

            List<CreditDocLite> creditDocs;
            if (creditDocIds.Count > 0)
            {
                creditDocs = creditDocIds.Count == 0
                    ? new List<CreditDocLite>()
                    : await _uow.LedgerDocuments.Query()
                        .Where(d => creditDocIds.Contains(d.Id))
                        .Select(d => new CreditDocLite(d.Id, d.PartyType, d.PartyId, d.Status, d.Kind, d.TotalBase))
                        .ToListAsync(ct);


                if (creditDocs.Count != creditDocIds.Count)
                    errors.Add("Una o más notas de crédito no existen.");

                foreach (var c in creditDocs)
                {
                    if (c.PartyType != PartyType.Customer || c.PartyId != req.CustomerId)
                        errors.Add($"La NC {c.Id} no pertenece al cliente {req.CustomerId}.");
                    if (c.Status != LedgerDocumentStatus.Active)
                        errors.Add($"La NC {c.Id} no está activa.");
                    if (c.Kind != LedgerDocumentKind.CreditNote)
                        errors.Add($"El documento {c.Id} no es una Nota de Crédito.");
                }
            }
            else
            {
                creditDocs = new List<CreditDocLite>();
            }

            if (errors.Count > 0) return errors;

            // 4) Disponibles por fuente
            var appliedByReceipt = receiptIds.Count == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByReceiptsAsync(receiptIds, ct);

            var receiptAvailable = receipts.ToDictionary(
                r => r.Id,
                r => Round2(r.TotalBase - (appliedByReceipt.TryGetValue(r.Id, out var ap) ? ap : 0m)));


            var appliedByCreditDoc = creditDocIds.Count == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByCreditDocsAsync(creditDocIds, ct);

            var creditDocAvailable = creditDocs.ToDictionary(
            c => c.Id,
            c => Round2(c.TotalBase - (appliedByCreditDoc.TryGetValue(c.Id, out var ap) ? ap : 0m)));


            // 5) Validaciones por cada débito (exact cover + no sobresuscribir disponibles)
            const decimal tol = 0.01m; // tolerancia de redondeo
            foreach (var dset in req.Debits)
            {
                if (!pendingByDebit.TryGetValue(dset.DebitDocumentId, out var pending))
                {
                    errors.Add($"No se pudo calcular el pendiente del débito {dset.DebitDocumentId}.");
                    continue;
                }

                // Suma por débito
                var sumForDebit = Round2(dset.Sources.Sum(s => s.AmountBase));
                if (Math.Abs(sumForDebit - pending) > tol)
                    errors.Add($"El débito {dset.DebitDocumentId} no queda 100% cubierto. Pendiente: {pending:N2}, propuesto: {sumForDebit:N2}.");

                // Chequear disponibles por cada source
                foreach (var s in dset.Sources)
                {
                    if (s.AmountBase <= 0)
                    {
                        errors.Add($"Monto inválido en la fuente {s.SourceKind} {s.SourceId} para el débito {dset.DebitDocumentId}.");
                        continue;
                    }

                    if (s.SourceKind == ManualAllocationSourceKind.Receipt)
                    {
                        if (!receiptAvailable.TryGetValue(s.SourceId, out var avail))
                            errors.Add($"No se pudo obtener disponible del recibo {s.SourceId}.");
                        else if (s.AmountBase - avail > tol)
                            errors.Add($"El recibo {s.SourceId} no tiene disponible suficiente. Disponible: {avail:N2}, solicitado: {s.AmountBase:N2}.");
                    }
                    else
                    {
                        if (!creditDocAvailable.TryGetValue(s.SourceId, out var avail))
                            errors.Add($"No se pudo obtener disponible de la NC {s.SourceId}.");
                        else if (s.AmountBase - avail > tol)
                            errors.Add($"La NC {s.SourceId} no tiene disponible suficiente. Disponible: {avail:N2}, solicitado: {s.AmountBase:N2}.");
                    }
                }
            }

            return errors;
        }

        private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.ToEven);
    }
}
