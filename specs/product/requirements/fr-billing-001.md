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
@FR-BILLING-001 @automated @regression @idempotence
Feature: Control de Cuota Gratuita de 2 Escandallos y Suscripción Ilimitada por Centro con Stripe

  Scenario: Desbloqueo de edición y exportación tras archivar escandallos excedentes sin pérdida de datos
    Given el centro "Centro Sol" tenía suscripción activa con 4 escandallos activos y su suscripción pasa a estado "canceled"
    And el servidor bloquea la edición y exportación de los escandallos de "Centro Sol" con código HTTP 402
    When el propietario pasa 2 de sus escandallos a estado "archived" dejando 2 escandallos "active"
    Then el servidor permite editar, recalcular y exportar los 2 escandallos "active" con código HTTP 200
    And conserva intactos los datos e histórico de los 2 escandallos "archived" para su futura restauración

  @boundary
  Scenario Outline: Verificación de operaciones permitidas en las fronteras exactas de 0, 1 y 2 escandallos activos
    Given un centro de estética tiene estado de suscripción "<estado_suscripcion>" en Stripe y cuenta con <escandallos_activos> escandallos activos
    When el propietario solicita ejecutar la operación "<operacion>" en dicho centro
    Then el resultado de autorización del servidor es "<resultado>" con código HTTP <codigo_http>

    Examples:
      | estado_suscripcion | escandallos_activos | operacion            | resultado | codigo_http |
      | none               | 0                   | crear_escandallo     | permitido | 201         |
      | none               | 1                   | crear_escandallo     | permitido | 201         |
      | none               | 2                   | recalcular_existente | permitido | 200         |
      | none               | 2                   | exportar_escandallo  | permitido | 200         |
      | canceled           | 4                   | archivar_escandallo  | permitido | 200         |
      | canceled           | 2                   | editar_escandallo    | permitido | 200         |
      | active             | 15                  | crear_escandallo     | permitido | 201         |

  @invalid
  Scenario Outline: Bloqueo de operaciones fuera de cuota gratuita o planes de suscripción inválidos
    Given un centro de estética tiene estado de suscripción "<estado_suscripcion>" en Stripe y cuenta con <escandallos_activos> escandallos activos
    When el propietario intenta ejecutar la operación no permitida "<operacion>" con parámetro "<parametro>"
    Then el servidor deniega la operación con código HTTP <codigo_http> y código de error "<error_code>"

    Examples:
      | estado_suscripcion | escandallos_activos | operacion              | parametro        | codigo_http | error_code                 |
      | none               | 2                   | crear_tercer_escandallo| plan_free        | 402         | FREE_QUOTA_LIMIT_REACHED   |
      | none               | 2                   | duplicar_escandallo    | plan_free        | 402         | FREE_QUOTA_LIMIT_REACHED   |
      | none               | 2                   | desarchivar_escandallo | plan_free        | 402         | FREE_QUOTA_LIMIT_REACHED   |
      | canceled           | 4                   | editar_escandallo      | sub_canceled     | 402         | SUBSCRIPTION_REQUIRED_LOCK |
      | past_due           | 3                   | exportar_escandallo    | sub_past_due     | 402         | SUBSCRIPTION_REQUIRED_LOCK |
      | none               | 2                   | iniciar_checkout_stripe| plan_inexistente | 400         | INVALID_SUBSCRIPTION_PLAN  |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del límite freemium de 2 escandallos y suscripción en Stripe. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Recálculo permitido en versión gratuita y bloqueo de edición/exportación cuando se supera el límite gratuito sin suscripción. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de tarifas oficiales (14,90 EUR/mes y 149,00 EUR/año) y Mejora M1 (archivado de escandallos sin pérdida de datos). |
| 1.3.0 | 2026-10-05 | agent-qa-engineer | Descomposición BDD en escenario nominal, casos límite de cuota (0, 1, 2) y casos inválidos/bloqueados (HTTP 402/400). |

