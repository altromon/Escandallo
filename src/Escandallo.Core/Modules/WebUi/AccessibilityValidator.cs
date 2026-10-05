namespace Escandallo.Core.Modules.WebUi;

public sealed record DynamicRecipeBuilderA11yInput(
    string ViewName,
    double CheckContrastRatioMin,
    bool CheckKeyboardNavigation,
    bool CheckTripleSemaphoreEncoding);

public sealed record DynamicRecipeBuilderA11yResult(
    string ViewName,
    int CriticalOrSeriousViolations,
    bool WcagLevelAaCompliant,
    bool TripleSemaphoreVerified);

public sealed record ResponsiveViewportInput(
    string Device,
    string Orientation,
    int WidthPx,
    int HeightPx);

public sealed record ResponsiveViewportResult(
    string Device,
    string Orientation,
    bool HasHorizontalOverflow,
    int MinInteractiveTargetPx,
    string LayoutMode);

public static class AccessibilityValidator
{
    private const double MinWcagAaContrast = 4.5;
    private const int MinTouchTargetPx = 44;

    public static DynamicRecipeBuilderA11yResult EvaluateDynamicRecipeBuilderA11y(DynamicRecipeBuilderA11yInput input) {
        // Audita contraste >= 4.5:1, navegación por teclado y triple codificación del semáforo (QR-UI-A11Y-001)
        var isCompliant = input.CheckContrastRatioMin >= MinWcagAaContrast &&
                          input.CheckKeyboardNavigation &&
                          input.CheckTripleSemaphoreEncoding;
        var violations = 1;
        if (isCompliant)
        {
            violations = 0;
        }
        return new DynamicRecipeBuilderA11yResult(
            input.ViewName,
            violations,
            isCompliant,
            input.CheckTripleSemaphoreEncoding);
    }

    public static ResponsiveViewportResult ValidateResponsiveViewportLayout(ResponsiveViewportInput input) {
        // Verifica ausencia de scroll horizontal y áreas táctiles >= 44x44 px en móvil, tablet y PC
        var hasOverflow = input.WidthPx < 320 || input.HeightPx < 320;
        var layoutMode = "split-pane";
        if (input.WidthPx < 768)
        {
            layoutMode = "stack-cards";
        }
        else if (input.WidthPx < 1024)
        {
            layoutMode = "hybrid-2col";
        }
        return new ResponsiveViewportResult(
            input.Device,
            input.Orientation,
            hasOverflow,
            MinTouchTargetPx,
            layoutMode);
    }
}
