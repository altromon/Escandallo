---
id: ADR-003-MULTITENANT-RLS-AND-ATOMIC-QUOTA
type: architecture-decision-record
title: "Aislamiento Multi-Tenant con PostgreSQL Row-Level Security (RLS) y Bloqueo Pesimista para Cuota Freemium de Stripe"
status: proposed
version: "1.0.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
  - "agent-threat-modeler"
decision-date: "2026-10-05"
affects-components:
  - CMP-IAM-TENANT-001
  - CMP-COST-ENGINE-001
  - CMP-BILLING-STRIPE-001
supersedes: null
superseded-by: null
---

# ADR-003: Aislamiento Multi-Tenant con PostgreSQL Row-Level Security (RLS) y Bloqueo Pesimista para Cuota Freemium de Stripe

## 1. Context and Problem Statement

El sistema aloja múltiples clientes donde cada usuario puede gestionar uno o varios centros de estética (`FR-TENANT-001`), incluyendo la capacidad de copiar catálogos de productos entre sus propios centros (`M4`). El modelo de amenazas identifica dos vectores críticos:
1. **`ABUSE-IDOR-TENANT-001` (`SEC-REQ-TENANT-001`)**: Exfiltración cross-tenant de salarios, costes de cosméticos o informes ejecutivos si un desarrollador olvida un filtro `WHERE center_id = $1` o si se manipula `source_center_id` en la copia de catálogo.
2. **`ABUSE-QUOTA-BYPASS-001` (`SEC-REQ-BILLING-001`)**: Evasión del límite gratuito de **2 escandallos en estado `active` por centro** (`BR-FREEMIUM-QUOTA-001`) mediante peticiones concurrentes (*race condition* TOCTOU) al crear, duplicar (`M4`) o desarchivar (`M1`) escandallos, o mediante falsificación/repetición de webhooks de Stripe.

## 2. Considered Technology Options

1. **Opción A (Elegida): Base de Datos Compartida con `PostgreSQL Row-Level Security (RLS)` Forzado + Bloqueo Transaccional `SELECT ... FOR UPDATE` en la Fila del Centro**:
   - Cada tabla de negocio (`costs`, `cost_history`, `escandallos`, `escandallo_items`, `escandallo_history`, `subscriptions`) incluye `center_id UUID NOT NULL` indexado y `ENABLE / FORCE ROW LEVEL SECURITY`.
   - Un middleware transaccional en `CMP-IAM-TENANT-001` verifica que el `user_id` autenticado pertenece al `center_id` solicitado e inyecta `SET LOCAL app.current_center_id = '<uuid>'` al inicio de cada transacción.
   - En cualquier transición hacia `status = 'active'` de un escandallo, `CMP-BILLING-STRIPE-001` ejecuta `SELECT id, subscription_status FROM centers WHERE id = $1 FOR UPDATE`, serializando las peticiones concurrentes de ese centro.
2. **Opción B: Filtrado Exclusivamente en Capa de Aplicación (ORM Where Clauses)**:
   - **Contras**: Un único olvido en una consulta de reporte o agregación expone datos de todos los centros de estética. Insuficiente para el nivel de seguridad requerido (`SEC-REQ-TENANT-001`).
3. **Opción C: Base de Datos o Esquema PostgreSQL Separado por cada Centro de Estética**:
   - **Contras**: Complejidad operativa extrema al ejecutar migraciones de esquema sobre cientos de centros y sobrecarga de memoria en el catálogo de PostgreSQL, perjudicando la rentabilidad del despliegue (`ADR-001`).

## 3. Decision Outcome

Se elige la **Opción A (Defensa en Profundidad: Validación en Capa de Aplicación + PostgreSQL RLS Forzado + Bloqueo Pesimista de Fila en Cuota Freemium)**.

### Mecanismo Técnico de Control

1. **Aislamiento RLS**: Ninguna consulta de lectura o escritura sobre tablas de negocio puede devolver o alterar filas cuyo `center_id` difiera de `current_setting('app.current_center_id', true)::uuid`. Para la operación de copia de catálogo `M4` (`source_center_id` $\rightarrow$ `target_center_id`), un procedimiento transaccional verifica explícitamente en `center_memberships` que el `user_id` posee rol de propietario en ambos centros antes de clonar los registros.
2. **Control Atómico de Cuota (`BR-FREEMIUM-QUOTA-001`)**:
   - Antes de insertar un escandallo nuevo, duplicar uno existente (`M4`) o cambiar su estado de `archived` a `active` (`M1`), la transacción bloquea la fila del centro (`FOR UPDATE`).
   - Si `subscription_status != 'active'` y `COUNT(escandallos WHERE center_id = $1 AND status = 'active') >= 2`, la transacción hace `ROLLBACK` y devuelve `HTTP 402 Payment Required`.
   - Si una suscripción pasa a cancelada/impagada teniendo $>2$ escandallos activos, se bloquea cualquier creación, edición o exportación hasta que el usuario archive escandallos hasta quedar en $\le 2$ activos o reactive su suscripción.
3. **Verificación e Idempotencia de Stripe**: El endpoint `POST /api/v1/billing/webhooks/stripe` verifica la firma `Stripe-Signature` con `stripe.webhooks.constructEvent(rawBody, sig, webhookSecret, 300)` e inserta `event.id` en la tabla `stripe_webhook_events (event_id TEXT PRIMARY KEY, processed_at TIMESTAMPTZ)` dentro de la misma transacción que actualiza el estado del centro.

## 4. Consequences

- **Positive**:
  - Inmunidad estructural frente a vulnerabilidades IDOR/BOLA y frente a condiciones de carrera en el límite gratuito de 2 escandallos activos.
- **Negative / Trade-offs**:
  - Todas las consultas de dominio deben ejecutarse a través del *Transaction Wrapper* de `CMP-IAM-TENANT-001` que inicializa las variables de sesión locales de PostgreSQL.

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Formal decision proposal | ARCH-INIT-001 |
