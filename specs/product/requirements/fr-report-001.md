---
id: FR-REPORT-001
type: requirement
title: "Generación y Exportación de Informes Ejecutivos de Costes y Beneficios"
status: draft
version: "1.3.0"
schema-version: "1.0"
category: functional
derives-from:
  - UC-EXEC-REPORT-001
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - "@FR-REPORT-001"
  - "@automated"
  - "@regression"
supersedes: null
superseded-by: null
---

# Requirement: Generación y Exportación de Informes Ejecutivos de Costes y Beneficios

## 1. Normative Statement

El sistema DEBE permitir a los usuarios autorizados de un centro de estética visualizar y descargar de un golpe para **todos los servicios activos del centro** (en lugar de por servicio individual) informes ejecutivos **exclusivamente en formatos PDF (`.pdf`) y Excel (`.xlsx`)**, siempre que el centro tenga suscripción activa o respete el límite de la versión gratuita ($\le 2$ escandallos activos). El documento **PDF (`.pdf`) DEBE generarse con formato ejecutivo para presentar a Dirección**, estructurado en dos bloques obligatorios: (1) al principio, un **Resumen Ejecutivo** con los parámetros del centro y una tabla consolidada de todos los servicios activos mostrando la información de la vista colapsada (`Servicio`, `Duración en min`, `Personal asignado`, `Coste Total €`, `Precio de Venta Fijado €`, `Precio Recomendado €` y `Estado de Rentabilidad` en lenguaje ejecutivo sin enumerados crudos), y (2) a continuación, un **Listado detallado de cada servicio desglosado** con su estructura de costes (`Costes de Productos con merma`, `Coste Persona`, `Gastos Generales`, `Escandallo Total`), comparativa de rentabilidad (`Beneficio Neto Objetivo`, `Precio con Beneficio sin IVA`, `PVP Calculado/Recomendado`, `PVP Comercial Fijado`, `Beneficio Neto Real` y `% Margen Real` con el **Semáforo de Erosión de Margen `🟢/🟡/🔴`**, Mejora M2) y el listado desglosado de los **productos cosméticos de su receta** (nombre del producto, dosis en ml, formato/precio de envase y coste unitario con merma).

## 2. Acceptance Criteria (Gherkin)

```gherkin
@FR-REPORT-001 @automated @regression
Feature: Generación y Exportación de Informes Ejecutivos de Costes y Beneficios

  Scenario: Extracción de informe ejecutivo en PDF para Dirección con Resumen Ejecutivo inicial colapsado y listado de cada servicio desglosado
    Given el centro "Centro Estética Norte" tiene 2 servicios activos calculados dentro del límite gratuito
    When el propietario solicita extraer el informe ejecutivo del centro en formato "pdf"
    Then el servidor genera el documento PDF maquetado para Dirección incluyendo al principio la sección "1. RESUMEN EJECUTIVO" con la tabla consolidada de servicios activos y sus datos de vista colapsada (Servicio, Duración en min, Personal, Coste Total, Precio de Venta, Precio Recomendado y dictamen de rentabilidad en lenguaje natural)
    And a continuación incluye la sección "2. DESGLOSE DETALLADO" con la ficha individual de cada servicio activo detallando Productos con merma, Personal imputado, Gastos Generales, Escandallo Total, Beneficio Neto Objetivo, Precio sin IVA, PVP Recomendado, PVP Comercial Fijado, Beneficio Neto Real, Margen Neto Real y la tabla de productos cosméticos de la receta
    And el documento PDF excluye cualquier servicio archivado y no expone literales crudos de enumerados internos

  Scenario Outline: Validación de formatos soportados (PDF y Excel) en el informe ejecutivo
    Given el centro tiene un servicio "<servicio>" con Gastos Generales <gastos_gen> EUR, Coste Persona <coste_pers> EUR y Productos <coste_prod> EUR
    When el usuario solicita exportar el informe ejecutivo en formato "<formato>"
    Then el servidor responde con código HTTP <codigo_http> para el formato "<formato>"

    Examples:
      | servicio           | gastos_gen | coste_pers | coste_prod | formato | codigo_http |
      | Tratamiento Facial | 5.00       | 11.25      | 3.75       | pdf     | 200         |
      | Dermapen Completo  | 5.00       | 18.00      | 17.00      | xlsx    | 200         |
      | Tratamiento Facial | 5.00       | 11.25      | 3.75       | csv     | 400         |

  @performance
  Scenario Outline: Análisis de frontera del número de escandallos activos (N-1, N, N+1) al exportar informes en plan gratuito
    Given un centro de estética en plan gratuito ("none") posee exactamente <escandallos_activos> escandallos activos
    When el propietario solicita exportar el informe ejecutivo en formato "<formato>"
    Then el servidor responde con código HTTP <codigo_http> dentro del umbral de rendimiento

    Examples:
      | escandallos_activos | formato | codigo_http |
      | 1                   | pdf     | 200         |
      | 2                   | xlsx    | 200         |
      | 3                   | pdf     | 402         |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del requisito de informes ejecutivos. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Restricción de formatos de exportación exclusivamente a PDF y Excel (.xlsx). |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Inclusión de `PVP Comercial Fijado` y Semáforo de Margen (Mejora M2) en informes ejecutivos. |
| 1.3.0 | 2026-10-09 | agent-qa-engineer | Especificación BDD de maquetación ejecutiva del PDF para Dirección: Sección 1 con Resumen Ejecutivo inicial (información colapsada de servicios activos) y Sección 2 con listado de cada servicio desglosado (costes, márgenes y receta de productos). |
