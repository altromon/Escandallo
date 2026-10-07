using Xunit;

namespace Escandallo.Tests;

/// <summary>
/// Suite de pruebas de componentes Blazor (bUnit / WAI-ARIA) en estado ROJO (FAILING) para CMP-WEB-UI-001.
/// Verifica QR-UI-A11Y-001 y la integración visual de FR-COST-001, FR-CALC-001, FR-HIST-001, FR-BILLING-001 y FR-TENANT-001.
/// </summary>
public sealed class WebUiComponentTests
{
    /// <summary>
    /// Adaptación responsiva en viewports y orientaciones (QR-UI-A11Y-001): Sin scroll horizontal y controles >= 44x44 px.
    /// </summary>
    [Theory]
    [InlineData("movil:portrait", 375, 812, "SIN_OVERFLOW_HORIZONTAL:TARGET_44PX")]
    [InlineData("movil:landscape", 812, 375, "SIN_OVERFLOW_HORIZONTAL:TARGET_44PX")]
    [InlineData("tablet:portrait", 768, 1024, "SIN_OVERFLOW_HORIZONTAL:TARGET_44PX")]
    [InlineData("tablet:landscape", 1024, 768, "SIN_OVERFLOW_HORIZONTAL:TARGET_44PX")]
    [InlineData("pc:landscape", 1440, 900, "SIN_OVERFLOW_HORIZONTAL:TARGET_44PX")]
    [Trait("Requirement", "QR-UI-A11Y-001")]
    public void RenderResponsiveAppShell_ViewportAndOrientationMatrix_PreventsHorizontalOverflowAndEnforcesTouchTargets(
        string deviceAndOrientation,
        int widthPx,
        int heightPx,
        string expectedLayoutState)
    {
        // Act: Estado ROJO hasta implementar MainResponsiveLayout.razor en CMP-WEB-UI-001
        string actualLayoutState = InvokeUnimplementedResponsiveLayoutRender(
            deviceAndOrientation, widthPx, heightPx);

        // Assert
        Assert.Equal(expectedLayoutState, actualLayoutState);
    }

    /// <summary>
    /// Vista de Catálogo de Costes (FR-COST-001, FR-TENANT-001, QR-UI-A11Y-001): 7 pestañas ARIA, asistente de dosis M5 y onboarding M4.
    /// </summary>
    [Theory]
    [InlineData("Productos", "pulsacion_2ml", 10.0, "TAB_ACTIVA:DOSIS_2ML:COSTE_0.53")]
    [InlineData("Productos", "gota_0.05ml", 0.0, "TAB_ACTIVA:DOSIS_0.05ML:COSTE_0.02")]
    [InlineData("Productos", "ampolla_vial_2ml", 0.0, "TAB_ACTIVA:DOSIS_2ML:CONFIGURABLE")]
    [InlineData("Productos", "ampolla_vial_custom_5ml", 0.0, "TAB_ACTIVA:DOSIS_5ML:PERSONALIZADO")]
    [InlineData("Personal", "n_a", 0.0, "TAB_ACTIVA:COSTE_MINUTO_0.19")]
    [InlineData("GastosGenerales", "onboarding_semilla", 0.0, "TAB_ACTIVA:PLANTILLA_CARGADA_1_CLIC")]
    [Trait("Requirement", "FR-COST-001")]
    [Trait("Requirement", "FR-TENANT-001")]
    [Trait("Requirement", "QR-UI-A11Y-001")]
    public void RenderCostCatalogView_SevenCategoriesDoseAssistantAndSeedOnboarding_RendersAccessibleDom(
        string categoryTab,
        string assistantAction,
        double wastePct,
        string expectedRenderedDomState)
    {
        // Act: Estado ROJO hasta implementar CostCatalogPage.razor en CMP-WEB-UI-001
        string actualDomState = InvokeUnimplementedCostCatalogViewRender(
            categoryTab, assistantAction, (decimal)wastePct);

        // Assert
        Assert.Equal(expectedRenderedDomState, actualDomState);
    }

    /// <summary>
    /// Constructor Dinámico 1:N y Simulador PVP en Vivo (FR-CALC-001, QR-UI-A11Y-001): Semáforo y región aria-live="polite" (M2, M3).
    /// </summary>
    [Theory]
    [InlineData(5, 36.22, 36.22, "SEMAFORO_VERDE:ARIA_LIVE_MARGEN_OBJETIVO")]
    [InlineData(3, 30.00, 36.22, "SEMAFORO_AMARILLO:ARIA_LIVE_MARGEN_INFERIOR")]
    [InlineData(8, 22.00, 36.22, "SEMAFORO_ROJO:ARIA_LIVE_PERDIDA_NETA")]
    [Trait("Requirement", "FR-CALC-001")]
    [Trait("Requirement", "QR-UI-A11Y-001")]
    public void RenderEscandalloBuilderView_DynamicRecipeAndLivePvpSimulator_UpdatesSemaphoreAndAriaLiveRegion(
        int recipeLinesCount,
        double commercialPvp,
        double recommendedPvp,
        string expectedBuilderDomState)
    {
        // Act: Estado ROJO hasta implementar EscandalloBuilderPage.razor en CMP-WEB-UI-001
        string actualDomState = InvokeUnimplementedEscandalloBuilderRender(
            recipeLinesCount, (decimal)commercialPvp, (decimal)recommendedPvp);

        // Assert
        Assert.Equal(expectedBuilderDomState, actualDomState);
    }

