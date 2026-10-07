---
id: CMP-WEB-UI-001
type: component
title: Cliente Web Adaptativo y Accesible (SPA/SSR Responsive UI para PC, Tablet y Móvil)
status: proposed
version: 1.0.0
schema-version: '1.0'
level: 2
bounded-context: Presentation & UX
parent-component: CMP-ESCANDALLO-SYS-001
implementation-type: service
implements-use-cases:
  - UC-COST-CATALOG-001
  - UC-ESCANDALLO-CALC-001
  - UC-EXEC-REPORT-001
  - UC-SUBSCRIPTION-001
  - UC-OBSERVABILITY-001
satisfies-requirements:
  - QR-UI-A11Y-001
  - FR-COST-001
  - FR-CALC-001
  - FR-REPORT-001
  - FR-OBS-001
  - FR-TENANT-001
  - FR-HIST-001
  - FR-BILLING-001
  - QR-SEC-001
  - SEC-REQ-TENANT-001
  - SEC-REQ-BILLING-001
  - SEC-REQ-XLSX-001
  - SEC-REQ-DOS-001
  - SEC-REQ-OBS-001
hosted-in-enclave: SEC-ENC-EDGE-WAF-001
interfaces:
  - name: Responsive Web Application UI (HTML5 / WAI-ARIA)
    protocol: HTTPS
    contract-spec: specs/product/requirements/qr-ui-a11Y-001.md
supersedes: null
superseded-by: null
---


# CMP-WEB-UI-001: Cliente Web Adaptativo y Accesible (SPA/SSR Responsive UI para PC, Tablet y Móvil)

## 1. Purpose, Responsibility, and Bounded Context

Subsistema de presentación de Nivel 2 construido en **.NET Puro con Blazor Web App (SSR + Interactive Server/WebAssembly)** y hojas de estilo accesibles (`MIT`), responsable de ofrecer una experiencia ergonómica y sin scroll horizontal en móvil (`320px–767px`), tablet en cabina (`768px–1023px`, vertical y horizontal) y escritorio (`>=1024px`), cumpliendo el estándar **WCAG 2.1 Nivel AA** (`QR-UI-A11Y-001`).

Implementa en interfaz las mejoras clave de MVP:
- **M1**: Filtros de estado `active` / `archived` y asistente de resolución de bloqueo post-cancelación de suscripción.
- **M2**: Simulador en vivo de `PVP Comercial Fijado` frente a `PVP Calculado` y **Semáforo de Erosión de Margen (`🟢/🟡/🔴`)**.
- **M3**: Constructor dinámico de recetas $1:N$ y modal de confirmación con **previsualización de impacto** (*"Este cambio recalculará N escandallos"*).
- **M4**: Onboarding en 1 clic con catálogo semilla, carga de plantilla `.xlsx`, botón *"Duplicar Escandallo"* y selector de copia de catálogo entre centros propios.
- **M5**: Asistente visual configurable de equivalencias de dosis de cabina (`pulsaciones`, `gotas`, `cucharaditas`, `ampollas/viales monodosis` $\rightarrow$ `ml`/`g`, editables desde la aplicación) y control de `% Merma`.

## 2. Structure and Connectivity Diagram (arc42 Sec. 5 / NAF v4)

```mermaid
graph TD
    Browser["Navegador Usuario (PC / Tablet / Móvil)"] -->|Interacción Táctil / Teclado| UI["CMP-WEB-UI-001 (Blazor Web App WAI-ARIA)"]
    UI -->|HTTPS JSON + Cookie HttpOnly| IAM["CMP-IAM-TENANT-001 (ASP.NET Core API)"]
    UI -->|Redirección Checkout / Portal| StripeCheckout["Stripe Hosted Checkout"]
```

## 3. Interface Contracts and Execution Policies

- **Execution Mechanism**: Componentes Razor/Blazor servidos por `ASP.NET Core` detrás de Cloudflare CDN (`SEC-ENC-EDGE-WAF-001`) y comunicación con los endpoints de dominio.
- **Fault Tolerance and Performance**: Áreas táctiles mínimas de `44x44 px`, contraste $\ge 4.5:1$, navegación completa por teclado (`Tab`, `Enter`, `Escape`), y regiones `aria-live="polite"` para anunciar recálculos de PVP y cambios del semáforo de margen a lectores de pantalla.

---

## 4. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Definición inicial del cliente web accesible y adaptativo | ARCH-INIT-001 |


## Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-07 | agent-devops | Integración de incremento de entrega chg-001-escandallo-mvp | chg-001-escandallo-mvp |
