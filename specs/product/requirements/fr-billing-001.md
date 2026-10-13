---
id: FR-BILLING-001
type: requirement
title: "Control de Cuota Gratuita de 2 Escandallos y Suscripción Ilimitada por Centro con Stripe"
status: draft
version: "1.0.0"
schema-version: "1.0"
category: functional
derives-from:
  - UC-SUBSCRIPTION-001
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - "@FR-BILLING-001"
  - "@automated"
  - "@regression"
supersedes: null
superseded-by: null
---

# Requirement: Control de Cuota Gratuita de 2 Escandallos y Suscripción Ilimitada por Centro con Stripe

## 1. Normative Statement

El sistema DEBE permitir hasta un máximo de 2 escandallos en estado `Activo` de forma gratuita por usuario y centro de estética (permitiendo recalcularlos libremente), bloqueando el alta o duplicación de un tercer escandallo activo hasta contratar una suscripción en Stripe (**14,90 EUR/mes** o **149,00 EUR/año** más IVA por centro); asimismo, si una suscripción se cancela o impaga mientras el centro posee más de 2 escandallos activos, el sistema DEBE bloquear la creación, duplicación, edición y exportación de cualquier escandallo de ese centro hasta que se reactive la suscripción o el usuario pase escandallos a estado **`Archivado`** (Mejora M1) hasta dejar $\le 2$ escandallos activos.

## 2. Acceptance Criteria (Gherkin)

```gherkin
@FR-BILLING-001 @automated @regression
Feature: Control de Cuota Gratuita de 2 Escandallos y Suscripción Ilimitada por Centro con Stripe

  Scenario: Desbloqueo de edición y exportación tras archivar escandallos excedentes sin pérdida de datos
    Given el centro "Centro Sol" tenía suscripción activa con 4 escandallos activos y su suscripción pasa a estado "canceled"
    And el servidor bloquea la edición y exportación de los escandallos de "Centro Sol" con código HTTP 402
    When el propietario pasa 2 de sus escandallos a estado "archived" dejando 2 escandallos "active"
    Then el servidor permite editar, recalcular y exportar los 2 escandallos "active" con código HTTP 200
    And conserva intactos los datos e histórico de los 2 escandallos "archived" para su futura restauración

  Scenario Outline: Verificación de operaciones permitidas según suscripción y número de escandallos activos del centro
    Given un centro de estética tiene estado de suscripción "<estado_suscripcion>" en Stripe y cuenta con <escandallos_activos> escandallos activos
    When el propietario solicita ejecutar la operación "<operacion>" en dicho centro
    Then el resultado de autorización del servidor es "<resultado>" con código HTTP <codigo_http>

    Examples:
      | estado_suscripcion | escandallos_activos | operacion            | resultado | codigo_http |
      | none               | 1                   | crear_escandallo     | permitido | 201         |
      | none               | 2                   | recalcular_existente | permitido | 200         |
      | none               | 2                   | exportar_escandallo  | permitido | 200         |
      | none               | 2                   | duplicar_escandallo  | bloqueado | 402         |
      | canceled           | 4                   | editar_escandallo    | bloqueado | 402         |
      | canceled           | 4                   | archivar_escandallo  | permitido | 200         |
      | canceled           | 2                   | editar_escandallo    | permitido | 200         |
      | active             | 15                  | crear_escandallo     | permitido | 201         |

  @idempotence
  Scenario Outline: Rechazo de contratación o activación ante planes de suscripción inexistentes o importes manipulados
    Given un centro de estética solicita iniciar una sesión de suscripción con el plan "<plan_id>" e importe "<importe_eur>" EUR
    When el servidor valida el catálogo oficial de tarifas de Stripe (14.90 EUR/mes o 149.00 EUR/año)
    Then la solicitud de suscripción retorna estado "<resultado>" con código HTTP <codigo_http>

    Examples:
      | plan_id          | importe_eur | resultado             | codigo_http |
      | PLAN_MENSUAL_EUR | 14.90       | sesion_creada         | 200         |
      | PLAN_ANUAL_EUR   | 149.00      | sesion_creada         | 200         |
      | PLAN_GRATIS_HACK | 0.00        | plan_invalido         | 422         |
      | PLAN_MENSUAL_EUR | 0.01        | importe_no_autorizado | 422         |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del límite freemium de 2 escandallos y suscripción en Stripe. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Recálculo permitido en versión gratuita y bloqueo de edición/exportación cuando se supera el límite gratuito sin suscripción. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de tarifas oficiales (14,90 EUR/mes y 149,00 EUR/año) y Mejora M1 (archivado de escandallos sin pérdida de datos). |
