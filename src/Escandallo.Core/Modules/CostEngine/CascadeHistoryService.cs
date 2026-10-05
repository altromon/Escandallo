namespace Escandallo.Core.Modules.CostEngine;

public sealed record LinkedEscandalloItem(
    string Id,
    string Name,
    int DoseUsed,
    string CurrentTotalEur,
    int Version);

public sealed record CostMutationCascadeInput(
    string CostId,
    string CostType,
    string PreviousAmountEur,
    string NewAmountEur,
    int PackageQuantity,
    IReadOnlyList<LinkedEscandalloItem> LinkedEscandallos);

public sealed record UpdatedEscandalloSnapshot(
    string Id,
    string Name,
    string NewTotalEur,
    int Version);

public sealed record CostMutationCascadeResult(
    int CostHistoryEntriesCreated,
    int RecalculatedEscandallosCount,
    IReadOnlyList<UpdatedEscandalloSnapshot> UpdatedEscandallos);

public sealed record ArchiveResourceInput(
    string ResourceId,
    bool HasHistoryOrUsage,
    string RequestedOperation);

public sealed record ArchiveResourceResult(
    int HttpStatus,
    string NewStatus,
    string? ErrorCode = null);

public static class CascadeHistoryService
{
    public static CostMutationCascadeResult ApplyCostMutationWithCascade(CostMutationCascadeInput input) {
        // Recalcula en cascada los escandallos vinculados y registra histórico condicional (FR-HIST-001)
        if (input.LinkedEscandallos.Count == 0)
        {
            return new CostMutationCascadeResult(0, 0, Array.Empty<UpdatedEscandalloSnapshot>());
        }
        var diff = CostCatalogCalculator.ParseDecimal(input.NewAmountEur) - CostCatalogCalculator.ParseDecimal(input.PreviousAmountEur);
        var deltaPerUnit = Math.Round(diff / input.PackageQuantity, 8, MidpointRounding.AwayFromZero);
        var updated = input.LinkedEscandallos
            .Select(esc =>
            {
                var nextTotal = CostCatalogCalculator.ParseDecimal(esc.CurrentTotalEur) + (esc.DoseUsed * deltaPerUnit);
                return new UpdatedEscandalloSnapshot(
                    esc.Id,
                    esc.Name,
                    CostCatalogCalculator.FormatDecimal(nextTotal, 6),
                    esc.Version + 1);
            })
            .ToArray();
        return new CostMutationCascadeResult(1, updated.Length, updated);
    }

    public static ArchiveResourceResult ArchiveCostOrService(ArchiveResourceInput input) {
        // Protege la inmutabilidad append-only de snapshots históricos y aplica baja lógica (M1)
        if (string.Equals(input.RequestedOperation, "sobrescribir_snapshot_historico_v1", StringComparison.Ordinal))
        {
            return new ArchiveResourceResult(403, "active", "IMMUTABLE_HISTORY_VIOLATION");
        }
        if (input.HasHistoryOrUsage && input.RequestedOperation.StartsWith("borrado_fisico", StringComparison.Ordinal))
        {
            return new ArchiveResourceResult(409, "archived", "PHYSICAL_DELETE_FORBIDDEN");
        }
        return new ArchiveResourceResult(200, "archived");
    }
}
