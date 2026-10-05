---
id: CMP-ESCANDALLO-SYS-001
type: component
title: "Plataforma SaaS Multi-Tenant de Escandallos y Rentabilidad para Centros de Estética (Sistema Raíz)"
status: proposed
version: "1.0.0"
schema-version: "1.0"
level: 1
bounded-context: "Escandallo SaaS Platform"
parent-component: null
implementation-type: composite
implements-use-cases:
  - UC-COST-CATALOG-001
  - UC-ESCANDALLO-CALC-001
  - UC-EXEC-REPORT-001
  - UC-SUBSCRIPTION-001
  - UC-OBSERVABILITY-001
satisfies-requirements:
  - FR-TENANT-001
  - FR-COST-001
  - FR-CALC-001
  - FR-HIST-001
  - FR-REPORT-001
  - FR-BILLING-001
  - FR-OBS-001
  - QR-UI-A11Y-001
  - QR-SEC-001
  - SEC-REQ-TENANT-001
  - SEC-REQ-BILLING-001
  - SEC-REQ-XLSX-001
  - SEC-REQ-DOS-001
  - SEC-REQ-OBS-001
hosted-in-enclave: SEC-ENC-APP-CORE-001
interfaces:
  - name: "Public Web & REST API Gateway"
    protocol: "HTTPS / REST / JSON"
    contract-spec: "specs/architecture/08-cross-cutting-concepts.md"
  - name: "Stripe Webhook Ingestion Endpoint"
    protocol: "HTTPS / HMAC-SHA256"
    contract-spec: "specs/architecture/08-cross-cutting-concepts.md"
supersedes: null
superseded-by: null
---

# CMP-ESCANDALLO-SYS-001: Plataforma SaaS Multi-Tenant de Escandallos y Rentabilidad para Centros de Estética (Sistema Raíz)

## 1. Purpose, Responsibility, and Bounded Context

Componente compuesto de Nivel 1 (Sistema Raíz) que encapsula el monolito modular de la plataforma SaaS de cálculo de escandallos, beneficios y PVP para centros de estética (`ADR-002-MODULAR-MONOLITH-STACK`), desplegado sobre la topología rentable Cloudflare Edge + VPS Cloud Europeo (`ADR-001-COST-EFFECTIVE-DEPLOYMENT`). Coordina los seis subsistemas de Nivel 2 garantizando aislamiento multi-tenant estricto (`SEC-REQ-TENANT-001`), exactitud aritmética (`ADR-004-EXACT-DECIMAL-AND-CASCADE-ENGINE`) y observabilidad dual (`ADR-005-DUAL-OBSERVABILITY-AND-SAFE-DOCUMENTS`).

## 2. Structure and Connectivity Diagram (arc42 Sec. 5 / NAF v4)

```mermaid
graph TD
    User["ACT-ESTHETIC-USER-001 (PC / Tablet / Móvil)"] -->|HTTPS TLS 1.3| WAF["SEC-ENC-EDGE-WAF-001 (Cloudflare WAF)"]
    Maintainer["ACT-MAINTAINER-001 (Operador)"] -->|HTTPS TLS 1.3| WAF
    Stripe["ACT-STRIPE-001 (Stripe Webhooks)"] -->|HTTPS + Stripe-Signature| WAF

    subgraph SYS["CMP-ESCANDALLO-SYS-001 (Monolito Modular TypeScript)"]
        UI["CMP-WEB-UI-001 (Cliente Web WCAG 2.1 AA)"]
        IAM["CMP-IAM-TENANT-001 (Auth, RBAC, RLS & Rate Limit)"]
        ENGINE["CMP-COST-ENGINE-001 (Motor Costes, Escandallos & Cascada)"]
        DOC["CMP-DOC-IO-001 (Importador .xlsx & Informes PDF/Excel)"]
        BILL["CMP-BILLING-STRIPE-001 (Cuota Freemium & Stripe)"]
        OBS["CMP-OBS-TELEMETRY-001 (Logs Pino, OTLP & Redactado)"]
    end

    WAF --> UI
    UI -->|REST JSON| IAM
    IAM --> ENGINE
    IAM --> DOC
    IAM --> BILL
    IAM --> OBS
    ENGINE --> DB[("PostgreSQL 16 (RLS Enabled)")]
    BILL --> DB
    OBS --> DB
```

## 3. Interface Contracts and Execution Policies

- **Execution Mechanism**: Proceso contenedorizado Node.js 22 LTS gestionado por Docker Compose detrás de Caddy Reverse Proxy y Cloudflare Edge WAF.
- **Fault Tolerance and Performance**: Huella de memoria máxima acotada a `512 MB RAM` por contenedor de aplicación; latencia $p95 < 100\text{ ms}$ en operaciones de cálculo y recálculo en cascada.

---

## 4. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Definición inicial del componente raíz del sistema | ARCH-INIT-001 |
