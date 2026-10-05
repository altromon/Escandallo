# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/qr-sec-001.md
# ID Requerimiento: QR-SEC-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@QR-SEC-001 @automated @regression
Feature: Seguridad de Servidor frente a Ciberataques, Protección IDOR y Verificación de Webhooks

  Scenario: Rechazo de webhook de Stripe falsificado sin firma criptográfica válida
    Given un atacante externo envía una petición POST al endpoint de webhook de Stripe simulando el pago de "Centro Madrid"
    And la cabecera "Stripe-Signature" es inválida o ha sido manipulada
    When el servidor procesa la petición entrante
    Then el servidor rechaza la petición con código HTTP 400 Bad Request
    And el estado de suscripción de "Centro Madrid" permanece sin cambios
    And se registra una alerta de seguridad en los logs de auditoría

  Scenario Outline: Mitigación activa de vectores de ataque comunes contra el servidor
    Given un atacante envía tráfico malicioso de tipo "<vector_ataque>" contra el endpoint "<endpoint>"
    When el servidor evalúa la petición en la capa de seguridad
    Then la petición es bloqueada con código HTTP <codigo_http> sin exponer trazas internas ni datos de otros clientes

    Examples:
      | vector_ataque                | endpoint                     | codigo_http |
      | fuerza_bruta_credenciales    | /api/auth/login              | 429         |
      | inyeccion_payload_malformado | /api/centers/costs           | 400         |
      | manipulacion_idor_tenant     | /api/centers/other-id/report | 403         |
      | falsificacion_firma_stripe   | /api/webhooks/stripe         | 400         |
