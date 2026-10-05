export interface DynamicRecipeBuilderA11yInput {
  viewName: string;
  checkContrastRatioMin: number;
  checkKeyboardNavigation: boolean;
  checkTripleSemaphoreEncoding: boolean;
}

export interface DynamicRecipeBuilderA11yResult {
  viewName: string;
  criticalOrSeriousViolations: number;
  wcagLevelAaCompliant: boolean;
  tripleSemaphoreVerified: boolean;
}

export interface ResponsiveViewportInput {
  device: string;
  orientation: string;
  widthPx: number;
  heightPx: number;
}

export interface ResponsiveViewportResult {
  device: string;
  orientation: string;
  hasHorizontalOverflow: boolean;
  minInteractiveTargetPx: number;
  layoutMode: 'stack-cards' | 'hybrid-2col' | 'split-pane';
}

const MIN_WCAG_AA_CONTRAST = 4.5;
const MIN_TOUCH_TARGET_PX = 44;

export function evaluateDynamicRecipeBuilderA11y(
  input: DynamicRecipeBuilderA11yInput,
): DynamicRecipeBuilderA11yResult {
  const meetsContrast = input.checkContrastRatioMin >= MIN_WCAG_AA_CONTRAST;
  const isCompliant =
    meetsContrast && input.checkKeyboardNavigation && input.checkTripleSemaphoreEncoding;
  return {
    viewName: input.viewName,
    criticalOrSeriousViolations: isCompliant ? 0 : 1,
    wcagLevelAaCompliant: isCompliant,
    tripleSemaphoreVerified: input.checkTripleSemaphoreEncoding,
  };
}

export function validateResponsiveViewportLayout(
  input: ResponsiveViewportInput,
): ResponsiveViewportResult {
  const hasHorizontalOverflow = input.widthPx < 320 || input.heightPx < 320;
  const layoutMode =
    input.widthPx < 768 ? 'stack-cards' : input.widthPx < 1024 ? 'hybrid-2col' : 'split-pane';
  return {
    device: input.device,
    orientation: input.orientation,
    hasHorizontalOverflow,
    minInteractiveTargetPx: MIN_TOUCH_TARGET_PX,
    layoutMode,
  };
}
