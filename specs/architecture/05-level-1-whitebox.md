---
id: ARCH-L1-WHITEBOX-001
type: building-blocks-level-1
title: "05. Level 1 Building Blocks: Overall Whitebox"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 5
naf-perspective: "Services & Systems (Level 1)"
bounded-contexts:
  - "Presentation & UX"
  - "Identity & Multi-Tenancy"
  - "Core Costing & Pricing Domain"
  - "Document Ingestion & Executive Reporting"
  - "Subscription & Monetization"
  - "Observability & Security Auditing"
contains-components:
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

# 05. Building Blocks View: Level 1 Whitebox (arc42 Sec. 5 / NAF Services & Systems)

## 1. Overall System Decomposition (Level 1 & Level 2)

Presenta la descomposición del sistema raíz `CMP-ESCANDALLO-SYS-001` en sus seis subsistemas modulares de Nivel 2, alineados con sus respectivos Bounded Contexts de Domain-Driven Design (DDD).

### 1.1 Level 1 Whitebox Diagram

```mermaid
graph TD
    subgraph BC_UX["Bounded Context: Presentation & UX"]
        CMP_UI["CMP-WEB-UI-001: Cliente Web Adaptativo WCAG 2.1 AA"]
    end

    subgraph BC_IAM["Bounded Context: Identity & Multi-Tenancy"]
        CMP_IAM["CMP-IAM-TENANT-001: Auth, RBAC, RLS & Rate Limiting"]
    end

    subgraph BC_CORE["Bounded Context: Core Costing & Pricing Domain"]
        CMP_ENGINE["CMP-COST-ENGINE-001: Catálogo, Escandallos, Semáforo M2 & Cascada"]
    end

    subgraph BC_DOC["Bounded Context: Document Ingestion & Reporting"]
        CMP_DOC["CMP-DOC-IO-001: Importador Seguro .xlsx & Exportador .pdf/.xlsx"]
    end

    subgraph BC_BILL["Bounded Context: Subscription & Monetization"]
        CMP_BILL["CMP-BILLING-STRIPE-001: Cuota Freemium Activos & Stripe"]
    end

    subgraph BC_OBS["Bounded Context: Observability & Security Auditing"]
        CMP_OBS["CMP-OBS-TELEMETRY-001: Logs Pino, OTLP, Nivel en Caliente & Redactado"]
    end

    CMP_UI -->|REST JSON| CMP_IAM
    CMP_IAM --> CMP_ENGINE
    CMP_IAM --> CMP_DOC
    CMP_IAM --> CMP_BILL
    CMP_IAM --> CMP_OBS
    CMP_ENGINE -->|Verifica Cuota FOR UPDATE| CMP_BILL
    CMP_DOC -->|Lee Catálogo y Escandallos| CMP_ENGINE
    CMP_ENGINE -.->|Trazas y Auditoría| CMP_OBS
    CMP_BILL -.->|Eventos Seguridad Pagos| CMP_OBS
```

---

## 2. Bounded Contexts and Components Catalog (`CMP-*`)

| Bounded Context | Component ID | Level | Enclave | Implemented Use Cases (`UC-*`) | Satisfied Requirements (`FR-*` / `QR-*` / `SEC-REQ-*`) |
| :--- | :--- | :---: | :--- | :--- | :--- |
| **Escandallo SaaS Platform** | [`CMP-ESCANDALLO-SYS-001`](./components/cmp-escandallo-sys-001.md) | 1 | `SEC-ENC-APP-CORE-001` | Todos (`UC-COST-CATALOG-001` a `UC-OBSERVABILITY-001`) | Todos (`FR-*`, `QR-*`, `SEC-REQ-*`) |
| **Presentation & UX** | [`CMP-WEB-UI-001`](./components/cmp-web-ui-001.md) | 2 | `SEC-ENC-EDGE-WAF-001` | Todos (`UC-COST-CATALOG-001` a `UC-OBSERVABILITY-001`) | `QR-UI-A11Y-001`, `FR-COST-001`, `FR-CALC-001`, `FR-REPORT-001`, `FR-OBS-001` |
| **Identity & Multi-Tenancy** | [`CMP-IAM-TENANT-001`](./components/cmp-iam-tenant-001.md) | 2 | `SEC-ENC-APP-CORE-001` | `UC-COST-CATALOG-001`, `UC-OBSERVABILITY-001` | `FR-TENANT-001`, `QR-SEC-001`, `SEC-REQ-TENANT-001`, `SEC-REQ-DOS-001`, `SEC-REQ-OBS-001` |
| **Core Costing & Pricing** | [`CMP-COST-ENGINE-001`](./components/cmp-cost-engine-001.md) | 2 | `SEC-ENC-APP-CORE-001` | `UC-COST-CATALOG-001`, `UC-ESCANDALLO-CALC-001` | `FR-COST-001`, `FR-CALC-001`, `FR-HIST-001`, `SEC-REQ-TENANT-001`, `SEC-REQ-DOS-001` |
| **Document Ingestion & Reporting** | [`CMP-DOC-IO-001`](./components/cmp-doc-io-001.md) | 2 | `SEC-ENC-APP-CORE-001` | `UC-COST-CATALOG-001`, `UC-EXEC-REPORT-001` | `FR-COST-001`, `FR-REPORT-001`, `SEC-REQ-XLSX-001`, `SEC-REQ-DOS-001`, `SEC-REQ-TENANT-001` |
| **Subscription & Monetization** | [`CMP-BILLING-STRIPE-001`](./components/cmp-billing-stripe-001.md) | 2 | `SEC-ENC-APP-CORE-001` | `UC-ESCANDALLO-CALC-001`, `UC-SUBSCRIPTION-001` | `FR-BILLING-001`, `QR-SEC-001`, `SEC-REQ-BILLING-001` |
| **Observability & Auditing** | [`CMP-OBS-TELEMETRY-001`](./components/cmp-obs-telemetry-001.md) | 2 | `SEC-ENC-DATA-OBS-001` | `UC-OBSERVABILITY-001` | `FR-OBS-001`, `QR-SEC-001`, `SEC-REQ-OBS-001` |

---

## 3. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Definición inicial de bloques de construcción Nivel 1 y Nivel 2 | ARCH-INIT-001 |
