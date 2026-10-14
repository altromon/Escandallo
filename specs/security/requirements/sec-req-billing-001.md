---
id: SEC-REQ-BILLING-001
type: security-requirement
title: "Control Atómico Anti-Race-Condition de Cuota Freemium y Verificación HMAC de Webhooks Stripe"
status: draft
version: "1.1.0"
schema-version: "1.0"
category: security
priority: critical
derives-from:
  - UC-ESCANDALLO-CALC-001
  - UC-SUBSCRIPTION-001
mitigates-abuse-case:
  - ABUSE-QUOTA-BYPASS-001
enforced-in-enclave: "SEC-ENC-APP-CORE-001"
owasp-asvs: "V11.1, V13.2"
stride-category: "Tampering"
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - "@SEC-REQ-BILLING-001"
  - "@automated"
  - "@regression"
supersedes: null
superseded-by: null
---

# Security Requirement: Control Atómico Anti-Race-Condition de Cuota Freemium y Verificación HMAC de Webhooks Stripe

## 1. Normative Statement

Para mitigar la evasión de límites de negocio (`ABUSE-QUOTA-BYPASS-001`, OWASP ASVS V11.1 / V13.2) en `CMP-BILLING-STRIPE-001` (`SEC-ENC-APP-CORE-001`), el sistema **DEBE** implementar:
1. **Serialización Transaccional del Contador de Escandallos Activos (`ADR-003-MULTITENANT-RLS-AND-ATOMIC-QUOTA`)**: Toda operación que incremente el número de escandallos en estado `active` de un centro desde **Blazor Web App** o API (creación, duplicación `M4` o transición de `archived` a `active` `M1`) **DEBE** ejecutarse dentro de una transacción de PostgreSQL 16 que adquiera un bloqueo exclusivo sobre la fila del centro (`SELECT subscription_tier FROM centers WHERE id = @centerId FOR UPDATE`), verificando que `active_escandallos_count < 2` cuando la suscripción no esté en estado `active` (`Pro`).
2. **Vinculación en Servidor de `metadata.center_id` en Checkout**: Antes de invocar `CreateStripeCheckoutSessionAsync`, el servidor **DEBE** validar mediante `ITenantIsolationGuard` que el usuario autenticado es titular del `centerId`, inyectando dicho UUID de forma inmutable desde el servidor en `SessionCreateOptions.Metadata["center_id"]`.
3. **Verificación Criptográfica sobre Cuerpo Crudo (`Stripe.net`) e Idempotencia Monótona**: El endpoint `/api/v1/billing/webhooks/stripe` **DEBE** acotar el cuerpo crudo a $\le 64\text{ KB}$, validar `Stripe-Signature` mediante `Stripe.EventUtility.ConstructEvent` (`HMAC-SHA256`, ventana máxima de `300 segundos`), registrar cada `event.Id` en `stripe_webhook_events (event_id PRIMARY KEY)` e impedir que eventos atrasados sobrescriban una cancelación más reciente.

## 2. Acceptance Criteria (Gherkin)

```gherkin
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

  Scenario Outline: Rechazo de webhooks de Stripe manipulados, sin firma, repetidos o con metadatos alterados
    Given el endpoint de webhooks de Stripe recibe un evento "<tipo_evento>" para el centro "C_FREE"
    When la cabecera Stripe-Signature presenta la anomalía "<anomalia_firma>" con antigüedad de <edad_segundos> segundos
    Then el sistema rechaza el webhook con estado HTTP <codigo_http>
    And el estado de suscripción del centro "C_FREE" no se modifica
    And se emite una alerta con severidad "SECURITY" en el registro de auditoría

    Examples:
      | tipo_evento                    | anomalia_firma                    | edad_segundos | codigo_http |
      | checkout.session.completed     | firma_hmac_invalida               | 10            | 400         |
      | invoice.paid                   | cabecera_ausente                  | 5             | 400         |
      | checkout.session.completed     | timestamp_expirado_replay         | 600           | 400         |
      | checkout.session.completed     | payload_raw_modificado_en_stream  | 12            | 400         |
      | customer.subscription.updated  | event_id_duplicado                | 15            | 200         |

  @idempotence
  Scenario Outline: Análisis de frontera en límite gratuito de escandallos activos (N-1, N, N+1), cuerpo crudo de webhook (64KB) y ventana HMAC (300s)
    Given el centro "C_FREE" en plan gratuito posee <activos_actuales> escandallos activos y recibe un webhook de <payload_kb> KB con antigüedad HMAC de <edad_hmac_seg> segundos
    When solicita activar un escandallo adicional o procesar el webhook en la frontera del umbral
    Then la activación de escandallo responde con HTTP <http_activacion> y el webhook con HTTP <http_webhook>

    Examples:
      | activos_actuales | payload_kb | edad_hmac_seg | http_activacion | http_webhook |
      | 0                | 63         | 299           | 201             | 200          |
      | 1                | 64         | 300           | 201             | 200          |
      | 2                | 65         | 301           | 402             | 400          |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial de protección anti-race-condition y verificación de webhooks Stripe. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Auditoría técnica .NET 10 LTS + Blazor: blindaje de `Stripe.EventUtility.ConstructEvent` sobre stream crudo, validación de `metadata.center_id` y serialización `FOR UPDATE` multi-circuito. |
