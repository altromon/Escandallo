# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-billing-001.md
# ID Requerimiento: FR-BILLING-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

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
