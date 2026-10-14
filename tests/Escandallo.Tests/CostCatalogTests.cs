using Xunit;

namespace Escandallo.Tests;

/// <summary>
/// Suite de pruebas unitarias y BDD en estado ROJO (FAILING) para FR-COST-001 y BR-CALC-FORMULAS-001.
/// Verifica el alta de catálogo, plantilla semilla M4, cálculo unitario en System.Decimal y merma M5.
/// </summary>
public sealed class CostCatalogTests
{
    /// <summary>
    /// Escenario Nominal (FR-COST-001): Inicialización con plantilla semilla y tasa por minuto de Gastos Generales.
    /// </summary>
    [Fact]
    [Trait("Requirement", "FR-COST-001")]
    [Trait("BusinessRule", "BR-CALC-FORMULAS-001")]
    public void CalculateOverheadMinuteRate_NominalSeedCatalog_ReturnsExactDecimalRate()
    {
        // Arrange: Local (335 EUR) + Consumos (100 EUR) + Consumibles (440 EUR) = 875 EUR / 10560 min
        decimal totalMonthlyOverhead = 335.00m + 100.00m + 440.00m;
        int monthlyWorkingMinutes = 10560;
        decimal expectedMinuteRate = 0.08285985m;

        // Act: Invoca el contrato aún no implementado en src/ (Estado TDD ROJO)
        decimal actualRate = InvokeUnimplementedOverheadCalculator(totalMonthlyOverhead, monthlyWorkingMinutes);

        // Assert
        Assert.Equal(expectedMinuteRate, actualRate);
    }

    /// <summary>
    /// Análisis de Frontera (FR-COST-001): Cálculo unitario de productos (ml, g, ud con merma M5) y personal.
    /// </summary>
    [Theory]
    [InlineData("Cleasing Milk", "producto", 500, 34.51, 0.00, 1.0, 34.51, 0.06902)]
    [InlineData("Crema Reafirmante", "producto", 250, 50.00, 0.00, 1.0, 50.00, 0.20)]
    [InlineData("Colagen Mask", "producto", 12, 86.52, 0.00, 1.0, 86.52, 7.21)]
    [InlineData("Tecnico", "personal", 10560, 1100.00, 0.20, 1.5, 1980.00, 0.1875)]
    [InlineData("Enfermero", "personal", 10560, 1800.00, 0.22, 1.5, 3294.00, 0.31193182)]
    [InlineData("Doctor", "personal", 10560, 3000.00, 0.25, 1.5, 5625.00, 0.53267045)]
    [Trait("Requirement", "FR-COST-001")]
    public void CalculateUnitCost_BoundaryCatalogItems_ReturnsExpectedCompanyAndUnitCost(
        string name,
        string itemType,
        decimal quantity,
        decimal baseAmount,
        decimal retentionPct,
        decimal companyCoeff,
        decimal expectedCompanyCost,
        decimal expectedUnitCost)
    {
        // Act: Debe fallar en ROJO hasta que agent-developer implemente CMP-COST-ENGINE-001
        (decimal actualCompanyCost, decimal actualUnitCost) = InvokeUnimplementedUnitCost(
            name, itemType, quantity, baseAmount, retentionPct, companyCoeff);

        // Assert
        Assert.Equal(expectedCompanyCost, actualCompanyCost);
        Assert.Equal(expectedUnitCost, actualUnitCost);
    }

    /// <summary>
    /// Escenarios Fuera de Rango / Inválidos (FR-COST-001): Rechazo HTTP 422 ante divisores cero o negativos.
    /// </summary>
    [Theory]
    [InlineData("producto", 0, 34.51, 0.05, 422, "CANTIDAD_DIVISOR_CERO")]
    [InlineData("producto", -100, 34.51, 0.05, 422, "CANTIDAD_NEGATIVA_INVALIDA")]
    [InlineData("producto", 500, -15.00, 0.05, 422, "IMPORTE_NEGATIVO_INVALIDO")]
    [InlineData("producto", 500, 99999999999.99, 0.05, 422, "DESBORDAMIENTO_DECIMAL_128BIT")]
    [InlineData("producto", 250, 50.00, 1.05, 422, "MERMA_FUERA_DE_RANGO_0_A_99_PCT")]
    [InlineData("personal", 0, 1100.00, 0.00, 422, "MINUTOS_MENSUALES_DIVISOR_CERO")]
    [InlineData("gasto_general", 0, 875.00, 0.00, 422, "MINUTOS_MENSUALES_DIVISOR_CERO")]
    [Trait("Requirement", "FR-COST-001")]
    [Trait("Requirement", "SEC-REQ-XLSX-001")]
    public void ValidateCostInput_OutOfRangeValues_ReturnsHttp422AndReason(
        string itemType,
        decimal invalidQty,
        decimal invalidAmount,
        decimal invalidWaste,
        int expectedStatus,
        string expectedReason)
    {
        // Act
        (int status, string reason) = InvokeUnimplementedCostValidation(
            itemType, invalidQty, invalidAmount, invalidWaste);

        // Assert
        Assert.Equal(expectedStatus, status);
        Assert.Equal(expectedReason, reason);
    }

    private static decimal InvokeUnimplementedOverheadCalculator(decimal totalMonthly, int minutes) { return Escandallo.Core.CostCatalogService.CalculateOverheadMinuteRate(totalMonthly, minutes); }

    private static (decimal CompanyCost, decimal UnitCost) InvokeUnimplementedUnitCost(
        string name, string type, decimal qty, decimal amount, decimal retention, decimal coeff) { return Escandallo.Core.CostCatalogService.CalculateUnitCost(name, type, qty, amount, retention, coeff); }

    private static (int Status, string Reason) InvokeUnimplementedCostValidation(
        string type, decimal qty, decimal amount, decimal waste) { return Escandallo.Core.CostCatalogService.ValidateCostInput(type, qty, amount, waste); }
}
