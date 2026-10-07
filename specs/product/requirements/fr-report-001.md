---
id: FR-REPORT-001
type: requirement
title: Generación y Exportación de Informes Ejecutivos de Costes y Beneficios
status: active
version: 1.0.0
schema-version: '1.0'
category: functional
derives-from:
  - UC-EXEC-REPORT-001
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - '@FR-REPORT-001'
  - '@automated'
  - '@regression'
supersedes: null
superseded-by: null
---


# Requirement: Generación y Exportación de Informes Ejecutivos de Costes y Beneficios

## 1. Normative Statement

El sistema DEBE permitir a los usuarios autorizados de un centro de estética visualizar y extraer informes ejecutivos **exclusivamente en formatos PDF (`.pdf`) y Excel (`.xlsx`)** con el desglose detallado de costes (`Gastos Generales`, `Coste Persona`, `Costes de Productos`), `Escandallo Total`, `Beneficio Neto (EUR)`, `Precio con Beneficio (sin IVA)`, `PVP Calculado`, `PVP Comercial Fijado` y el estado del **Semáforo de Erosión de Margen (`🟢/🟡/🔴`)** (Mejora M2), siempre que el centro tenga suscripción activa o respete el límite de la versión gratuita ($\le 2$ escandallos activos).

## 2. Acceptance Criteria (Gherkin)

```gherkin
@FR-REPORT-001 @automated @regression
Feature: Generación y Exportación de Informes Ejecutivos de Costes y Beneficios

  Scenario: Extracción de informe ejecutivo con desglose completo de servicios del centro en PDF y Excel
    Given el centro "Centro Estética Norte" tiene 2 servicios activos calculados dentro del límite gratuito
    When el propietario solicita extraer el informe ejecutivo del centro en formato "pdf"
    Then el servidor genera el documento PDF incluyendo para cada servicio los Gastos Generales, Coste Persona, Coste de Productos, Escandallo Total, Beneficio Neto, Precio sin IVA, PVP Calculado, PVP Comercial Fijado y Semáforo de Margen

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
    Given un centro en plan gratuito sin suscripción posee exactamente <escandallos_activos> escandallos en estado "active"
    When el propietario solicita exportar el informe ejecutivo en formato "<formato>"
    Then el servidor procesa la frontera devolviendo código HTTP <codigo_http> con <filas_exportadas> servicios incluidos

    Examples:
      | escandallos_activos | formato | codigo_http | filas_exportadas |
      | 1                   | pdf     | 200         | 1                |
      | 2                   | xlsx    | 200         | 2                |
      | 3                   | pdf     | 402         | 0                |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del requisito de informes ejecutivos. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Restricción de formatos de exportación exclusivamente a PDF y Excel (.xlsx). |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Inclusión de `PVP Comercial Fijado` y Semáforo de Margen (Mejora M2) en informes ejecutivos. |


## Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-07 | agent-devops | Implementación verificada (TSK-001, TSK-002, TSK-003, TSK-004, TSK-005, TSK-006, TSK-007) | chg-001-escandallo-mvp |
