---
id: BR-FREEMIUM-QUOTA-001
type: business-rule
title: "Cuota Gratuita de 2 Escandallos y Suscripción Ilimitada por Centro en Stripe"
status: draft
version: "1.0.0"
schema-version: "1.0"
governs:
  - UC-ESCANDALLO-CALC-001
  - UC-EXEC-REPORT-001
  - UC-SUBSCRIPTION-001
supersedes: null
superseded-by: null
---

# Business Rule: Cuota Gratuita de 2 Escandallos y Suscripción Ilimitada por Centro en Stripe

## 1. Rule Definition

1. **Cuota Gratuita Inicial (Permite Recalcular)**: Cada usuario dispone de un máximo de **2 servicios con escandallo en estado `Activo` (`active`) de forma gratuita por centro de estética** sin necesidad de método de pago activo. Dentro de este límite de 2 escandallos activos, **se permite recalcularlos libremente** (tanto automáticamente al modificar costes como al ajustar parámetros o simular PVP sobre esos 2 servicios). Los duplicados de escandallo (Mejora M4) también contabilizan contra la cuota de escandallos activos.
2. **Bloqueo al Superar la Cuota Gratuita y Mecanismo de Archivado sin Pérdida de Datos (Mejora M1)**:
   - En un centro sin suscripción activa que ya tiene 2 escandallos activos, el sistema impide dar de alta o duplicar un 3er escandallo activo.
   - Si un centro tenía una suscripción activa con $> 2$ escandallos activos y la suscripción pasa a estado inactivo (`canceled`, `unpaid`, `past_due`), **se bloquea la creación, duplicación, edición y exportación de cualquier escandallo del centro** hasta que se reactive la suscripción o el usuario pase escandallos a estado **`Archivado` (`archived`)** hasta dejar $\le 2$ escandallos `Activos`.
   - Los escandallos en estado `Archivado` conservan intactos sus datos e histórico, no son editables ni exportables durante el plan gratuito, y se restauran a estado `Activo` en 1 clic al reactivar la suscripción en Stripe.
3. **Suscripción por Centro de Estética vía Stripe y Tarifas Oficiales**:
   - La contratación y pago de una suscripción en Stripe se asocia a un centro de estética específico y habilita **cálculos, duplicaciones, ediciones y exportaciones ilimitadas** para ese centro mientras la suscripción permanezca activa (`active` / `trialing`).
   - **Tarifas Oficiales en Stripe por Centro de Estética**:
     - **Plan Mensual**: **14,90 EUR / mes por centro de estética** (IVA no incluido).
     - **Plan Anual**: **149,00 EUR / año por centro de estética** (equivalente a 12,42 EUR/mes, 2 meses gratis, IVA no incluido).

## 2. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial de la regla de monetización freemium con Stripe. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Recálculo permitido en cuota gratuita, bloqueo de edición/exportación si se excede el límite sin suscripción y propuesta de pricing benchmark. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Confirmación de tarifas oficiales (14,90 EUR/mes y 149,00 EUR/año) e incorporación de Mejora M1 (archivado de escandallos sin pérdida de datos). |
