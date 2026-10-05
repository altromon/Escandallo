---
id: ABUSE-QUOTA-BYPASS-001
type: abuse-case
title: "Evasión de Cuota Freemium mediante Condiciones de Carrera y Spoofing de Webhooks Stripe"
status: draft
version: "1.0.0"
schema-version: "1.0"
primary-threat-actor: ACT-THREAT-TENANT-001
targets-use-cases:
  - UC-ESCANDALLO-CALC-001
  - UC-SUBSCRIPTION-001
stride-category: "Tampering"
severity: "HIGH"
likelihood: "HIGH"
mitigated-by:
  - SEC-REQ-BILLING-001
supersedes: null
superseded-by: null
---

# Abuse Case: Evasión de Cuota Freemium mediante Condiciones de Carrera y Spoofing de Webhooks Stripe

## 1. Attack Scenario & Preconditions

Un usuario en el plan gratuito (`ACT-THREAT-TENANT-001`), limitado por `BR-FREEMIUM-QUOTA-001` a un máximo de 2 escandallos en estado `active` por centro, busca crear, duplicar o desarchivar un número ilimitado de escandallos sin abonar la suscripción de Stripe (**14,90 EUR/mes** o **149,00 EUR/año**).

## 2. Attack Flow (Step-by-Step)

1. **Vector A (Race Condition TOCTOU)**: Con 1 escandallo activo en su centro, el atacante lanza 20 peticiones HTTP concurrentes de creación (`POST /escandallos`), duplicación (`M4`) o cambio de estado de `archived` a `active` (`M1`). Si la verificación `COUNT(status='active') < 2` no es atómica ni bloquea el registro del centro, las 20 transacciones leen `count = 1` simultáneamente y persisten 21 escandallos activos.
2. **Vector B (Webhook Spoofing / Replay)**: El atacante envía una petición `POST /webhooks/stripe` falsificando un evento `checkout.session.completed` sin firma criptográfica válida o reenviando un payload antiguo para activar el estado `active` de la suscripción de su centro.

## 3. Business & Security Impact

- **Pérdida Directa de Ingresos (Fraude SaaS)**: Uso ilimitado de la plataforma sin pago de suscripción.
- Inconsistencia de estado financiero entre Stripe y la base de datos de la aplicación.

## 4. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del caso de abuso de evasión de cuota freemium y webhooks. |
