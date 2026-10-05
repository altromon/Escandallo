# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-billing-001.md
# ID Requerimiento: FR-BILLING-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

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
