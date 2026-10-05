---
id: SEC-REQ-BILLING-001
type: security-requirement
title: "Control Atómico Anti-Race-Condition de Cuota Freemium y Verificación HMAC de Webhooks Stripe"
status: draft
version: "1.0.0"
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

Para mitigar la evasión de límites de negocio (`ABUSE-QUOTA-BYPASS-001`, OWASP ASVS V11.1 / V13.2), el sistema **DEBE** implementar:
1. **Serialización Transaccional del Contador de Escandallos Activos**: Toda operación que incremente el número de escandallos en estado `active` de un centro (creación, duplicación `M4` o transición de `archived` a `active` `M1`) **DEBE** ejecutarse dentro de una transacción de base de datos que adquiera un bloqueo exclusivo sobre la fila del centro (`SELECT ... FOR UPDATE` en el registro de `Center`), verificando que `active_escandallos_count < 2` cuando la suscripción no esté en estado `active`.
2. **Verificación Criptográfica e Idempotencia de Webhooks de Stripe**: El endpoint receptor de webhooks **DEBE** validar la cabecera `Stripe-Signature` mediante HMAC-SHA256 sobre el cuerpo crudo (*raw body*), rechazar marcas de tiempo con una desviación superior a `300 segundos` (5 minutos) para impedir ataques de repetición (*replay*), y registrar cada `event.id` de Stripe en una tabla con restricción `UNIQUE` de idempotencia.

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
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial de protección anti-race-condition y verificación de webhooks Stripe. |
