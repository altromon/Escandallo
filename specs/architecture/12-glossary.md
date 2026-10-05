---
id: ARCH-GLOSS-001
type: architecture-glossary
title: "12. Architecture Glossary and Terms Taxonomy"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 12
naf-perspective: "Taxonomy & Terms"
cites-domain-terms:
  - TERM-ESCANDALLO-001
cites-bounded-contexts:
  - "Presentation & UX"
  - "Identity & Multi-Tenancy"
  - "Core Costing & Pricing Domain"
  - "Document Ingestion & Executive Reporting"
  - "Subscription & Monetization"
  - "Observability & Security Auditing"
supersedes: null
superseded-by: null
---

# 12. Architecture Glossary (arc42 Sec. 12 / NAF Taxonomy)

## 1. Canonical Domain Terms (`TERM-*`)

| Term / ID | Canonical Definition | Reference Bounded Context |
| :--- | :--- | :--- |
| [`TERM-ESCANDALLO-001`](../product/terms/term-escandallo-001.md) | Glosario canónico del dominio de escandallos para centros de estética: costes directos (`Tabla1`, `Tabla2`, `Tabla3`, `Tabla25`), gastos generales (`Tabla13`), parámetros (`Tabla12`), `PVP Calculado` vs `PVP Comercial Fijado`, Semáforo de Erosión de Margen (`🟢/🟡/🔴`, M2), ciclo de vida `active`/`archived` (M1), recetas dinámicas $1:N$ (M3), catálogo semilla (M4) y merma operativa (M5). | Core Costing & Pricing Domain |

---

## 2. Technical Abbreviations and Acronyms

| Acronym | Full Meaning | Definition in System Context |
| :--- | :--- | :--- |
| **RLS** | Row-Level Security | Mecanismo nativo de PostgreSQL 16 que restringe a nivel de motor qué filas puede leer o modificar una transacción según `app.current_center_id`. |
| **TOCTOU** | Time-Of-Check to Time-Of-Use | Condición de carrera mitigada mediante `SELECT ... FOR UPDATE` al verificar el límite gratuito de 2 escandallos activos (`SEC-REQ-BILLING-001`). |
| **DDE** | Dynamic Data Exchange | Vector de inyección de fórmulas ejecutables en hojas Excel (`=`, `+`, `-`, `@`) neutralizado por `CMP-DOC-IO-001` (`SEC-REQ-XLSX-001`). |
| **OTLP** | OpenTelemetry Protocol | Protocolo estándar abierto para la transmisión y exportación de logs, trazas distribuidas W3C y métricas (`CMP-OBS-TELEMETRY-001`). |
| **TCO** | Total Cost of Ownership | Coste total mensual de infraestructura (~5,50 a 9,50 EUR/mes en la Opción A recomendada en `ADR-001-COST-EFFECTIVE-DEPLOYMENT`). |

---

## 3. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Definición inicial del glosario de arquitectura | ARCH-INIT-001 |
