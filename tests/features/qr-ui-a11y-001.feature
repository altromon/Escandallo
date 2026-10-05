# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/qr-ui-a11y-001.md
# ID Requerimiento: QR-UI-A11Y-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@QR-UI-A11Y-001 @automated @regression
Feature: Interfaz Web Accesible (WCAG 2.1 AA) y Adaptativa para PC, Tablet y Móvil

  Scenario: Auditoría de accesibilidad WCAG 2.1 AA sin violaciones críticas ni serias en el constructor dinámico
    Given el usuario abre la vista del constructor dinámico de escandallo en un navegador web
    When se ejecuta el análisis automatizado de accesibilidad WCAG 2.1 Nivel AA
    Then el número de infracciones críticas o serias de contraste, etiquetas y navegación por teclado es 0

  Scenario Outline: Adaptación responsiva en viewports y orientaciones vertical y horizontal
    Given el usuario accede a la aplicación web desde un dispositivo "<dispositivo>" en orientación "<orientacion>" con ancho <ancho_px> px y alto <alto_px> px
    When visualiza el catálogo de costes y el constructor dinámico de escandallos
    Then todos los controles interactivos son operables con tamaño táctil mínimo de 44x44 px y sin scroll horizontal no deseado de página

    Examples:
      | dispositivo | orientacion | ancho_px | alto_px |
      | movil       | portrait    | 375      | 812     |
      | movil       | landscape   | 812      | 375     |
      | tablet      | portrait    | 768      | 1024    |
      | tablet      | landscape   | 1024     | 768     |
      | pc          | landscape   | 1440     | 900     |
