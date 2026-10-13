---
id: ARCH-STRAT-001
type: solution-strategy
title: "04. Estrategia de Solución (arc42 Sec. 4 / NAF Strategy)"
status: proposed
version: "1.1.0"
schema-version: "1.0"
arc42-section: 4
naf-perspective: "Service & Resource Strategy"
strategies:
  - STRAT-ARCH-MODULAR-MONOLITH-001
  - STRAT-UI-BLAZOR-HYBRID-002
  - STRAT-DATA-RLS-DECIMAL-003
  - STRAT-SEC-DEFENSE-IN-DEPTH-004
  - STRAT-FINOPS-SINGLE-VPS-005
supersedes: null
superseded-by: null
---

# 04. Estrategia de Solución (arc42 Sec. 4 / NAF Strategy)

## 1. Decisiones Estructurales Fundamentales (`STRAT-*`)

Describe las elecciones arquitectónicas globales que dan forma al sistema y su correspondencia con los registros de decisión (`ADR-*`):

| ID Estrategia | Patrón / Decisión Fundamental | Racional Técnico, Compensaciones (Trade-offs) y ADR Vinculado |
| :--- | :--- | :--- |
| `STRAT-ARCH-MODULAR-MONOLITH-001` | **Monolito Modular Hexagonal en .NET 10 LTS** | Aísla el dominio puro de cálculo financiero (`CMP-COST-ENGINE-001`) como ensamblado C# sin dependencias de infraestructura, alojando todos los módulos en un único proceso `Kestrel` para eliminar latencia de red entre servicios y sobrecoste operativo de Kubernetes (`ADR-002-MODULAR-MONOLITH-STACK`). |
| `STRAT-UI-BLAZOR-HYBRID-002` | **UI Unificada en ASP.NET Core Blazor Web App con Circuitos Acotados** | Comparte modelos de validación y tipos `decimal` entre la interfaz (`CMP-WEB-UI-001`) y el motor de dominio en C#. Combina *Server-Side Rendering (SSR)* con *Interactive Server* acotado (`<= 5` circuitos/usuario, `<= 32 KB` por mensaje, `<= 100` líneas/servicio) y revalida continuamente `AuthenticationStateProvider` en servidor (`QR-UI-A11Y-001`, `SEC-REQ-TENANT-001`, `SEC-REQ-DOS-001`). |
| `STRAT-DATA-RLS-DECIMAL-003` | **PostgreSQL 16 con `FORCE RLS` (`SET LOCAL` + `NOBYPASSRLS`) y `NUMERIC(18,6)`** | Impone aislamiento multi-tenant en dos capas independientes (`ITenantIsolationGuard` + política `FORCE ROW LEVEL SECURITY` *fail-closed* con `SELECT set_config('app.current_center_id', @centerId::text, true)` bajo rol `NOSUPERUSER NOBYPASSRLS`) y garantiza exactitud financiera con `NUMERIC(18,6)`, guardas anti-`OverflowException` y bloqueos `SELECT ... FOR UPDATE` (`ADR-003-MULTITENANT-RLS-AND-ATOMIC-QUOTA`, `ADR-004-EXACT-DECIMAL-AND-CASCADE-ENGINE`). |
| `STRAT-SEC-DEFENSE-IN-DEPTH-004` | **Procesamiento Documental Defensivo (`BoundedReadStream`) y Telemetría Inmutable** | Descomprime `.xlsx` a través de `BoundedReadStream` ($\le 10\text{ MB}$ reales sobre `RecyclableMemoryStream`), prohíbe DTD/XXE, neutraliza fórmulas DDE (`TrimStart` + `'` + `XLDataType.Text`), emplea `ClosedXML` (`MIT`) + `PDFsharp` (`MIT`) bajo `SemaphoreSlim(2, 2)`, y aplica `ToMaximumDepth(3)`, redacción en `Exception.Message` y canal `SECURITY` anti-evicción (`ADR-005-DUAL-OBSERVABILITY-AND-SAFE-DOCUMENTS`). |
| `STRAT-FINOPS-SINGLE-VPS-005` | **Despliegue Ultra-Rentable Edge WAF + Cloud VPS Europeo con mTLS `KnownProxies`** | Combina Cloudflare Edge WAF y Caddy mTLS (*Authenticated Origin Pulls* + `KnownProxies` contra spoofing de `X-Forwarded-For`) en un servidor Cloud VPS europeo (Hetzner CX22/CPX21 con Docker Compose) por $< 12\text{ EUR/mes}$ totales, logrando *break-even* con 1 sola suscripción Pro (`14,90 EUR/mes`) (`ADR-001-COST-EFFECTIVE-DEPLOYMENT`). |

