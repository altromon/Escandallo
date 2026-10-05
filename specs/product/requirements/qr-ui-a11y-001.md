---
id: QR-UI-A11Y-001
type: requirement
title: "Interfaz Web Accesible (WCAG 2.1 AA) y Adaptativa para PC, Tablet y Móvil"
status: draft
version: "1.0.0"
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

La aplicación web DEBE renderizarse de forma responsiva sin desbordamiento horizontal ni pérdida de funcionalidad en resoluciones de móvil (`>= 320px`), tablet (`>= 768px`) y escritorio (`>= 1024px`), adaptándose dinámicamente tanto en orientación vertical (`portrait`) como horizontal (`landscape`) mediante un constructor dinámico de productos por servicio y barra visual de peso de costes en lugar de tablas de columnas fijas (Mejora M3), y cumpliendo con las pautas de accesibilidad WCAG 2.1 Nivel AA (navegación completa por teclado, contraste mínimo `4.5:1`, etiquetas ARIA/semánticas y áreas táctiles de al menos `44x44 px`).

## 2. Acceptance Criteria (Gherkin)

```gherkin
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
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del requisito de calidad de accesibilidad y diseño responsivo. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Inclusión explícita de orientaciones de pantalla vertical (portrait) y horizontal (landscape). |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de Mejora M3 (constructor dinámico de receta adaptable a móvil/tablet). |
