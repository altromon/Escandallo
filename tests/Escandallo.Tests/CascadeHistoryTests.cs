using Xunit;

namespace Escandallo.Tests;

/// <summary>
/// Suite de pruebas unitarias y BDD en estado ROJO (FAILING) para FR-HIST-001 y BR-RECALC-HISTORY-001.
/// Verifica la previsualización de impacto M3, el histórico condicional y la baja lógica M1.
/// </summary>
public sealed class CascadeHistoryTests
{
    /// <summary>
    /// Escenario Nominal (FR-HIST-001): Previsualización de impacto M3, recálculo automático y baja lógica M1.
    /// </summary>
    [Fact]
    [Trait("Requirement", "FR-HIST-001")]
    [Trait("BusinessRule", "BR-RECALC-HISTORY-001")]
    public void UpdateCostWithCascade_ProductInUse_CreatesSnapshotsAndRecalculatesEscandallo()
    {
        // Arrange: "Peeling Facial" usa 5 ml de "Lifting Peeling" (sube de 100 EUR a 120 EUR por 100 ml)
        decimal previousPackagePrice = 100.00m;
        decimal newPackagePrice = 120.00m;
        decimal previousEscandalloTotal = 20.00m;

        // Act: Estado ROJO hasta implementar la transacción de cascada en CMP-COST-ENGINE-001
        (int impactedCount, decimal updatedTotal, string deleteStatus) = InvokeUnimplementedCascadeUpdate(
            previousPackagePrice, newPackagePrice, previousEscandalloTotal);

        // Assert
        Assert.Equal(1, impactedCount);
        Assert.Equal(21.00m, updatedTotal);
        Assert.Equal("archived", deleteStatus);
    }

    /// <summary>
    /// Análisis de Frontera (FR-HIST-001): Sólo se guarda histórico del coste si al menos 1 escandallo lo utiliza.
    /// </summary>
    [Theory]
    [InlineData("producto", 0, 0, 0)]
    [InlineData("personal", 0, 0, 0)]
    [InlineData("producto", 1, 1, 1)]
    [InlineData("gasto_general", 2, 1, 2)]
    [Trait("Requirement", "FR-HIST-001")]
    public void UpdateCost_BoundaryLinkedEscandallos_CreatesHistoryOnlyWhenCostIsInUse(
        string costType,
        int linkedEscandallosCount,
        int expectedCostHistoryEntries,
        int expectedRecalculatedEscandallos)
    {
        // Act: Estado ROJO
        (int costHistoryEntries, int recalculatedCount) = InvokeUnimplementedConditionalHistory(
            costType, linkedEscandallosCount);

        // Assert
        Assert.Equal(expectedCostHistoryEntries, costHistoryEntries);
        Assert.Equal(expectedRecalculatedEscandallos, recalculatedCount);
    }

    /// <summary>
    /// Escenarios Fuera de Rango / Inválidos (FR-HIST-001): Bloqueo de borrado físico y protección de inmutabilidad.
    /// </summary>
    [Theory]
    [InlineData(2, 3, "DELETE_FISICO_COSTE_EN_USO", "BLOQUEADO_SOLO_ARCHIVADO_PERMITIDO", 409)]
    [InlineData(1, 1, "DELETE_SNAPSHOT_HISTORICO", "HISTORICO_INMUTABLE_PROTEGIDO", 403)]
    [InlineData(2, 3, "UPDATE_COSTE_SIN_CAMBIO_DE_IMPORTE", "IDEMPOTENTE_SIN_NUEVA_VERSION", 200)]
    [Trait("Requirement", "FR-HIST-001")]
    public void EnforceHistoryImmutability_RestrictedOperations_ReturnsExpectedGuardResponse(
        int previousVersions,
        int activeEscandallos,
        string restrictedOperation,
        string expectedResponseState,
        int expectedHttpStatus)
    {
        // Act: Estado ROJO
        (string responseState, int httpStatus) = InvokeUnimplementedHistoryGuard(
            previousVersions, activeEscandallos, restrictedOperation);

        // Assert
        Assert.Equal(expectedResponseState, responseState);
        Assert.Equal(expectedHttpStatus, httpStatus);
    }

    private static (int Impacted, decimal UpdatedTotal, string DeleteStatus) InvokeUnimplementedCascadeUpdate(decimal oldPrice, decimal newPrice, decimal currentTotal) { return Escandallo.Core.CascadeHistoryService.UpdateCostWithCascade(oldPrice, newPrice, currentTotal); }

    private static (int CostHistoryEntries, int RecalculatedCount) InvokeUnimplementedConditionalHistory(string costType, int linkedCount) { return Escandallo.Core.CascadeHistoryService.EvaluateConditionalHistory(costType, linkedCount); }

    private static (string State, int Status) InvokeUnimplementedHistoryGuard(int versions, int activeCount, string operation) { return Escandallo.Core.CascadeHistoryService.EnforceHistoryImmutability(versions, activeCount, operation); }
}
