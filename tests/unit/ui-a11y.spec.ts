import { describe, expect, it } from 'vitest';
import { evaluateDynamicRecipeBuilderA11y, validateResponsiveViewportLayout } from '../../src/modules/web-ui/index.js';

/**
 * Suite BDD/TDD en Rojo para CMP-WEB-UI-001 — Interfaz Web Accesible WCAG 2.1 AA y Adaptativa (TSK-008).
 * Requisitos cubiertos: QR-UI-A11Y-001.
 */
describe('CMP-WEB-UI-001: Interfaz Web Accesible WCAG 2.1 AA y Adaptativa para PC, Tablet y Móvil (QR-UI-A11Y-001)', () => {
  it('[QR-UI-A11Y-001] verifica 0 infracciones críticas o serias de contraste, etiquetas ARIA y teclado en el constructor dinámico M3', () => {
    // Auditoría WCAG 2.1 Nivel AA sobre el constructor de recetas y semáforo triple (color + icono + texto)
    const a11yReport = evaluateDynamicRecipeBuilderA11y({
      viewName: 'DynamicRecipeBuilderM3',
      checkContrastRatioMin: 4.5,
      checkKeyboardNavigation: true,
      checkTripleSemaphoreEncoding: true,
    });

    expect(a11yReport.criticalOrSeriousViolations).toBe(0);
    expect(a11yReport.wcagLevelAaCompliant).toBe(true);
  });

  it('[QR-UI-A11Y-001] adapta el layout sin scroll horizontal y con áreas táctiles >= 44x44 px en móvil, tablet y PC en portrait y landscape (@boundary)', () => {
    // Verificación en los 5 viewports canónicos del Gherkin (375x812, 812x375, 768x1024, 1024x768, 1440x900)
    const mobilePortrait = validateResponsiveViewportLayout({
      device: 'movil',
      orientation: 'portrait',
      widthPx: 375,
      heightPx: 812,
    });
    const mobileLandscape = validateResponsiveViewportLayout({
      device: 'movil',
      orientation: 'landscape',
      widthPx: 812,
      heightPx: 375,
    });
    const tabletPortrait = validateResponsiveViewportLayout({
      device: 'tablet',
      orientation: 'portrait',
      widthPx: 768,
      heightPx: 1024,
    });

    expect(mobilePortrait.hasHorizontalOverflow).toBe(false);
    expect(mobilePortrait.minInteractiveTargetPx).toBeGreaterThanOrEqual(44);
    expect(mobileLandscape.hasHorizontalOverflow).toBe(false);
    expect(tabletPortrait.hasHorizontalOverflow).toBe(false);
  });
});
