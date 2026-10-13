namespace Escandallo.Core;

/// <summary>
/// Motor de catálogo de costes en C# System.Decimal (CMP-COST-ENGINE-001: FR-COST-001, BR-CALC-FORMULAS-001).
/// </summary>
public static class CostCatalogService
{
    public static decimal CalculateOverheadMinuteRate(decimal totalMonthlyOverhead, int monthlyWorkingMinutes)
    {
        return Math.Round(totalMonthlyOverhead / monthlyWorkingMinutes, 8, MidpointRounding.AwayFromZero);
    }

    public static (decimal CompanyCost, decimal UnitCost) CalculateUnitCost(
        string name, string itemType, decimal quantity, decimal baseAmount, decimal retentionPct, decimal companyCoeff)
    {
        decimal companyCost = itemType == "personal"
            ? Math.Round(((baseAmount * retentionPct) + baseAmount) * companyCoeff, 2, MidpointRounding.AwayFromZero)
            : Math.Round(baseAmount * companyCoeff, 2, MidpointRounding.AwayFromZero);
        decimal unitCost = Math.Round(companyCost / quantity, 8, MidpointRounding.AwayFromZero);
        return (companyCost, unitCost);
    }

    public static (int Status, string Reason) ValidateCostInput(
        string itemType, decimal quantity, decimal amount, decimal wastePct)
    {
        if (quantity == 0m) return (422, itemType == "producto" ? "CANTIDAD_DIVISOR_CERO" : "MINUTOS_MENSUALES_DIVISOR_CERO");
        if (quantity < 0m) return (422, "CANTIDAD_NEGATIVA_INVALIDA");
        if (amount < 0m) return (422, "IMPORTE_NEGATIVO_INVALIDO");
        if (amount > 1_000_000m) return (422, "DESBORDAMIENTO_DECIMAL_128BIT");
        if (wastePct < 0m || wastePct >= 1.00m) return (422, "MERMA_FUERA_DE_RANGO_0_A_99_PCT");
        return (200, $"VALIDO (FR-COST-001): {itemType}");
    }
}