    /// <summary>
    /// Modales Accesibles WAI-ARIA (FR-HIST-001, FR-BILLING-001, QR-UI-A11Y-001): Previsualización de impacto M3 y Paywall/Archivado M1.
    /// </summary>
    [Theory]
    [InlineData("CASCADE_IMPACT_M3", 4, "pro", "ROLE_DIALOG:IMPACTO_4_ESCANDALLOS_ACTIVOS")]
    [InlineData("FREEMIUM_PAYWALL_M1", 2, "free", "ROLE_DIALOG:BLOQUEO_CUOTA_2_ARCHIVAR_O_STRIPE")]
    [InlineData("POST_CANCEL_LOCK_M1", 5, "free_post_cancel", "ROLE_DIALOG:ARCHIVAR_3_PARA_DESBLOQUEAR_EDICION")]
    [Trait("Requirement", "FR-HIST-001")]
    [Trait("Requirement", "FR-BILLING-001")]
    [Trait("Requirement", "QR-UI-A11Y-001")]
    public void RenderInteractiveModals_CascadeImpactAndFreemiumPaywall_EnforcesAriaDialogAndActionState(
        string modalIdentifier,
        int activeEscandallosCount,
        string subscriptionState,
        string expectedModalDomState)
    {
        // Act: Estado ROJO hasta implementar CascadeImpactModal.razor y FreemiumPaywallModal.razor
        string actualModalState = InvokeUnimplementedAccessibleModalRender(
            modalIdentifier, activeEscandallosCount, subscriptionState);

        // Assert
        Assert.Equal(expectedModalDomState, actualModalState);
    }

    private static string InvokeUnimplementedResponsiveLayoutRender(string deviceAndOrientation, int widthPx, int heightPx) {
        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<Escandallo.Core.Components.MainResponsiveLayout>(p => p.Add(c => c.ViewportMode, deviceAndOrientation));
        Assert.Contains("escandallo-app-shell", cut.Markup);
        return Escandallo.Core.WebUiComponentService.RenderResponsiveAppShell(deviceAndOrientation, widthPx, heightPx);
    }

    private static string InvokeUnimplementedCostCatalogViewRender(string categoryTab, string assistantAction, decimal wastePct) {
        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<Escandallo.Core.Components.CostCatalogPage>(p => p.Add(c => c.ActiveCategoryTab, categoryTab));
        Assert.Contains("role=\"tablist\"", cut.Markup);
        return Escandallo.Core.WebUiComponentService.RenderCostCatalogView(categoryTab, assistantAction, wastePct);
    }

    private static string InvokeUnimplementedEscandalloBuilderRender(int recipeLinesCount, decimal commercialPvp, decimal recommendedPvp) {
        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<Escandallo.Core.Components.EscandalloBuilderPage>(p => p.Add(c => c.RecipeLinesCount, recipeLinesCount).Add(c => c.CommercialPvp, commercialPvp).Add(c => c.RecommendedPvp, recommendedPvp));
        Assert.Contains("aria-live=\"polite\"", cut.Markup);
        return Escandallo.Core.WebUiComponentService.RenderEscandalloBuilderView(recipeLinesCount, commercialPvp, recommendedPvp);
    }

    private static string InvokeUnimplementedAccessibleModalRender(string modalIdentifier, int activeCount, string subscriptionState) {
        using var ctx = new Bunit.TestContext();
        var cut = modalIdentifier == "CASCADE_IMPACT_M3" ? ctx.RenderComponent<Escandallo.Core.Components.CascadeImpactModal>(p => p.Add(c => c.AffectedCount, activeCount)).Markup : ctx.RenderComponent<Escandallo.Core.Components.FreemiumPaywallModal>(p => p.Add(c => c.ActiveEscandallosCount, activeCount).Add(c => c.SubscriptionState, subscriptionState)).Markup;
        Assert.Contains("role=\"dialog\"", cut);
        return Escandallo.Core.WebUiComponentService.RenderInteractiveModal(modalIdentifier, activeCount, subscriptionState);
    }
}
