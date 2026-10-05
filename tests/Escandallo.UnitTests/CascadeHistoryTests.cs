using Escandallo.Core.Modules.CostEngine;
using Xunit;

namespace Escandallo.UnitTests;

/// <summary>
/// Suite BDD/TDD en C# xUnit para CMP-COST-ENGINE-001 — Recálculo en Cascada e Histórico (TSK-004).
/// Requisitos cubiertos: FR-HIST-001, BR-RECALC-HISTORY-001, SEC-REQ-DOS-001.
/// </summary>
public sealed class CascadeHistoryTests
{
    [Fact]
    public void FrHist001_RecalculatesLinkedEscandallosInCascadeAndCreatesCostHistory() {
        // Escenario nominal: mutación de producto vinculado a 2 escandallos activos
        var linked = new[]
        {
            new LinkedEscandalloItem("esc-1", "Higiene Facial", 10, "24.121791", 1),
            new LinkedEscandalloItem("esc-2", "Ritual Hidratante", 20, "18.500000", 1)
        };
        var mutation = CascadeHistoryService.ApplyCostMutationWithCascade(new CostMutationCascadeInput(
            "prod-cleasing-milk", "product", "34.51", "39.51", 500, linked));
        Assert.Equal(1, mutation.CostHistoryEntriesCreated);
        Assert.Equal(2, mutation.RecalculatedEscandallosCount);
        Assert.Equal("24.221791", mutation.UpdatedEscandallos[0].NewTotalEur);
        Assert.Equal(2, mutation.UpdatedEscandallos[0].Version);
        Assert.Equal("18.700000", mutation.UpdatedEscandallos[1].NewTotalEur);
    }

    [Fact]
    public void FrHist001_DoesNotCreateCostHistoryWhenProductIsUnusedByEscandallos() {
        // Regla de histórico condicional (BR-RECALC-HISTORY-001): 0 escandallos vinculados -> 0 entradas
        var unusedMutation = CascadeHistoryService.ApplyCostMutationWithCascade(new CostMutationCascadeInput(
            "prod-unused-serum", "product", "20.00", "25.00", 100, Array.Empty<LinkedEscandalloItem>()));
        Assert.Equal(0, unusedMutation.CostHistoryEntriesCreated);
        Assert.Equal(0, unusedMutation.RecalculatedEscandallosCount);
        Assert.Empty(unusedMutation.UpdatedEscandallos);
    }

    [Fact]
    public void FrHist001_BlocksPhysicalDeleteAndSnapshotOverwriteEnforcingLogicalArchiveM1() {
        // Protección de integridad histórica (M1): baja lógica archived (409) e inmutabilidad (403)
        var deleteInUse = CascadeHistoryService.ArchiveCostOrService(new ArchiveResourceInput(
            "prod-cleasing-milk", true, "borrado_fisico_producto_en_uso"));
        var overwriteHistory = CascadeHistoryService.ArchiveCostOrService(new ArchiveResourceInput(
            "hist-snap-v1", true, "sobrescribir_snapshot_historico_v1"));
        Assert.Equal(409, deleteInUse.HttpStatus);
        Assert.Equal("PHYSICAL_DELETE_FORBIDDEN", deleteInUse.ErrorCode);
        Assert.Equal("archived", deleteInUse.NewStatus);
        Assert.Equal(403, overwriteHistory.HttpStatus);
        Assert.Equal("IMMUTABLE_HISTORY_VIOLATION", overwriteHistory.ErrorCode);
    }
}
