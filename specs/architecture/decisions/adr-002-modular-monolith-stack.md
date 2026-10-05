---
id: ADR-002-MODULAR-MONOLITH-STACK
type: architecture-decision-record
title: "Arquitectura de Monolito Modular Full-Stack en TypeScript (Node.js LTS + Fastify/React + PostgreSQL) Alineada con license-policy.yaml"
status: proposed
version: "1.0.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
  - "Product Owner"
decision-date: "2026-10-05"
affects-components:
  - CMP-ESCANDALLO-SYS-001
  - CMP-WEB-UI-001
  - CMP-IAM-TENANT-001
  - CMP-COST-ENGINE-001
  - CMP-DOC-IO-001
  - CMP-BILLING-STRIPE-001
  - CMP-OBS-TELEMETRY-001
supersedes: null
superseded-by: null
---

# ADR-002: Arquitectura de Monolito Modular Full-Stack en TypeScript (Node.js LTS + Fastify/React + PostgreSQL) Alineada con license-policy.yaml

## 1. Context and Problem Statement

El sistema requiere una aplicación web accesible (WCAG 2.1 AA) y optimizada para PC, tablet y móvil (`QR-UI-A11Y-001`), un motor de cálculo financiero determinista (`FR-CALC-001`), generación de informes ejecutivos en PDF y Excel (`FR-REPORT-001`), integración con Stripe (`FR-BILLING-001`) y observabilidad avanzada (`FR-OBS-001`). Además, todo el código fuente debe cumplir las métricas estrictas de `quality-policy.yaml` (complejidad ciclomática $\le 10$, complejidad cognitiva $\le 15$, índice de mantenibilidad $\ge 50$, longitud de función $\le 40$ líneas) y el árbol de dependencias debe utilizar exclusivamente licencias permisivas aprobadas en `license-policy.yaml` (`MIT`, `Apache-2.0`, `BSD`, `ISC`).

## 2. Considered Technology Options

1. **Opción A (Elegida): Monolito Modular Hexagonal en TypeScript Estricto (`Node.js 22 LTS` + `Fastify` + `React/Vite` SPA/SSR + `Drizzle/Kysely` + `PostgreSQL 16`)**:
   - **Pros**: Un único lenguaje tipado de extremo a extremo (compartiendo esquemas de validación `Zod` entre cliente y servidor), huella de memoria mínima (`~120 MB RAM`), altísimo rendimiento HTTP con `Fastify` (`MIT`), y compatibilidad nativa con el motor de análisis estático AST de `@aisdlc/core` (`ts-morph`).
   - **Licencias**: 100% `MIT` / `Apache-2.0` / `BSD`.
2. **Opción B: Arquitectura de Microservicios Distribuida (API Gateway + Servicio Costes + Servicio Escandallos + Servicio Reportes + Broker Kafka/RabbitMQ)**:
   - **Contras**: Multiplica por $5\times$ el consumo de RAM y CPU, introduce latencia de serialización de red y transacciones distribuidas (Sagas) innecesarias para un dominio cuyas operaciones de recálculo son fuertemente consistentes (`BR-RECALC-HISTORY-001`). Viola el principio de simplicidad (YAGNI).
3. **Opción C: Stack Java/Spring Boot o Python/Django con Celery/Redis**:
   - **Contras**: Mayor huella de memoria base en contenedor (JVM requiere $\ge 1\text{ GB RAM}$ por servicio; Celery + Redis añade procesos adicionales), encareciendo el VPS mínimo necesario.

## 3. Decision Outcome

Se adopta la **Opción A (Monolito Modular Hexagonal en TypeScript Estricto sobre Node.js 22 LTS y PostgreSQL 16)**, estructurado internamente en módulos de dominio desacoplados (`iam-tenant`, `cost-engine`, `doc-io`, `billing-stripe`, `obs-telemetry`) con fronteras de importación estrictas.

### Catálogo de Librerías Aprobadas (`license-policy.yaml` Compliance)

| Capacidad | Paquete Seleccionado | Licencia SPDX | Estado en `license-policy.yaml` | Alternativa Descartada (Motivo) |
| :--- | :--- | :---: | :---: | :--- |
| Servidor HTTP / API | `fastify`, `@fastify/helmet`, `@fastify/rate-limit` | `MIT` | `ALLOW` | — |
| Interfaz Web Accesible | `react`, `tailwindcss`, `@radix-ui/primitives` | `MIT` | `ALLOW` | Componentes sin soporte WAI-ARIA nativo |
| Validación de Esquemas | `zod` | `MIT` | `ALLOW` | — |
| Aritmética Financiera | `decimal.js` | `MIT` | `ALLOW` | `Number` nativo JS (error coma flotante IEEE 754) |
| Acceso a Datos SQL | `pg` + `drizzle-orm` (o `kysely`) | `MIT` / `Apache-2.0` | `ALLOW` | ORMs pesados sin soporte limpio para RLS transaccional |
| Generación PDF | `pdfkit` | `MIT` | `ALLOW` | `iText` / `WeasyPrint` / `Puppeteer` (Licencia `AGPL-3.0` prohibida o consumo excesivo de RAM) |
| Importación/Exportación `.xlsx` | `exceljs` + `fflate` (inspección ZIP) | `MIT` | `ALLOW` | Versiones comerciales o con licencias ambiguas |
| Pasarela de Pagos | `stripe` (SDK oficial Node.js) | `MIT` | `ALLOW` | — |
| Observabilidad y Logs | `pino` + `@opentelemetry/sdk-node` | `MIT` / `Apache-2.0` | `ALLOW` | Agentes propietarios cerrados |

## 4. Consequences

- **Positive**:
  - Cumplimiento garantizado de `license-policy.yaml` (cero dependencias `GPL`/`AGPL`/`SSPL`/`BSL`).
  - Despliegue atómico en un único contenedor ligero de aplicación conectado a PostgreSQL, reduciendo el coste operativo y facilitando los tests de integración y BDD con Vitest/Cucumber.
- **Negative / Trade-offs**:
  - Al compartir proceso en Node.js, la generación de archivos `.pdf` y `.xlsx` debe ejecutarse mediante *streams* asíncronos no bloqueantes o *Worker Threads* acotados (`SEC-REQ-DOS-001`) para no bloquear el *Event Loop*.

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Formal decision proposal | ARCH-INIT-001 |
