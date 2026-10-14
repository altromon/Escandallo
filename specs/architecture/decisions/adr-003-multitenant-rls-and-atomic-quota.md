---
id: ADR-003-MULTITENANT-RLS-AND-ATOMIC-QUOTA
type: architecture-decision-record
title: "Aislamiento Multi-Tenant con PostgreSQL FORCE RLS y Control Atómico de Cuota Freemium con SELECT FOR UPDATE"
status: proposed
version: "1.1.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
decision-date: "2026-10-08"
affects-components:
  - CMP-IAM-TENANT-001
  - CMP-WEB-UI-001
  - CMP-BILLING-STRIPE-001
  - CMP-COST-ENGINE-001
supersedes: null
superseded-by: null
---

# ADR-003: Aislamiento Multi-Tenant con PostgreSQL FORCE RLS y Control Atómico de Cuota Freemium con SELECT FOR UPDATE

## 1. Contexto y Planteamiento del Problema

Dos de las amenazas más críticas modeladas en `specs/security/` son:
1. **`ABUSE-IDOR-TENANT-001` (`SEC-REQ-TENANT-001`)**: Acceso horizontal no autorizado a catálogos o escandallos de un centro competidor manipulando identificadores (`center_id`, `escandallo_id`), alterando propiedades `[Parameter]` / `[SupplyParameterFromForm]` en circuitos Blazor (`/_blazor`), abusando de la copia dual entre centros (`M4`) o explotando conexiones reutilizadas en el pool de `Npgsql` (`RSK-RLS-POOL-LEAK-004`).
2. **`ABUSE-QUOTA-BYPASS-001` (`SEC-REQ-BILLING-001`)**: Evasión del límite gratuito de `2` escandallos activos (`BR-FREEMIUM-QUOTA-001`) mediante ráfagas de peticiones concurrentes entre múltiples pestañas/circuitos de creación, duplicación (`M4`) o desarchivado lógico (`M1`), manipulación de `metadata.center_id` en Checkout o falsificación/desorden de webhooks de Stripe.

## 2. Opciones Tecnológicas Consideradas

1. **Opción A (Recomendada — Defensa en Profundidad: `ITenantIsolationGuard` en Servidor + PostgreSQL 16 `FORCE ROW LEVEL SECURITY` con `SET LOCAL` y `NOBYPASSRLS` + Bloqueo Pesimista `SELECT ... FOR UPDATE` + Stripe Raw Stream HMAC e Idempotencia Monótona)**:
   - Una sola base de datos PostgreSQL 16 compartida con columna obligatoria `center_id UUID NOT NULL`, conectada mediante un rol `NOSUPERUSER NOBYPASSRLS`, protegida simultáneamente por revalidación en servidor en C# y por políticas nativas `ENABLE` + `FORCE ROW LEVEL SECURITY` *fail-closed* basadas en `SELECT set_config('app.current_center_id', @centerId::text, true)` (`is_local = true`), más bloqueo de fila `SELECT ... FOR UPDATE` sobre `centers` al modificar escandallos activos.
2. **Opción B (Filtro Exclusivo en Capa de Aplicación / ORM `HasQueryFilter`)**:
   - Si un desarrollador olvida el filtro en una consulta SQL nativa o Dapper, se produce una fuga masiva de datos entre tenants. No bloquea condiciones de carrera TOCTOU sin bloqueos explícitos.
3. **Opción C (Base de Datos o Esquema Físico Separado por Centro)**:
   - Eleva drásticamente el coste de memoria y mantenimiento de migraciones cuando existan cientos de centros en el plan gratuito.

## 3. Decisión Adoptada

Se elige la **Opción A (Doble Barrera `ITenantIsolationGuard` + PostgreSQL 16 `FORCE RLS` con `SET LOCAL` + `SELECT ... FOR UPDATE` + Idempotencia Monótona Stripe)**.

**Justificación técnica (`SEC-REQ-TENANT-001`, `SEC-REQ-BILLING-001`)**:
- **Imposibilidad Matemática de Fuga Cruzada o Contaminación en Pool `Npgsql`**: Al conectar con un rol sin `SUPERUSER` ni `BYPASSRLS` y fijar `set_config('app.current_center_id', @centerId::text, true)` (`is_local = true` ligado a la transacción), el contexto desaparece automáticamente al confirmar o abortar la transacción (`RSK-RLS-POOL-LEAK-004`). Si la variable es nula o vacía, la política `FORCE RLS` actúa en modo *fail-closed* devolviendo `0` filas.
- **Revalidación Continua en Blazor y Copia Dual `M4`**: `ITenantIsolationGuard` extrae siempre el `userId` desde `AuthenticationStateProvider` en servidor, ignorando parámetros del cliente, y en la copia `M4` exige titularidad simultánea en `source_center_id` y `target_center_id` clonando por valor con nuevos `UUIDv4`.
- **Serialización Atómica de Cuota Freemium Multi-Circuito**: Al ejecutar `SELECT subscription_tier FROM centers WHERE id = @centerId FOR UPDATE` al inicio de cualquier creación, duplicación (`M4`) o transición `archived -> active` (`M1`), las peticiones concurrentes se serializan en PostgreSQL 16 garantizando `COUNT(active) <= 2` en plan `Free` y rechazando el excedente con `402 Payment Required` y alerta `SECURITY`.
- **Integridad de Checkout y Webhooks Stripe**: El servidor vincula de forma inmutable `SessionCreateOptions.Metadata["center_id"]` previa validación en `ITenantIsolationGuard`. El endpoint `/api/v1/billing/webhooks/stripe` acota el cuerpo crudo a $\le 64\text{ KB}$, valida `Stripe.EventUtility.ConstructEvent` (`HMAC-SHA256`, ventana $\le 300\text{ s}$), registra `event_id PRIMARY KEY` en `stripe_webhook_events` e impone **idempotencia monótona** para impedir que eventos atrasados sobrescriban una cancelación más reciente.

## 4. Consecuencias

- **Positivas**:
  - Cumplimiento riguroso de `SEC-REQ-TENANT-001`, `SEC-REQ-BILLING-001` y `QR-SEC-001` con sobrecoste de latencia inferior a `1 ms`.
- **Negativas / Compensaciones (Trade-offs)**:
  - Toda unidad de trabajo sobre tablas multi-tenant debe ejecutarse dentro de una transacción que invoque el interceptor `SET LOCAL app.current_center_id`.

---

## 5. Historial de Revisiones y Control de Versiones

| Versión | Fecha | Decisores | Estado de la Decisión | Referencia (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-04-12 | `agent-system-architect` | Propuesta formal aceptada | `chg-001-escandallo-mvp` |
| **1.1.0** | 2026-10-08 | `agent-system-architect` | Incorporación de rol `NOBYPASSRLS`, `SET LOCAL` *fail-closed*, vinculación de `metadata.center_id` e idempotencia monótona (`SEC-REQ-TENANT-001`, `SEC-REQ-BILLING-001` v1.1.0) | `chg-001-escandallo-mvp` |
