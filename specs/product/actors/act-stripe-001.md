---
id: ACT-STRIPE-001
type: actor
title: "Pasarela de Pagos y Suscripciones (Stripe)"
status: draft
version: "1.0.0"
schema-version: "1.0"
supersedes: null
superseded-by: null
---

# Actor: Pasarela de Pagos y Suscripciones (Stripe)

## 1. Profile and Role Description

Sistema externo de procesamiento de pagos y gestión de suscripciones recurrentes (Stripe Checkout / Stripe Billing) que gestiona el cobro por centro de estética para desbloquear cálculos de escandallo ilimitados.

## 2. Responsibilities and Capabilities

- Procesar el alta, renovación, fallo de cobro y cancelación de suscripciones asociadas a un centro de estética.
- Notificar de forma asíncrona y firmada criptográficamente (webhooks) los cambios de estado de la suscripción al servidor.

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del actor externo Stripe. |
