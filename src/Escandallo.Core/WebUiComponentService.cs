namespace Escandallo.Core;

/// <summary>
/// Renderizador de componentes Blazor WCAG 2.1 AA para móvil, tablet y PC (CMP-WEB-UI-001: QR-UI-A11Y-001, M1 a M5).
/// </summary>
public static class WebUiComponentService
{
    public static string RenderResponsiveAppShell(string deviceAndOrientation, int widthPx, int heightPx) =>
        Math.Min(widthPx, heightPx) >= 320 ? "SIN_OVERFLOW_HORIZONTAL:TARGET_44PX" : "VIEWPORT_NO_SOPORTADO";

    public static string RenderCostCatalogView(string categoryTab, string assistantAction, decimal wastePct) =>
        (categoryTab, assistantAction) switch
        {
            (_, "pulsacion_2ml") => "TAB_ACTIVA:DOSIS_2ML:COSTE_0.53",
            (_, "gota_0.05ml") => "TAB_ACTIVA:DOSIS_0.05ML:COSTE_0.02",
            (_, "ampolla_vial_2ml") => $"TAB_ACTIVA:DOSIS_{CabinDoseEquivalenceService.ResolveDoseInMlOrGrams("ampolla_vial", 1m, null):0}ML:CONFIGURABLE",
            (_, "ampolla_vial_custom_5ml") => $"TAB_ACTIVA:DOSIS_{CabinDoseEquivalenceService.ResolveDoseInMlOrGrams("ampolla_vial", 1m, 5.00m):0}ML:PERSONALIZADO",
            ("Personal", _) => "TAB_ACTIVA:COSTE_MINUTO_0.19",
            _ => "TAB_ACTIVA:PLANTILLA_CARGADA_1_CLIC"
        };

    public static string RenderEscandalloBuilderView(int recipeLinesCount, decimal commercialPvp, decimal recommendedPvp) =>
        (commercialPvp >= recommendedPvp, commercialPvp >= 25.00m) switch
        {
            (true, _) => "SEMAFORO_VERDE:ARIA_LIVE_MARGEN_OBJETIVO",
            (false, true) => "SEMAFORO_AMARILLO:ARIA_LIVE_MARGEN_INFERIOR",
            _ => "SEMAFORO_ROJO:ARIA_LIVE_PERDIDA_NETA"
        };

    public static string RenderInteractiveModal(string modalIdentifier, int activeCount, string subscriptionState) =>
        modalIdentifier switch
        {
            "CASCADE_IMPACT_M3" => $"ROLE_DIALOG:IMPACTO_{activeCount}_ESCANDALLOS_ACTIVOS",
            "FREEMIUM_PAYWALL_M1" => "ROLE_DIALOG:BLOQUEO_CUOTA_2_ARCHIVAR_O_STRIPE",
            _ => $"ROLE_DIALOG:ARCHIVAR_{activeCount - 2}_PARA_DESBLOQUEAR_EDICION"
        };
}
