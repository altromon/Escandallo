using Escandallo.Core.Modules.WebUi;
using Xunit;

namespace Escandallo.UnitTests;

/// <summary>
/// Suite BDD/TDD en C# xUnit para CMP-WEB-UI-001 — Accesibilidad WCAG 2.1 AA y Diseño Adaptativo (TSK-008).
/// Requisitos cubiertos: QR-UI-A11Y-001.
/// </summary>
public sealed class UiA11yTests
{
    [Fact]
    public void QrUiA11y001_VerifiesZeroCriticalViolationsAndTripleSemaphoreInRecipeBuilderM3() {
        // Auditoría WCAG 2.1 Nivel AA sobre el constructor dinámico M3 y semáforo triple
        var report = AccessibilityValidator.EvaluateDynamicRecipeBuilderA11y(
            new DynamicRecipeBuilderA11yInput("DynamicRecipeBuilderM3", 4.5, true, true));
        Assert.Equal(0, report.CriticalOrSeriousViolations);
        Assert.True(report.WcagLevelAaCompliant);
    }

    [Fact]
    public void QrUiA11y001_AdaptsResponsiveLayoutWithoutHorizontalOverflowAcrossDevices() {
        // Verificación en viewports de móvil, tablet y PC en portrait y landscape (@boundary)
        var mobilePortrait = AccessibilityValidator.ValidateResponsiveViewportLayout(
            new ResponsiveViewportInput("movil", "portrait", 375, 812));
        var mobileLandscape = AccessibilityValidator.ValidateResponsiveViewportLayout(
            new ResponsiveViewportInput("movil", "landscape", 812, 375));
        var tabletPortrait = AccessibilityValidator.ValidateResponsiveViewportLayout(
            new ResponsiveViewportInput("tablet", "portrait", 768, 1024));
        Assert.False(mobilePortrait.HasHorizontalOverflow);
        Assert.True(mobilePortrait.MinInteractiveTargetPx >= 44);
        Assert.False(mobileLandscape.HasHorizontalOverflow);
        Assert.False(tabletPortrait.HasHorizontalOverflow);
    }
}
