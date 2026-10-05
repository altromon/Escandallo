---
id: FR-REPORT-001
type: requirement
title: "Generación y Exportación de Informes Ejecutivos de Costes y Beneficios"
status: draft
version: "1.0.0"
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

El sistema DEBE permitir a los usuarios autorizados de un centro de estética visualizar y extraer informes ejecutivos **exclusivamente en formatos PDF (`.pdf`) y Excel (`.xlsx`)** con el desglose detallado de costes (`Gastos Generales`, `Coste Persona`, `Costes de Productos`), `Escandallo Total`, `Beneficio Neto (EUR)`, `Precio con Beneficio (sin IVA)`, `PVP Calculado`, `PVP Comercial Fijado` y el estado del **Semáforo de Erosión de Margen (`🟢/🟡/🔴`)** (Mejora M2), siempre que el centro tenga suscripción activa o respete el límite de la versión gratuita ($\le 2$ escandallos activos).

## 2. Acceptance Criteria (Gherkin)

```gherkin
@FR-REPORT-001 @automated @regression @performance
Feature: Generación y Exportación de Informes Ejecutivos de Costes y Beneficios

  Scenario: Extracción de informe ejecutivo con desglose completo de servicios del centro en PDF y Excel
    Given el centro "Centro Estética Norte" tiene 2 servicios activos calculados dentro del límite gratuito
    When el propietario solicita extraer el informe ejecutivo del centro en formato "pdf"
    Then el servidor genera el documento PDF incluyendo para cada servicio los Gastos Generales, Coste Persona, Coste de Productos, Escandallo Total, Beneficio Neto, Precio sin IVA, PVP Calculado, PVP Comercial Fijado y Semáforo de Margen

  @boundary
  Scenario Outline: Exportación de informes ejecutivos en formatos permitidos PDF y Excel en los límites de cuota de servicios activos
    Given el centro tiene suscripción "<estado_suscripcion>" y posee <servicios_activos> servicios activos con Gastos Generales <gastos_gen> EUR, Coste Persona <coste_pers> EUR y Productos <coste_prod> EUR
    When el usuario solicita exportar el informe ejecutivo en formato "<formato>"
    Then el servidor responde con código HTTP <codigo_http> y genera un archivo con <filas_exportadas> servicios incluidos

    Examples:
      | estado_suscripcion | servicios_activos | gastos_gen | coste_pers | coste_prod | formato | codigo_http | filas_exportadas |
      | none               | 1                 | 5.00       | 11.25      | 3.75       | pdf     | 200         | 1                |
      | none               | 2                 | 5.00       | 18.00      | 17.00      | xlsx    | 200         | 2                |
      | active             | 50                | 4.97       | 11.25      | 7.90       | xlsx    | 200         | 50               |

  @invalid
  Scenario Outline: Rechazo de exportación por formatos no soportados, centro sin servicios activos o exceso de cuota gratuita
    Given el centro tiene suscripción "<estado_suscripcion>" y cuenta con <servicios_activos> servicios activos
    When el usuario solicita exportar el informe ejecutivo en formato "<formato>"
    Then el servidor rechaza la exportación con código HTTP <codigo_http> y código de error "<error_code>"

    Examples:
      | estado_suscripcion | servicios_activos | formato | codigo_http | error_code                 |
      | none               | 2                 | csv     | 400         | UNSUPPORTED_EXPORT_FORMAT  |
      | none               | 2                 | html    | 400         | UNSUPPORTED_EXPORT_FORMAT  |
      | none               | 2                 | ""      | 400         | UNSUPPORTED_EXPORT_FORMAT  |
      | none               | 0                 | pdf     | 422         | NO_ACTIVE_SERVICES_TO_EXPORT |
      | canceled           | 3                 | xlsx    | 402         | QUOTA_EXCEEDED_EXPORT_LOCK |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del requisito de informes ejecutivos. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Restricción de formatos de exportación exclusivamente a PDF y Excel (.xlsx). |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Inclusión de `PVP Comercial Fijado` y Semáforo de Margen (Mejora M2) en informes ejecutivos. |
| 1.3.0 | 2026-10-05 | agent-qa-engineer | Separación de casos límite (Boundary) y casos fuera de rango/inválidos en Gherkin. |

