namespace Escandallo.Core.Modules.CostEngine;

public sealed record EscandalloProductLine(
    string Name,
    string DoseQuantity,
    string UnitPriceEur,
    string WastagePct);

public sealed record EscandalloServiceInput(
    string ServiceName,
    int DurationMinutes,
    string OverheadRatePerMinuteEur,
    string StaffMinuteRateEur,
    string TargetProfitPct,
    string VatPct,
    IReadOnlyList<EscandalloProductLine> ProductLines);

public sealed record EscandalloServiceResult(
    int HttpStatus,
    string? ErrorCode = null,
    string? AllocatedOverheadEur = null,
    string? StaffCostEur = null,
    string? ProductsCostEur = null,
    string? TotalEscandalloCostEur = null,
    string? NetProfitEur = null,
    string? PriceWithProfitExclVatEur = null,
    string? CalculatedPvpInclVatEur = null);

public sealed record MarginSemaphoreInput(
    string TotalEscandalloCostEur,
    string TargetProfitPct,
    string VatPct,
    string FixedCommercialPvpEur);

public sealed record MarginSemaphoreResult(
    string RealPriceExclVatEur,
    string RealNetProfitEur,
    string SemaphoreStatus);

public static class EscandalloCalculator
{
    private static EscandalloServiceResult? ValidateServiceInput(EscandalloServiceInput input) {
        // Valida duración positiva, límite de 100 líneas de receta y ausencia de dosis negativas
        if (input.DurationMinutes == 0) return new EscandalloServiceResult(422, "INVALID_SERVICE_DURATION");
        if (input.DurationMinutes < 0) return new EscandalloServiceResult(422, "NEGATIVE_SERVICE_DURATION");
        if (input.ProductLines.Count > 100) return new EscandalloServiceResult(422, "MAX_RECIPE_LINES_EXCEEDED");
        var hasNegativeDose = input.ProductLines.Any(line => CostCatalogCalculator.ParseDecimal(line.DoseQuantity) < 0m);
        if (hasNegativeDose) return new EscandalloServiceResult(422, "NEGATIVE_PRODUCT_DOSE");
        return null;
    }

    private static decimal SumRecipeLines(IReadOnlyList<EscandalloProductLine> lines) {
        // Suma el coste de cada línea de producto aplicando su porcentaje de merma (M5)
        var total = 0m;
        foreach (var line in lines)
        {
            var dose = CostCatalogCalculator.ParseDecimal(line.DoseQuantity);
            var unitPrice = CostCatalogCalculator.ParseDecimal(line.UnitPriceEur);
            var wastage = CostCatalogCalculator.ParseDecimal(line.WastagePct);
            total += Math.Round(dose * unitPrice * (1m + wastage), 8, MidpointRounding.AwayFromZero);
        }
        return total;
    }

    public static EscandalloServiceResult CalculateEscandalloService(EscandalloServiceInput input) {
        // Calcula el desglose completo de Tabla5 y Tabla57 con System.Decimal de 128 bits
        var err = ValidateServiceInput(input);
        if (err is not null)
        {
            return err;
        }
        var overhead = Math.Round(input.DurationMinutes * CostCatalogCalculator.ParseDecimal(input.OverheadRatePerMinuteEur), 8, MidpointRounding.AwayFromZero);
        var staff = Math.Round(input.DurationMinutes * CostCatalogCalculator.ParseDecimal(input.StaffMinuteRateEur), 8, MidpointRounding.AwayFromZero);
        var products = SumRecipeLines(input.ProductLines);
        var totalCost = overhead + staff + products;
        var netProfit = Math.Round(totalCost * CostCatalogCalculator.ParseDecimal(input.TargetProfitPct), 8, MidpointRounding.AwayFromZero);
        var priceExclVat = totalCost + netProfit;
        var pvpInclVat = Math.Round(priceExclVat * (1m + CostCatalogCalculator.ParseDecimal(input.VatPct)), 8, MidpointRounding.AwayFromZero);
        return new EscandalloServiceResult(
            200,
            null,
            CostCatalogCalculator.FormatDecimal(overhead, 2),
            CostCatalogCalculator.FormatDecimal(staff, 2),
            CostCatalogCalculator.FormatDecimal(products, 2),
            CostCatalogCalculator.FormatDecimal(totalCost, 2),
            CostCatalogCalculator.FormatDecimal(netProfit, 2),
            CostCatalogCalculator.FormatDecimal(priceExclVat, 2),
            CostCatalogCalculator.FormatDecimal(pvpInclVat, 2));
    }

    public static MarginSemaphoreResult EvaluateMarginSemaphore(MarginSemaphoreInput input) {
        // Evalúa el beneficio neto real y el Semáforo de Erosión de Margen frente al PVP Comercial (M2)
        var pvp = CostCatalogCalculator.ParseDecimal(input.FixedCommercialPvpEur);
        var vat = CostCatalogCalculator.ParseDecimal(input.VatPct);
        var totalCost = CostCatalogCalculator.ParseDecimal(input.TotalEscandalloCostEur);
        var targetPct = CostCatalogCalculator.ParseDecimal(input.TargetProfitPct);
        var realExclVat = Math.Round(pvp / (1m + vat), 8, MidpointRounding.AwayFromZero);
        var realNetProfit = realExclVat - totalCost;
        var targetProfit = Math.Round(totalCost * targetPct, 8, MidpointRounding.AwayFromZero);
        var status = "OPTIMO";
        if (realNetProfit <= 0m)
        {
            status = "CRITICO";
        }
        else if (realNetProfit < targetProfit)
        {
            status = "ALERTA";
        }
        return new MarginSemaphoreResult(
            CostCatalogCalculator.FormatDecimal(realExclVat, 2),
            CostCatalogCalculator.FormatDecimal(realNetProfit, 2),
            status);
    }
}
