---
id: ABUSE-IDOR-TENANT-001
type: abuse-case
title: "Exfiltración Cross-Tenant de Costes, Catálogos e Informes mediante IDOR/BOLA"
status: draft
version: "1.1.0"
schema-version: "1.0"
primary-threat-actor: ACT-THREAT-TENANT-001
targets-use-cases:
  - UC-COST-CATALOG-001
  - UC-EXEC-REPORT-001
stride-category: "Information Disclosure"
severity: "CRITICAL"
likelihood: "HIGH"
mitigated-by:
  - SEC-REQ-TENANT-001
supersedes: null
superseded-by: null
---

# Abuse Case: Exfiltración Cross-Tenant de Costes, Catálogos e Informes mediante IDOR/BOLA

## 1. Attack Scenario & Preconditions

Un inquilino malicioso (`ACT-THREAT-TENANT-001`) autenticado en su propio centro de estética intenta acceder a información financiera confidencial de un centro competidor (nóminas netas de empleados en `Tabla1`, costes de proveedor de cosmética en `Tabla3`, alquileres en `Tabla13` e informes ejecutivos PDF/Excel) explotando los mecanismos de enlace de datos de **Blazor Web App** (`SEC-ENC-APP-CORE-001`) o el pool de conexiones **`Npgsql` / PostgreSQL 16** (`SEC-ENC-DATA-OBS-001`).

## 2. Attack Flow (Step-by-Step)

1. **Vector A (IDOR en Endpoints HTTP y Copia Dual `M4`)**: El atacante obtiene una sesión válida para su propio centro (`center_id = C_ATTACKER`) y durante la consulta de costes, exportación de informes (`UC-EXEC-REPORT-001`) o copia rápida de catálogo entre centros (`M4` en `UC-COST-CATALOG-001`), sustituye `center_id` o `source_center_id` por el UUID de un centro víctima (`C_VICTIM`).
2. **Vector B (Tampering de `[Parameter]` / `[SupplyParameterFromForm]` en Circuitos Blazor `CMP-WEB-UI-001`)**: En una vista interactiva (`@rendermode InteractiveServer`) o formulario SSR, el atacante manipula los mensajes del circuito SignalR (`/_blazor`) o campos ocultos del DOM para cambiar el `CenterId` activo después de haberse superado la carga inicial de la página (`OnInitializedAsync`).
3. **Vector C (Contaminación de Contexto RLS en Pool `Npgsql` — `RSK-RLS-POOL-LEAK-004`)**: Si una operación en `CMP-IAM-TENANT-001` fija `SET SESSION app.current_center_id` en lugar de `SET LOCAL` dentro de una transacción explícita, o si el usuario de PostgreSQL posee privilegios `BYPASSRLS` / propietario de tabla sin `FORCE ROW LEVEL SECURITY`, una conexión devuelta al pool de `Npgsql` filtra filas del tenant anterior hacia la consulta del atacante.

## 3. Business & Security Impact

- **Impacto Crítico (Espionaje Industrial y Violación RGPD)**: Exposición de salarios de personal, costes negociados con proveedores y márgenes netos de centros de estética terceros.
- Ruptura total de la regla de negocio `BR-TENANT-ISOLATION-001`.

## 4. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del caso de abuso IDOR/BOLA cross-tenant. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Incorporación de vectores técnicos sobre manipulación de parámetros en circuitos Blazor y fuga de sesión RLS en el pool Npgsql. |
