---
id: BR-TENANT-ISOLATION-001
type: business-rule
title: "Aislamiento Multi-Tenant Estricto por Cliente y Centro de Estética"
status: draft
version: "1.0.0"
schema-version: "1.0"
governs:
  - UC-COST-CATALOG-001
  - UC-ESCANDALLO-CALC-001
  - UC-EXEC-REPORT-001
  - UC-SUBSCRIPTION-001
supersedes: null
superseded-by: null
---

# Business Rule: Aislamiento Multi-Tenant Estricto por Cliente y Centro de Estética

## 1. Rule Definition

1. **Relación Usuario – Centros de Estética**: Un usuario (`ACT-ESTHETIC-USER-001`) puede dar de alta y administrar uno o múltiples centros de estética asociados a su cuenta.
2. **Aislamiento de Datos entre Clientes**: Ningún usuario puede leer, listar, modificar, recalcular ni exportar catálogos de costes, escandallos, históricos, suscripciones o informes pertenecientes a centros de estética de otros clientes.
3. **Independencia de Catálogos entre Centros**: Los costes (`Productos`, `Personal`, `Gastos Generales`, `IVA`, `Beneficio`) y los escandallos están vinculados a un centro de estética específico, de modo que la modificación de un coste en el Centro A no altera los escandallos del Centro B.

## 2. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del aislamiento multi-tenant por centro de estética. |
