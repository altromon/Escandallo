using System.Globalization;

namespace Escandallo.Core.Modules.CostEngine;

public sealed record OverheadExpenseItem(string Name, string MonthlyCostEur);

public sealed record OverheadRateInput(int MonthlyMinutesCapacity, IReadOnlyList<OverheadExpenseItem> Expenses);

public sealed record OverheadRateResult(string TotalMonthlyOverheadEur, string OverheadRatePerMinuteEur);

public sealed record ProductUnitCostInput(
    string Name,
    int PackageQuantity,
    string Unit,
    string PackagePriceEur,
    string WastagePct);

public sealed record ProductUnitCostResult(
    int HttpStatus,
    string? ErrorCode = null,
    string? UnitPriceEur = null,
    string? EffectiveUnitPriceWithWastageEur = null);

public sealed record StaffUnitCostInput(
    string RoleName,
    int MonthlyMinutes,
    string NetSalaryEur,
    string RetentionPct,
    string CompanyCoefficient);

public sealed record StaffUnitCostResult(
    int HttpStatus,
    string? ErrorCode = null,
    string? CompanyMonthlyCostEur = null,
    string? MinuteRateEur = null);

public static class CostCatalogCalculator
{
    public static decimal ParseDecimal(string raw) {
        // Parsea cadenas numéricas en base 10 con cultura invariante hacia System.Decimal de 128 bits
        return decimal.Parse(raw.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    public static string FormatDecimal(decimal value, int minDecimals = 2) {
        // Redondea a 8 decimales (AwayFromZero) y conserva al menos minDecimals dígitos fraccionarios
        var rounded = Math.Round(value, 8, MidpointRounding.AwayFromZero);
        var formatted = rounded.ToString("0.00000000", CultureInfo.InvariantCulture);
        var dotIndex = formatted.IndexOf('.');
        var intPart = formatted[..dotIndex];
        var fracPart = formatted[(dotIndex + 1)..].TrimEnd('0');
        if (fracPart.Length < minDecimals)
        {
            fracPart = fracPart.PadRight(minDecimals, '0');
        }
        return $"{intPart}.{fracPart}";
    }

    public static OverheadRateResult CalculateOverheadRatePerMinute(OverheadRateInput input) {
        // Calcula la suma mensual de Tabla1 y la tasa por minuto de gastos generales
        var total = input.Expenses.Sum(item => ParseDecimal(item.MonthlyCostEur));
        var rate = Math.Round(total / input.MonthlyMinutesCapacity, 8, MidpointRounding.AwayFromZero);
        return new OverheadRateResult(FormatDecimal(total, 2), FormatDecimal(rate, 2));
    }

    private static ProductUnitCostResult? ValidateProductInput(ProductUnitCostInput input) {
        // Verifica invariantes de dominio sobre nombre, cantidad, precio y porcentaje de merma (M5)
        if (string.IsNullOrWhiteSpace(input.Name)) return new ProductUnitCostResult(400, "EMPTY_ITEM_NAME");
        if (input.PackageQuantity == 0) return new ProductUnitCostResult(422, "INVALID_ZERO_QUANTITY");
        if (input.PackageQuantity < 0) return new ProductUnitCostResult(422, "NEGATIVE_QUANTITY");
        if (!decimal.TryParse(input.PackagePriceEur, NumberStyles.Float, CultureInfo.InvariantCulture, out var price) || price < 0m)
        {
            return new ProductUnitCostResult(422, "NEGATIVE_PRICE");
        }
        var wastage = ParseDecimal(input.WastagePct);
        if (wastage < 0m || wastage > 1m) return new ProductUnitCostResult(422, "WASTAGE_OUT_OF_RANGE");
        return null;
    }

    public static ProductUnitCostResult CalculateProductUnitCost(ProductUnitCostInput input) {
        // Calcula el precio unitario por ml/g/ud y el coste efectivo incluyendo % Merma (Tabla2 + M5)
        var error = ValidateProductInput(input);
        if (error is not null)
        {
            return error;
        }
        var price = ParseDecimal(input.PackagePriceEur);
        var wastage = ParseDecimal(input.WastagePct);
        var unitPrice = Math.Round(price / input.PackageQuantity, 8, MidpointRounding.AwayFromZero);
        var effectivePrice = Math.Round(unitPrice * (1m + wastage), 8, MidpointRounding.AwayFromZero);
        return new ProductUnitCostResult(200, null, FormatDecimal(unitPrice, 2), FormatDecimal(effectivePrice, 2));
    }

    public static StaffUnitCostResult CalculateStaffUnitCost(StaffUnitCostInput input) {
        // Calcula el coste empresa mensual y por minuto de cada perfil profesional (Tabla3 y Tabla4)
        if (string.IsNullOrWhiteSpace(input.RoleName))
        {
            return new StaffUnitCostResult(400, "EMPTY_ITEM_NAME");
        }
        if (input.MonthlyMinutes <= 0)
        {
            return new StaffUnitCostResult(422, "DIVISION_BY_ZERO_GUARD");
        }
        var net = ParseDecimal(input.NetSalaryEur);
        var retention = ParseDecimal(input.RetentionPct);
        var coef = ParseDecimal(input.CompanyCoefficient);
        var companyMonthly = Math.Round(((net * retention) + net) * coef, 8, MidpointRounding.AwayFromZero);
        var minuteRate = Math.Round(companyMonthly / input.MonthlyMinutes, 8, MidpointRounding.AwayFromZero);
        return new StaffUnitCostResult(200, null, FormatDecimal(companyMonthly, 2), FormatDecimal(minuteRate, 2));
    }
}
