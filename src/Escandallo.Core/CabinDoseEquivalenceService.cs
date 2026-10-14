namespace Escandallo.Core;

/// <summary>
/// Servicio de equivalencias configurables de dosis en cabina (M5: gota, pulsación, cucharadita, ampolla/vial monodosis).
/// </summary>
public static class CabinDoseEquivalenceService
{
    private static readonly Dictionary<string, decimal> DefaultFactorsMl = new(StringComparer.OrdinalIgnoreCase)
    {
        ["gota"] = 0.05m,
        ["pulsacion"] = 2.00m,
        ["cucharadita"] = 5.00m,
        ["ampolla_vial"] = 2.00m
    };

    public static decimal ResolveDoseInMlOrGrams(string doseUnitKey, decimal unitsCount, decimal? userConfiguredFactor)
    {
        decimal effectiveFactor = userConfiguredFactor ?? DefaultFactorsMl.GetValueOrDefault(doseUnitKey, 1.00m);
        return Math.Round(unitsCount * effectiveFactor, 4, MidpointRounding.AwayFromZero);
    }
}
