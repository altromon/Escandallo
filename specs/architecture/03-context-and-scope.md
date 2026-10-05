---
id: ARCH-CTX-001
type: context-and-scope
title: "03. System Context and Scope"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 3
naf-perspective: "Operational Perspective"
operational-exchanges:
  - OIE-USER-WEB-001
  - OIE-REPORT-DOC-002
  - OIE-STRIPE-BILL-003
  - OIE-MAINT-OBS-004
context-boundaries:
  - CTX-PERIMETER-EDGE-001
  - CTX-APP-CORE-002
supersedes: null
superseded-by: null
---

# 03. Context and Scope (arc42 Sec. 3 / NAF Operational)

## 1. Business Context

Modela las fronteras conceptuales e intercambios de información entre la plataforma **Escandallo** y sus actores externos.

### 1.1 Business Context Diagram

```mermaid
graph LR
    EstheticUser["ACT-ESTHETIC-USER-001 (Gestor/a Centro Estética)"] -->|"Alta Costes / Plantilla .xlsx / Escandallos"| System(["CMP-ESCANDALLO-SYS-001 (Plataforma Escandallo)"])
    System -->|"Semáforo Margen 🟢🟡🔴 + Informes .pdf / .xlsx"| EstheticUser
    System <-->|"Checkout 14,90€/mes o 149€/año + Webhooks HMAC"| Stripe["ACT-STRIPE-001 (Pasarela Stripe)"]
    Maintainer["ACT-MAINTAINER-001 (Usuario Mantenedor)"] -->|"Ajuste Nivel Log en Caliente (DEBUG..SECURITY)"| System
    System -->|"Exportación Logs/Trazas/Métricas (JSON/CSV/OTLP)"| Maintainer
```

### 1.2 Operational Information Exchanges (`OIE-*`)

| Exchange ID | Source / Target | Payload / Message | Protocol / Channel | Format |
| :--- | :--- | :--- | :--- | :--- |
| `OIE-USER-WEB-001` | `ACT-ESTHETIC-USER-001` $\leftrightarrow$ `CMP-ESCANDALLO-SYS-001` | Gestión de centros, catálogo de costes, recetas dinámicas `M3`, simulación de PVP `M2` e histórico | HTTPS (TLS 1.3) / REST | JSON (Validado con Zod) |
| `OIE-REPORT-DOC-002` | `ACT-ESTHETIC-USER-001` $\leftrightarrow$ `CMP-DOC-IO-001` | Subida de plantilla de catálogo (`.xlsx`) y descarga de informes ejecutivos (`.pdf`, `.xlsx`) | HTTPS Streaming | OOXML (`.xlsx`) / PDF 1.7 (`.pdf`) |
| `OIE-STRIPE-BILL-003` | `CMP-BILLING-STRIPE-001` $\leftrightarrow$ `ACT-STRIPE-001` | Creación de sesiones Checkout/Portal y recepción asíncrona de eventos de pago | HTTPS / Webhook HMAC-SHA256 | JSON (`Stripe-Signature`) |
| `OIE-MAINT-OBS-004` | `ACT-MAINTAINER-001` $\leftrightarrow$ `CMP-OBS-TELEMETRY-001` | Cambio en caliente de nivel de log y exportación de telemetría redactada | HTTPS / OTLP | JSON / CSV / OTLP |

---

## 2. Technical Context and Perimeter Infrastructure

### 2.1 Technical Context Diagram

```mermaid
graph TD
    Client["Navegador Web (PC / Tablet / Móvil)"] -->|"TLS 1.3 (Puerto 443)"| CF["SEC-ENC-EDGE-WAF-001: Cloudflare Anycast WAF & CDN"]
    StripeHook["Stripe Webhook Dispatcher"] -->|"TLS 1.3 + Cabecera Stripe-Signature"| CF
    CF -->|"mTLS Authenticated Origin Pulls (IPs Filtradas)"| Caddy["Caddy Reverse Proxy (Contenedor DMZ)"]
    Caddy -->|"HTTP Interno"| App["SEC-ENC-APP-CORE-001: Monolito Modular Node.js 22 LTS"]
    App -->|"Socket / Red Interna Aislada + RLS"| PG[("SEC-ENC-DATA-OBS-001: PostgreSQL 16 NVMe")]
    PG -.->|"Backup Diario Cifrado (restic + AES-256)"| R2[("Cloudflare R2 Object Storage UE")]
```

---

## 3. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Definición inicial de contexto de negocio y técnico | ARCH-INIT-001 |
