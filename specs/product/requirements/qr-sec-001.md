---
id: QR-SEC-001
type: requirement
title: "Seguridad de Servidor frente a Ciberataques, Protección IDOR y Verificación de Webhooks"
status: draft
version: "1.0.0"
schema-version: "1.0"
category: quality
derives-from:
  - UC-SUBSCRIPTION-001
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - "@QR-SEC-001"
  - "@automated"
  - "@regression"
supersedes: null
superseded-by: null
---

# Requirement: Seguridad de Servidor frente a Ciberataques, Protección IDOR y Verificación de Webhooks

## 1. Normative Statement

El servidor DEBE aplicar defensa en profundidad frente a atacantes cibernéticos mediante limitación de tasa de peticiones (*rate limiting*), validación estricta de esquemas de entrada, prevención de referencias directas inseguras a objetos (IDOR) entre centros de estética, cabeceras de seguridad HTTP (CSP, HSTS) y verificación criptográfica obligatoria de firmas `Stripe-Signature` en todos los webhooks de pago antes de modificar el estado de suscripción de cualquier centro.

## 2. Acceptance Criteria (Gherkin)

```gherkin
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

  @security @mitigation
  Scenario Outline: Análisis de frontera en ventana temporal de firma de webhook (300s) y límite de peticiones por minuto
    Given una petición llega al servidor con antigüedad de firma <edad_firma_seg> segundos y tasa de <peticiones_minuto> peticiones/minuto
    When el middleware de seguridad evalúa las fronteras de tolerancia (máximo 300s y 30 req/min)
    Then el servidor responde con código HTTP <codigo_http>

    Examples:
      | edad_firma_seg | peticiones_minuto | codigo_http |
      | 299            | 29                | 200         |
      | 300            | 30                | 200         |
      | 301            | 10                | 400         |
      | 50             | 31                | 429         |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del requisito de calidad de ciberseguridad en servidor. |