---

## 2. Principios de Descomposición y Diseño

- **Arquitectura Hexagonal (Puertos y Adaptadores)**: El núcleo matemático (`CMP-COST-ENGINE-001`) no conoce bases de datos, HTTP ni librerías de Excel/PDF; expone contratos puros (`ICostCatalogService`, `IEscandalloCalculatorService`) consumidos por la UI Blazor (`CMP-WEB-UI-001`) y los adaptadores de infraestructura.
- **Inmutabilidad Histórica Condicional (`BR-RECALC-HISTORY-001`)**: Los cambios de catálogo actualizan en cascada el borrador activo de los tratamientos dependientes, pero solo persisten una nueva instantánea (`EscandalloSnapshot`) cuando existe una variación matemática real ($\Delta \text{Coste} \ne 0 \lor \Delta \text{Margen} \ne 0$).
- **Seguridad y Privacidad por Defecto (Zero-Trust)**: Ninguna acción de componente Blazor confía en parámetros del cliente sin revalidar `AuthenticationStateProvider`, ninguna consulta SQL se ejecuta sin transacción `SET LOCAL app.current_center_id`, y ningún evento de telemetría se serializa sin atravesar el filtro inmutable `[REDACTED]` (`CMP-OBS-TELEMETRY-001`).

---

## 3. Matriz de Cumplimiento de Objetivos de Calidad

| Objetivo de Calidad | Estrategia Adoptada | Mecanismo de Garantía Verificable |
| :--- | :--- | :--- |
| `QR-UI-A11Y-001` | `STRAT-UI-BLAZOR-HYBRID-002` | Componentes Blazor responsivos (`360px`–`4K`), navegación por teclado, contraste $\ge 4.5:1$ y regiones `aria-live="polite"`. |
| `QR-SEC-001` / `SEC-REQ-TENANT-001` | `STRAT-DATA-RLS-DECIMAL-003` | Revalidación en servidor `ITenantIsolationGuard` + rol `NOBYPASSRLS` + `SET LOCAL` + `FORCE ROW LEVEL SECURITY` *fail-closed* en PostgreSQL 16 (IDOR = `403`/`404` + alerta `SECURITY`). |
| `SEC-REQ-BILLING-001` | `STRAT-DATA-RLS-DECIMAL-003` | `SELECT ... FOR UPDATE` sobre `centers` + vinculación en servidor de `metadata.center_id` + `Stripe.EventUtility.ConstructEvent` sobre stream crudo ($\le 64\text{ KB}$) + idempotencia monótona en `stripe_webhook_events`. |
| `SEC-REQ-XLSX-001` / `SEC-REQ-DOS-001` | `STRAT-SEC-DEFENSE-IN-DEPTH-004` | `BoundedReadStream` ($\le 2\text{ MB}$ comp / $\le 10\text{ MB}$ reales / ratio $\le 20:1$), `DtdProcessing.Prohibit`, `TrimStart` + `'` + `XLDataType.Text`, guardas `System.Decimal`, `SemaphoreSlim(2, 2)`, `RecyclableMemoryStream`, `30 mut/min` y `<= 5` circuitos WSS/usuario. |
| `SEC-REQ-OBS-001` | `STRAT-SEC-DEFENSE-IN-DEPTH-004` | `[Authorize(Roles = "Maintainer")]` anti-Overposting, `ToMaximumDepth(3)`, redacción `[REDACTED]` en propiedades y `Exception.Message`, escape JSON de una línea y canal `SECURITY` anti-evicción. |

---

## 4. Historial de Revisiones y Control de Versiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-08 | `agent-system-architect` | Definición inicial de la estrategia de solución (.NET 10 LTS + Blazor + PostgreSQL RLS) | `chg-001-escandallo-mvp` |
| **1.1.0** | 2026-10-08 | `agent-system-architect` | Incorporación de estrategias de defensa técnica para `SEC-REQ-*` v1.1.0 | `chg-001-escandallo-mvp` |
