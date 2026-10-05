using Escandallo.Core.Modules.CostEngine;
using Xunit;

namespace Escandallo.UnitTests;

/// <summary>
/// Suite BDD/TDD en C# xUnit para CMP-COST-ENGINE-001 — Motor de Escandallos y Semáforo de Margen (TSK-003).
/// Requisitos cubiertos: FR-CALC-001, BR-CALC-FORMULAS-001.
/// </summary>
public sealed class EscandalloCalcTests
{
    [Fact]
    public void FrCalc001_CalculatesExactFacialHygieneBreakdownMatchingExcelTabla5AndTabla57() {
        // Caso canónico de Tabla5 y Tabla57 del Excel Escandallo.xlsx
        var lines = new[]
        {
            new EscandalloProductLine("Cleasing Milk", "10", "0.06902", "0.00"),
            new EscandalloProductLine("Colagen Mask", "1", "7.21", "0.00")
        };
        var result = EscandalloCalculator.CalculateEscandalloService(new EscandalloServiceInput(
            "Higiene Facial", 60, "0.08285985", "0.1875", "0.50", "0.21", lines));
        Assert.Equal("4.971591", result.AllocatedOverheadEur);
        Assert.Equal("11.25", result.StaffCostEur);
        Assert.Equal("7.9002", result.ProductsCostEur);
        Assert.Equal("24.121791", result.TotalEscandalloCostEur);
        Assert.Equal("12.0608955", result.NetProfitEur);
        Assert.Equal("36.1826865", result.PriceWithProfitExclVatEur);
        Assert.Equal("43.78105067", result.CalculatedPvpInclVatEur);
    }

    [Fact]
    public void FrCalc001_EvaluatesOptimoAndAlertaMarginSemaphoreStatesM2() {
        // Semáforo de Erosión de Margen (M2): estados OPTIMO y ALERTA frente a PVP Comercial
        var optimo = EscandalloCalculator.EvaluateMarginSemaphore(
            new MarginSemaphoreInput("20.00", "0.50", "0.21", "36.30"));
        var alerta = EscandalloCalculator.EvaluateMarginSemaphore(
            new MarginSemaphoreInput("24.121791", "0.50", "0.21", "36.30"));
        Assert.Equal("30.00", optimo.RealPriceExclVatEur);
        Assert.Equal("10.00", optimo.RealNetProfitEur);
        Assert.Equal("OPTIMO", optimo.SemaphoreStatus);
        Assert.Equal("5.878209", alerta.RealNetProfitEur);
        Assert.Equal("ALERTA", alerta.SemaphoreStatus);
    }

    [Fact]
    public void FrCalc001_EvaluatesCriticoSemaphoreAtZeroAndNegativeProfitBoundaries() {
        // Frontera exacta de beneficio 0.00 EUR y pérdida neta (-1.00 EUR) como CRITICO
        var zeroBoundary = EscandalloCalculator.EvaluateMarginSemaphore(
            new MarginSemaphoreInput("30.00", "0.50", "0.21", "36.30"));
        var negativeLoss = EscandalloCalculator.EvaluateMarginSemaphore(
            new MarginSemaphoreInput("31.00", "0.50", "0.21", "36.30"));
        Assert.Equal("0.00", zeroBoundary.RealNetProfitEur);
        Assert.Equal("CRITICO", zeroBoundary.SemaphoreStatus);
        Assert.Equal("-1.00", negativeLoss.RealNetProfitEur);
        Assert.Equal("CRITICO", negativeLoss.SemaphoreStatus);
    }

    [Fact]
    public void FrCalc001_RejectsInvalidDurationNegativeDoseAndExcessiveRecipeLines() {
        // Rechazo con HTTP 422 de duraciones <= 0, dosis negativas y recetas > 100 líneas
        var zeroDuration = EscandalloCalculator.CalculateEscandalloService(new EscandalloServiceInput(
            "Facial Cero", 0, "0.08285985", "0.1875", "0.50", "0.21", Array.Empty<EscandalloProductLine>()));
        var overLimitLines = Enumerable.Range(0, 101)
            .Select(_ => new EscandalloProductLine("Prod", "1", "1.00", "0.00"))
            .ToArray();
        var massiveRecipe = EscandalloCalculator.CalculateEscandalloService(new EscandalloServiceInput(
            "Receta Masiva", 60, "0.08285985", "0.1875", "0.50", "0.21", overLimitLines));
        Assert.Equal(422, zeroDuration.HttpStatus);
        Assert.Equal("INVALID_SERVICE_DURATION", zeroDuration.ErrorCode);
        Assert.Equal(422, massiveRecipe.HttpStatus);
        Assert.Equal("MAX_RECIPE_LINES_EXCEEDED", massiveRecipe.ErrorCode);
    }
}
