---
id: UC-EXEC-REPORT-001
type: use-case
title: "Extracción de Informes Ejecutivos de Desglose de Costes y Beneficios"
status: draft
version: "1.0.0"
schema-version: "1.0"
primary-actor: ACT-ESTHETIC-USER-001
governed-by:
  - BR-CALC-FORMULAS-001
  - BR-TENANT-ISOLATION-001
  - BR-FREEMIUM-QUOTA-001
supersedes: null
superseded-by: null
---

# Use Case: Extracción de Informes Ejecutivos de Desglose de Costes y Beneficios

## 1. Intent and Outcome

As a `ACT-ESTHETIC-USER-001` I want to generate and export executive reports in PDF and Excel (`.xlsx`) formats detailing the breakdown of costs (`Gastos Generales`, `Coste Persona`, `Productos`), profit margins, and PVP across my beauty center's services To evaluate service profitability and support pricing decisions.

## 2. Preconditions

- El usuario está autenticado y ha seleccionado un centro de estética propio con al menos un servicio/escandallo registrado.
- El centro de estética respeta el límite de la versión gratuita ($\le 2$ escandallos activos) o mantiene una suscripción activa en Stripe (`BR-FREEMIUM-QUOTA-001`).

## 3. Main Flow

1. El usuario accede a la vista de Informes Ejecutivos del centro de estética seleccionado.
2. El sistema presenta el resumen ejecutivo de todos los servicios del centro (`Tabla57` + `Tabla5`), incluyendo:
   - Coste de Gastos Generales imputados, Coste de Personal y Coste de Productos por servicio.
   - `Escandallo Total`, `Beneficio Neto (€)`, `Precio con Beneficio (sin IVA)` y `PVP (con IVA)`.
   - Comparativa de rentabilidad y evolución histórica reciente.
3. El usuario solicita la exportación del informe ejecutivo seleccionando exclusivamente entre formato **PDF** (`.pdf`) o **Excel** (`.xlsx`).
4. El servidor genera y entrega el documento descargable en el formato seleccionado (`PDF` o `Excel`) con el desglose completo del centro.

## 4. Alternative and Exception Flows

- **E1 – Centro sin escandallos**: El sistema informa que no existen servicios calculados para exportar e invita a crear el primer escandallo.
- **E2 – Suscripción inactiva superando el límite gratuito ($> 2$ escandallos)**: El servidor bloquea la exportación de cualquier escandallo o informe ejecutivo hasta que el usuario reactive su suscripción o reduzca los escandallos activos para volver a respetar el límite gratuito de 2 escandallos.

## 5. Postconditions

- El usuario obtiene el informe ejecutivo en formato PDF o Excel (`.xlsx`) con los datos exclusivamente pertenecientes a su centro de estética.

## 6. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del caso de uso de informes ejecutivos. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Acotación de exportación exclusivamente a PDF y Excel (.xlsx) y bloqueo de exportación si se excede la cuota gratuita sin suscripción. |
