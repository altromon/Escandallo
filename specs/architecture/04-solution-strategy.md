---
id: ARCH-STRAT-001
type: solution-strategy
title: "04. Architecture Solution Strategy"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 4
naf-perspective: "Service & Resource Strategy"
strategies:
  - STRAT-DEPLOY-001
  - STRAT-ARCH-002
  - STRAT-TENANT-003
  - STRAT-CALC-004
  - STRAT-OBS-005
supersedes: null
superseded-by: null
---

# 04. Solution Strategy (arc42 Sec. 4 / NAF Strategy)

## 1. Fundamental Structural Decisions (`STRAT-*`)

| Strategy ID | Fundamental Pattern / Decision | Reference ADR | Technical Rationale & Trade-offs |
| :--- | :--- | :---: | :--- |
| `STRAT-DEPLOY-001` | **Despliegue Cloud Europeo Ultra-Rentable con Escudo Edge WAF** | `ADR-001` | Cloudflare WAF/CDN en el borde + 1 VPS Europeo (Hetzner CX22/CX32 con Docker Compose) reduce el coste fijo a **~5,50–9,50 EUR/mes**, logrando punto de equilibrio con **1 sola suscripción** de 14,90 EUR/mes. |
| `STRAT-ARCH-002` | **Monolito Modular Hexagonal en TypeScript Estricto** | `ADR-002` | Separa el dominio puro de los adaptadores HTTP/SQL sin incurrir en la sobrecarga operativa ni de memoria de microservicios distribuidos. |
| `STRAT-TENANT-003` | **Defensa en Profundidad Multi-Tenant con PostgreSQL RLS y Bloqueo Pesimista** | `ADR-003` | Garantiza aislamiento por `center_id` a nivel de motor de base de datos (`FORCE RLS`) y serializa el contador de escandallos activos con `SELECT ... FOR UPDATE`. |
| `STRAT-CALC-004` | **Motor Decimal Puro y Recálculo en Cascada Síncrono Transaccional** | `ADR-004` | Utiliza `decimal.js` + `NUMERIC(14,4)` y ejecuta el recálculo de los $N$ escandallos afectados y su histórico en una única transacción ACID (`< 15 ms`). |
| `STRAT-OBS-005` | **Telemetría Dual Ligera con Redactado Inmutable y Documentos en Streaming** | `ADR-005` | Emplea `pino` + `OpenTelemetry` con redactado en origen e informes generados en streaming con `pdfkit` y `exceljs` (`MIT`). |

---

## 2. Quality Goals Fulfillment Matrix

| Quality Goal | Requirement ID | Adopted Decision / Strategy | Guarantee Mechanism |
| :--- | :--- | :--- | :--- |
| **Accesibilidad y Diseño Adaptativo** | `QR-UI-A11Y-001` | `CMP-WEB-UI-001` con Radix UI + TailwindCSS | Vistas específicas para móvil, tablet (vertical/horizontal) y PC, objetivos táctiles $\ge 44\times 44\text{ px}$ y contraste $\ge 4.5:1$. |
| **Protección Anti-IDOR Multi-Tenant** | `SEC-REQ-TENANT-001`, `FR-TENANT-001` | `STRAT-TENANT-003` (`ADR-003`) | Middleware de contexto de centro + políticas `FORCE ROW LEVEL SECURITY` en PostgreSQL 16 + validación dual en copia `M4`. |
| **Integridad de Cuota Freemium y Pagos** | `SEC-REQ-BILLING-001`, `FR-BILLING-001` | `STRAT-TENANT-003` (`ADR-003`) | Bloqueo exclusivo de fila del centro al activar/crear escandallos y verificación HMAC-SHA256 con ventana de 300s e idempotencia. |
| **Resiliencia frente a Archivos Maliciosos y DoS** | `SEC-REQ-XLSX-001`, `SEC-REQ-DOS-001` | `STRAT-OBS-005` (`ADR-005`) | Inspección previa de cabeceras ZIP (`<= 10 MB` descomprimido, sin XXE), sanitización DDE y semáforo de máx. 2 exportaciones simultáneas por centro. |
| **Seguridad del Rol Mantenedor y Logs** | `SEC-REQ-OBS-001`, `FR-OBS-001` | `STRAT-OBS-005` (`ADR-005`) | RBAC estricto, filtro de ofuscación `[REDACTED]` inmutable en nivel `DEBUG` y serialización JSON monolineal anti-CRLF. |

---

## 3. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Definición inicial de estrategia de solución | ARCH-INIT-001 |
