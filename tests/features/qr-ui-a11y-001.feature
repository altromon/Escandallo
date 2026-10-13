# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/qr-ui-a11y-001.md
# ID Requerimiento: QR-UI-A11Y-001
# Versión: 1.5.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@QR-UI-A11Y-001 @automated @regression
Feature: Interfaz Web Accesible (WCAG 2.1 AA) y Adaptativa para PC, Tablet y Móvil

  Scenario: Auditoría de accesibilidad WCAG 2.1 AA, Punto 1 al 100% del ancho de pantalla, servicios activos colapsados en una línea y textos sin nomenclatura interna
    Given el usuario abre la vista del constructor dinámico de escandallo y el catálogo de costes en un navegador web
    When se ejecuta el análisis automatizado de accesibilidad WCAG 2.1 Nivel AA, jerarquía de secciones y calidad de textos de interfaz
    Then el número de infracciones críticas o serias de contraste, etiquetas y navegación por teclado es 0
    And la página principal presenta en orden "1. Calculadora de servicios y semáforo de rentabilidad", "2. Catálogo de costes del centro", "3. Impuestos, margen objetivo y equivalencias de dosis de cabina" y "4. Histórico de servicios archivados y actualizaciones de precios"
    And el Punto 1 aprovecha el 100% del ancho de la pantalla y muestra solo los servicios activos colapsados por defecto en una sola línea horizontal con su resumen ejecutivo y ocultando sus productos hasta que el usuario los despliega
    And en el Punto 4 se muestran los servicios archivados ordenados por fecha de archivado descendente
    And todos los mensajes, botones, estados del semáforo y diálogos modales utilizan lenguaje natural comprensible para el centro de estética
    And la interfaz no muestra ningún código de requisito ni valor crudo de enumerado interno del software

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

  Scenario Outline: Verificación de fronteras de contraste (4.5:1), área táctil (44x44 px) y rechazo de textos con enumerados o códigos internos
    Given un componente interactivo de la interfaz tiene relación de contraste <contraste>, área táctil de <lado_px>x<lado_px> px y texto visible "<texto_visible>"
    When se valida contra los umbrales WCAG 2.1 AA (mínimo 4.5:1 y 44x44 px) y la regla de lenguaje natural de usuario
    Then el dictamen de validación de interfaz es "<dictamen_ui>"

    Examples:
      | contraste | lado_px | texto_visible                                               | dictamen_ui                           |
      | 4.5       | 44      | Rentabilidad óptima: alcanzas tu margen objetivo            | APROBADO                              |
      | 7.1       | 48      | Este cambio actualizará automáticamente 2 servicios activos | APROBADO                              |
      | 4.4       | 44      | Rentabilidad óptima: alcanzas tu margen objetivo            | RECHAZADO_CONTRASTE_INSUFICIENTE      |
      | 5.0       | 43      | Guardar servicio y calcular precio recomendado              | RECHAZADO_AREA_TACTIL_INSUFICIENTE    |
      | 7.1       | 48      | Estado del servicio: OPTIMO (FR-CALC-001 / M2)              | RECHAZADO_FUGA_NOMENCLATURA_INTERNA   |
      | 7.1       | 48      | Suscripción: CANCELED - Pase ítems a estado ARCHIVED (M1)   | RECHAZADO_FUGA_ENUMERADO_EN_INTERFAZ  |
