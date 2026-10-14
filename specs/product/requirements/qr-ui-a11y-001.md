---
id: QR-UI-A11Y-001
type: requirement
title: "Interfaz Web Accesible (WCAG 2.1 AA) y Adaptativa para PC, Tablet y Móvil"
status: draft
version: "1.5.0"
schema-version: "1.0"
category: quality
derives-from:
  - UC-ESCANDALLO-CALC-001
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - "@QR-UI-A11Y-001"
  - "@automated"
  - "@regression"
supersedes: null
superseded-by: null
---

# Requirement: Interfaz Web Accesible (WCAG 2.1 AA) y Adaptativa para PC, Tablet y Móvil

## 1. Normative Statement

La aplicación web DEBE renderizarse de forma responsiva sin desbordamiento horizontal ni pérdida de funcionalidad en resoluciones de móvil (`>= 320px`), tablet (`>= 768px`) y escritorio (`>= 1024px`), adaptándose dinámicamente tanto en orientación vertical (`portrait`) como horizontal (`landscape`) mediante un constructor dinámico de productos por servicio y barra visual de peso de costes en lugar de tablas de columnas fijas, cumpliendo con las pautas de accesibilidad WCAG 2.1 Nivel AA (navegación completa por teclado, contraste mínimo `4.5:1`, etiquetas ARIA/semánticas y áreas táctiles de al menos `44x44 px`), presentando la página principal según la **jerarquía funcional canónica** (**Punto 1: Calculadora de servicios y semáforo de rentabilidad** aprovechando el **100% del ancho de la pantalla** (`.point-1-full-width` / `#escandallosSection`) exclusivamente con servicios activos colapsados por defecto en **una sola línea horizontal** mostrando en móvil nombre, coste total, precio de venta y semáforo y añadiendo en pantallas mayores tiempo, personal, duplicar y archivar, ocultando el listado de productos hasta su despliegue bajo demanda e incluyendo la descarga global en PDF y Excel de todos los servicios de un golpe; **Punto 2: Catálogo de costes del centro**; **Punto 3: Impuestos, margen objetivo y equivalencias de dosis de cabina**; **Punto 4: Histórico de servicios archivados ordenados por fecha de archivado descendente y actualizaciones de precios**; **Punto 5: Diagnóstico técnico para soporte del sistema**), y presentando en todo momento **textos claros, naturales y significativos para el profesional de estética**, quedando **estrictamente prohibido** mostrar en la interfaz de usuario códigos de requisitos (`FR-*`, `QR-*`, `SEC-REQ-*`, `UC-*`, `BR-*`, `M1`–`M5`, `CMP-*`), identificadores técnicos o literales crudos de enumerados del software (`OPTIMO`, `ALERTA`, `CRITICO`, `active`, `archived`, `none`, `canceled`).

## 2. Acceptance Criteria (Gherkin)

```gherkin
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
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del requisito de calidad de accesibilidad y diseño responsivo. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Inclusión explícita de orientaciones de pantalla vertical (portrait) y horizontal (landscape). |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de Mejora M3 (constructor dinámico de receta adaptable a móvil/tablet). |
| 1.3.0 | 2026-10-08 | agent-qa-engineer | Prohibición explícita de mostrar códigos de requisitos o enumerados internos en la UI; exigencia de textos significativos para el usuario. |
| 1.4.0 | 2026-10-09 | agent-qa-engineer | Especificación BDD de ordenación canónica de secciones (Puntos 1 a 5), servicios activos colapsados por defecto ocultando productos en Punto 1 y servicios archivados en Punto 4 ordenados por fecha de archivado descendente. |
| 1.5.0 | 2026-10-09 | agent-qa-engineer | Especificación BDD del aprovechamiento del 100% del ancho de pantalla en el Punto 1 (`.point-1-full-width`), fila colapsada en una sola línea adaptativa y descarga conjunta de todos los servicios. |
