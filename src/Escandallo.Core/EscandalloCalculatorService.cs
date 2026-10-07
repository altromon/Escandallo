namespace Escandallo.Core;

/// <summary>
/// Motor de cálculo de escandallo, beneficio neto, PVP y Semáforo M2 (CMP-COST-ENGINE-001: FR-CALC-001, BR-CALC-FORMULAS-001).
/// </summary>
public static class EscandalloCalculatorService
{
    public static decimal CalculateServicePvpWithVat(
        int durationMinutes, decimal overheadRate, decimal staffRate, decimal productsCost, decimal margin, decimal vat)
    {
        decimal overheadAllocated = durationMinutes * overheadRate;
        decimal staffCost = durationMinutes * staffRate;
        decimal totalEscandallo = overheadAllocated + staffCost + productsCost;
        decimal netProfit = totalEscandallo * margin;
        decimal priceExVat = totalEscandallo + netProfit;
        return Math.Round(priceExVat * (1m + vat), 8, MidpointRounding.AwayFromZero);
    }

    public static (decimal PriceExVat, decimal NetProfit, string Semaphore) EvaluateMarginSemaphore(
        decimal totalCost, decimal targetMargin, decimal vat, decimal commercialPvp)
    {
        decimal priceExVat = Math.Round(commercialPvp / (1m + vat), 2, MidpointRounding.AwayFromZero);
        decimal netProfit = Math.Round(priceExVat - totalCost, 2, MidpointRounding.AwayFromZero);
        decimal targetProfit = Math.Round(totalCost * targetMargin, 2, MidpointRounding.AwayFromZero);
        string semaphore = netProfit <= 0m ? "CRITICO" : (netProfit < targetProfit ? "ALERTA" : "OPTIMO");
        return (priceExVat, netProfit, semaphore);
    }

    public static (int HttpStatus, string ValidationCode) ValidateRecipeParameters(
        int duration, decimal dose, decimal margin, decimal vat)
    {
        if (duration <= 0) return (422, "DURACION_SERVICIO_INVALIDA");
        if (dose < 0m) return (422, "DOSIS_PRODUCTO_NEGATIVA");
        if (margin < 0m || margin > 10m) return (422, "MARGEN_BENEFICIO_FUERA_RANGO");
        if (vat < 0m) return (422, "IVA_NEGATIVO_INVALIDO");
        return (200, $"VALIDO (FR-CALC-001): dur={duration}");
    }
}
