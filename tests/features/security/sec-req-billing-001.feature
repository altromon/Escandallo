# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/security/requirements/sec-req-billing-001.md
# ID Requerimiento: SEC-REQ-BILLING-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@SEC-REQ-BILLING-001 @automated @regression @security @mitigation
Feature: Integridad transaccional de cuota freemium y autenticidad de eventos de pago Stripe
  Como motor de reglas de negocio y facturación (SEC-ENC-APP-CORE-001)
  Quiero serializar las transiciones a estado activo y verificar criptográficamente los webhooks de Stripe
  Para impedir que un inquilino supere los 2 escandallos gratuitos o active suscripciones falsas

  Scenario: Bloqueo de condición de carrera concurrente al crear o desarchivar escandallos en plan gratuito
    Given el centro "C_FREE" tiene plan gratuito sin suscripción activa y posee 1 escandallo en estado "active"
    And el centro "C_FREE" posee además 2 escandallos en estado "archived"
    When el usuario lanza 5 peticiones concurrentes en el mismo milisegundo para crear o desarchivar escandallos en "C_FREE"
    Then exactamente 1 petición finaliza con éxito dejando el total de escandallos en estado "active" en 2
    And las 4 peticiones restantes son rechazadas con código HTTP 402
    And se registra un evento de seguridad por intento de desbordamiento concurrente de cuota

  Scenario Outline: Rechazo de webhooks de Stripe manipulados, sin firma o repetidos
    Given el endpoint de webhooks de Stripe recibe un evento "<tipo_evento>" para el centro "C_FREE"
    When la cabecera Stripe-Signature presenta la anomalía "<anomalia_firma>" con antigüedad de <edad_segundos> segundos
    Then el sistema rechaza el webhook con estado HTTP <codigo_http>
    And el estado de suscripción del centro "C_FREE" no se modifica
    And se emite una alerta con severidad "SECURITY" en el registro de auditoría

    Examples:
      | tipo_evento                    | anomalia_firma              | edad_segundos | codigo_http |
      | checkout.session.completed     | firma_hmac_invalida         | 10            | 400         |
      | invoice.paid                   | cabecera_ausente            | 5             | 400         |
      | checkout.session.completed     | timestamp_expirado_replay   | 600           | 400         |
      | customer.subscription.updated  | event_id_duplicado          | 15            | 200         |
