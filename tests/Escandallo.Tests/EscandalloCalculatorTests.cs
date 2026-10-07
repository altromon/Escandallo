using Xunit;

namespace Escandallo.Tests;

/// <summary>
/// Suite de pruebas unitarias y BDD en estado ROJO (FAILING) para FR-CALC-001 y BR-CALC-FORMULAS-001.
/// Verifica el cálculo de Escandallo Total, Beneficio Neto, PVP Calculado, PVP Comercial Fijado y Semáforo M2.
/// </summary>
public sealed class EscandalloCalculatorTests
{
    /// <summary>
    /// Escenario Nominal (FR-CALC-001): Desglose completo del servicio "Higiene Facial" de 60 minutos.
    /// </summary>
    [Fact]
    [Trait("Requirement", "FR-CALC-001")]
    [Trait("BusinessRule", "BR-CALC-FORMULAS-001")]
    public void CalculateServiceEscandallo_NominalHigieneFacial_MatchesExcelCanonicalValues()
    {
        // Arrange: Datos reales de CalculadoraEscandallo.xlsx (Tabla5 / Tabla57)
        int durationMinutes = 60;
        decimal overheadRatePerMin = 0.08285985m;
        decimal staffRatePerMin = 0.1875m;
        decimal productsTotalCost = (10m * 0.06902m) + (1m * 7.21m);

        // Act: Estado ROJO hasta implementar IEscandalloCalculatorService en src/
        decimal actualPvpWithVat = InvokeUnimplementedServiceCalculation(
            durationMinutes, overheadRatePerMin, staffRatePerMin, productsTotalCost, 0.50m, 0.21m);

        // Assert
        Assert.Equal(43.78105067m, actualPvpWithVat);
    }

    /// <summary>
    /// Análisis de Frontera (FR-CALC-001 / M2): Evaluación del Semáforo de Erosión de Margen frente al PVP Comercial Fijado.
    /// </summary>
    [Theory]
    [InlineData(20.00, 0.50, 0.21, 36.30, 30.00, 10.00, "OPTIMO")]
    [InlineData(25.00, 0.50, 0.21, 36.30, 30.00, 5.00, "ALERTA")]
    [InlineData(32.00, 0.50, 0.21, 36.30, 30.00, -2.00, "CRITICO")]
    [Trait("Requirement", "FR-CALC-001")]
    public void EvaluateMarginSemaphore_BoundaryCommercialPvp_ReturnsExpectedSemaphoreState(
        decimal totalEscandalloCost,
        decimal targetMarginPct,
        decimal vatPct,
        decimal commercialPvp,
        decimal expectedPriceExVat,
        decimal expectedNetProfit,
        string expectedSemaphore)
    {
        // Act: Estado ROJO
        (decimal priceExVat, decimal netProfit, string semaphore) = InvokeUnimplementedSemaphoreEvaluation(
            totalEscandalloCost, targetMarginPct, vatPct, commercialPvp);

        // Assert
        Assert.Equal(expectedPriceExVat, priceExVat);
        Assert.Equal(expectedNetProfit, netProfit);
        Assert.Equal(expectedSemaphore, semaphore);
    }

    /// <summary>
    /// Escenarios Fuera de Rango / Inválidos (FR-CALC-001): Rechazo HTTP 422 ante duraciones, dosis o márgenes inválidos.
    /// </summary>
    [Theory]
    [InlineData(0, 10.0, 0.50, 0.21, 422, "DURACION_SERVICIO_INVALIDA")]
    [InlineData(-30, 10.0, 0.50, 0.21, 422, "DURACION_SERVICIO_INVALIDA")]
    [InlineData(60, -5.0, 0.50, 0.21, 422, "DOSIS_PRODUCTO_NEGATIVA")]
    [InlineData(60, 10.0, -1.20, 0.21, 422, "MARGEN_BENEFICIO_FUERA_RANGO")]
    [InlineData(60, 10.0, 0.50, -0.10, 422, "IVA_NEGATIVO_INVALIDO")]
    [Trait("Requirement", "FR-CALC-001")]
    public void ValidateEscandalloRecipe_OutOfRangeParameters_ReturnsHttp422AndValidationCode(
        int durationMinutes,
        decimal productDose,
        decimal marginPct,
        decimal vatPct,
        int expectedHttpStatus,
        string expectedValidationCode)
    {
        // Act: Estado ROJO
        (int httpStatus, string validationCode) = InvokeUnimplementedRecipeValidation(
            durationMinutes, productDose, marginPct, vatPct);

        // Assert
        Assert.Equal(expectedHttpStatus, httpStatus);
        Assert.Equal(expectedValidationCode, validationCode);
    }

    private static decimal InvokeUnimplementedServiceCalculation(int duration, decimal overheadRate, decimal staffRate, decimal productsCost, decimal margin, decimal vat) { return Escandallo.Core.EscandalloCalculatorService.CalculateServicePvpWithVat(duration, overheadRate, staffRate, productsCost, margin, vat); }

    private static (decimal PriceExVat, decimal NetProfit, string Semaphore) InvokeUnimplementedSemaphoreEvaluation(decimal totalCost, decimal targetMargin, decimal vat, decimal commercialPvp) { return Escandallo.Core.EscandalloCalculatorService.EvaluateMarginSemaphore(totalCost, targetMargin, vat, commercialPvp); }

    private static (int HttpStatus, string ValidationCode) InvokeUnimplementedRecipeValidation(int duration, decimal dose, decimal margin, decimal vat) { return Escandallo.Core.EscandalloCalculatorService.ValidateRecipeParameters(duration, dose, margin, vat); }
}
