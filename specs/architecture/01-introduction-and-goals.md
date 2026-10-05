---
id: ARCH-INTRO-001
type: architecture-introduction
title: "01. Introduction and System Goals"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 1
naf-perspective: "Enterprise & Capability"
cites-product-actors:
  - ACT-ESTHETIC-USER-001
  - ACT-MAINTAINER-001
  - ACT-STRIPE-001
cites-quality-goals:
  - QR-UI-A11Y-001
  - QR-SEC-001
cites-use-cases:
  - UC-COST-CATALOG-001
  - UC-ESCANDALLO-CALC-001
  - UC-EXEC-REPORT-001
  - UC-SUBSCRIPTION-001
  - UC-OBSERVABILITY-001
supersedes: null
superseded-by: null
---

# 01. Introduction and System Goals (arc42 Sec. 1 / NAF Enterprise)

## 1. System Vision and Executive Summary

### 1.1 Mission Statement
> *La plataforma SaaS **Escandallo** proporciona a los centros de estética una solución web multi-tenant, accesible (WCAG 2.1 AA) y adaptativa (PC, tablet y móvil) para calcular con precisión decimal el escandallo de cada servicio, simular su PVP comercial, prevenir la erosión de márgenes ante subidas de costes de proveedores (`🟢/🟡/🔴`) y exportar informes ejecutivos en PDF y Excel, operando sobre una arquitectura segura y ultra-rentable.*

---

## 2. Priority Quality Goals (Canonical ProductShape Citations)

| Priority | Quality Goal | Requirement ID | Architectural Rationale |
| :---: | :--- | :--- | :--- |
| **1** | **Aislamiento Multi-Tenant Zero-Trust y Seguridad Perimetral** | `QR-SEC-001`, `SEC-REQ-TENANT-001` | PostgreSQL Row-Level Security (`FORCE RLS`), validación dual de titularidad en copia de catálogos (`M4`) y escudo perimetral Cloudflare WAF (`SEC-ENC-EDGE-WAF-001`). |
| **2** | **Accesibilidad Universal y Ergonomía Multi-Dispositivo** | `QR-UI-A11Y-001` | Interfaz web con conformidad WCAG 2.1 AA, objetivos táctiles $\ge 44\times 44\text{ px}$ y vistas adaptativas para móvil, tablet en cabina (vertical/horizontal) y escritorio. |
| **3** | **Exactitud Financiera e Integridad del Histórico en Cascada** | `FR-CALC-001`, `FR-HIST-001` | Motor de cálculo puro en `decimal.js` + `NUMERIC(14,4)` y transacciones ACID síncronas (`< 15 ms`) que guardan snapshots inmutables solo cuando un coste está en uso activo. |
| **4** | **Rentabilidad Operativa Extrema y Protección Anti-DoS** | `FR-BILLING-001`, `SEC-REQ-DOS-001` | Monolito modular en Node.js LTS + PostgreSQL desplegado en VPS europeo con TCO `< 10 EUR/mes` (rentable desde el primer suscriptor de **14,90 EUR/mes**) y estrangulamiento de operaciones asimétricas. |

---

## 3. Stakeholder and Primary Actors Matrix

| Role / Stakeholder | Actor ID | Architectural Expectations |
| :--- | :--- | :--- |
| **Gestor/a de Centro de Estética** | `ACT-ESTHETIC-USER-001` | Onboarding rápido (`M4`), edición ágil en tablet/móvil, semáforo de margen en tiempo real (`M2`) e informes PDF/Excel instantáneos. |
| **Usuario Mantenedor (Operador)** | `ACT-MAINTAINER-001` | Extracción dual de logs, trazas W3C y métricas (`JSON`/`CSV`/`OTLP`) y cambio en caliente del nivel de severidad (`DEBUG` a `SECURITY`) sin exponer secretos (`SEC-REQ-OBS-001`). |
| **Pasarela de Pagos (Stripe)** | `ACT-STRIPE-001` | Recepción fiable e idempotente de webhooks firmados con HMAC-SHA256 (`SEC-REQ-BILLING-001`). |

---

## 4. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Definición inicial de objetivos arquitectónicos | ARCH-INIT-001 |
