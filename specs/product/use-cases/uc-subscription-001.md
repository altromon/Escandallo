---
id: UC-SUBSCRIPTION-001
type: use-case
title: "Contratación y Gestión de Suscripción Ilimitada por Centro mediante Stripe"
status: draft
version: "1.0.0"
schema-version: "1.0"
primary-actor: ACT-ESTHETIC-USER-001
governed-by:
  - BR-TENANT-ISOLATION-001
  - BR-FREEMIUM-QUOTA-001
supersedes: null
superseded-by: null
---

# Use Case: Contratación y Gestión de Suscripción Ilimitada por Centro mediante Stripe

## 1. Intent and Outcome

As a `ACT-ESTHETIC-USER-001` I want to subscribe a specific beauty center through Stripe once I reach the 2 free escandallo calculations limit To unlock unlimited escandallo calculations for that beauty center.

## 2. Preconditions

- El usuario está autenticado y es propietario del centro de estética para el que desea activar la suscripción.

## 3. Main Flow

1. El usuario alcanza los 2 escandallos activos gratuitos en su centro de estética o selecciona directamente "Suscribir Centro" eligiendo entre el **Plan Mensual (14,90 EUR/mes + IVA)** o el **Plan Anual (149,00 EUR/año + IVA)** por centro de estética.
2. El servidor crea una sesión de pago segura en `ACT-STRIPE-001` (Stripe Checkout) vinculada al identificador del centro de estética (`center_id`), al plan elegido y al cliente.
3. El usuario completa el pago en la pasarela de Stripe.
4. `ACT-STRIPE-001` envía un evento webhook firmado (`checkout.session.completed` / `customer.subscription.updated`) al servidor.
5. El servidor verifica la firma criptográfica del webhook, activa la suscripción ilimitada para ese centro de estética y desbloquea la creación, duplicación, edición, exportación y restauración en 1 clic de escandallos archivados.

## 4. Alternative and Exception Flows

- **E1 – Pago rechazado o cancelado**: La suscripción no se activa y el centro mantiene el límite gratuito de 2 escandallos activos.
- **E2 – Cancelación o impago posterior y Archivado sin Pérdida de Datos (`customer.subscription.deleted` / `past_due`, Mejora M1)**: El servidor actualiza el estado de la suscripción del centro y, mientras el número de escandallos activos del centro supere el límite gratuito ($> 2$ activos), bloquea la creación, duplicación, edición y exportación de cualquier escandallo. El usuario puede pasar escandallos a estado **`Archivado` (`archived`)** hasta dejar $\le 2$ escandallos `Activos` (conservando intactos los datos e histórico de los archivados) o reactivar su suscripción en Stripe.

## 5. Postconditions

- El centro de estética suscrito dispone de cálculos, ediciones y exportaciones de escandallo ilimitados mientras la suscripción en Stripe permanezca activa.

## 6. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del caso de uso de suscripción con Stripe. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Bloqueo de edición y exportación de escandallos cuando se supera el límite gratuito tras cancelación/impago. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de tarifas oficiales (14,90 EUR/mes y 149,00 EUR/año) y archivado sin pérdida de datos (Mejora M1). |
