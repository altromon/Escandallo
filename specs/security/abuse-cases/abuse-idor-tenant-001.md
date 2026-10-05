---
id: ABUSE-IDOR-TENANT-001
type: abuse-case
title: "Exfiltración Cross-Tenant de Costes, Catálogos e Informes mediante IDOR/BOLA"
status: draft
version: "1.0.0"
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

Un inquilino malicioso (`ACT-THREAT-TENANT-001`) autenticado en su propio centro de estética intenta acceder a información financiera confidencial de un centro competidor (nóminas netas de empleados en `Tabla1`, costes de proveedor de cosmética en `Tabla3`, alquileres en `Tabla13` e informes ejecutivos PDF/Excel).

## 2. Attack Flow (Step-by-Step)

1. El atacante obtiene un token de sesión válido para su propio centro (`center_id = C_ATTACKER`).
2. Durante una operación de consulta de costes, exportación de informe ejecutivo (`UC-EXEC-REPORT-001`) o copia rápida de catálogo de productos entre centros (`M4` en `UC-COST-CATALOG-001`), el atacante intercepta la petición HTTP y sustituye `center_id` o `source_center_id` por el identificador de un centro ajeno (`C_VICTIM`).
3. Si el backend confía en el `center_id` enviado en el payload o query string sin cruzarlo contra la tabla de pertenencia del usuario autenticado ni aplicar aislamiento en base de datos, el sistema devuelve o clona el catálogo de productos y márgenes del centro víctima.

## 3. Business & Security Impact

- **Impacto Crítico (Espionaje Industrial y Violación RGPD)**: Exposición de salarios de personal, costes negociados con proveedores y márgenes netos de centros de estética terceros.
- Ruptura total de la regla de negocio `BR-TENANT-ISOLATION-001`.

## 4. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del caso de abuso IDOR/BOLA cross-tenant. |
