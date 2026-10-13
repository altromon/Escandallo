---
id: ABUSE-QUOTA-BYPASS-001
type: abuse-case
title: "Evasión de Cuota Freemium mediante Condiciones de Carrera y Spoofing de Webhooks Stripe"
status: draft
version: "1.1.0"
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

Un usuario en el plan gratuito (`ACT-THREAT-TENANT-001`), limitado por `BR-FREEMIUM-QUOTA-001` a un máximo de 2 escandallos en estado `active` por centro, busca crear, duplicar o desarchivar un número ilimitado de escandallos sin abonar la suscripción de Stripe (**14,90 EUR/mes** o **149,00 EUR/año**) atacando `CMP-BILLING-STRIPE-001` en `SEC-ENC-APP-CORE-001`.

## 2. Attack Flow (Step-by-Step)

1. **Vector A (Race Condition TOCTOU entre Circuitos Blazor y Peticiones HTTP)**: Con 1 escandallo activo en su centro, el atacante abre múltiples pestañas/circuitos de Blazor (`FreemiumPaywallModal.razor` / `EscandalloBuilderPage.razor`) y lanza 20 invocaciones simultáneas de creación, duplicación (`M4`) o transición `archived -> active` (`M1`). Si `IFreemiumQuotaService` no adquiere un bloqueo pesimista `SELECT ... FOR UPDATE` sobre la fila del centro en PostgreSQL 16 antes de evaluar `COUNT(status='active') < 2`, las 20 transacciones observan `count = 1` y persisten 21 escandallos activos.
2. **Vector B (Webhook Spoofing, Replay y Alteración de Stream en Kestrel)**: El atacante envía una petición `POST /api/v1/billing/webhooks/stripe` falsificando un evento `checkout.session.completed` sin firma válida, reenviando un payload capturado hace más de `300 segundos`, o enviando un cuerpo sobredimensionado/malformado antes de `Stripe.EventUtility.ConstructEvent`.
3. **Vector C (Manipulación de `metadata.center_id` y Eventos Desordenados de Cancelación)**: El atacante intenta inyectar otro `center_id` al invocar `CreateStripeCheckoutSessionAsync` o provoca una condición de carrera entre los webhooks `checkout.session.completed` y `customer.subscription.deleted` para retener el estado `Pro` con más de 2 escandallos activos tras cancelar el pago.

## 3. Business & Security Impact

- **Pérdida Directa de Ingresos (Fraude SaaS)**: Uso ilimitado de la plataforma sin pago de suscripción.
- Inconsistencia de estado financiero entre Stripe y la base de datos PostgreSQL 16 de la aplicación.

## 4. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del caso de abuso de evasión de cuota freemium y webhooks. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Incorporación de vectores técnicos sobre concurrencia multi-circuito Blazor, metadatos Stripe Checkout y orden de eventos. |
