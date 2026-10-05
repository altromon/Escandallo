---
id: UC-ESCANDALLO-CALC-001
type: use-case
title: "Cálculo de Escandallo, Beneficios, PVP e Histórico por Servicio Estético"
status: draft
version: "1.0.0"
schema-version: "1.0"
primary-actor: ACT-ESTHETIC-USER-001
governed-by:
  - BR-CALC-FORMULAS-001
  - BR-RECALC-HISTORY-001
  - BR-TENANT-ISOLATION-001
  - BR-FREEMIUM-QUOTA-001
supersedes: null
superseded-by: null
---

# Use Case: Cálculo de Escandallo, Beneficios, PVP e Histórico por Servicio Estético

## 1. Intent and Outcome

As a `ACT-ESTHETIC-USER-001` I want to define beauty services with their duration, assigned personnel category, and consumed products, and view their calculated total cost, profit, PVP, and historical evolution To make informed retail pricing decisions for my beauty center.

## 2. Preconditions

- El usuario está autenticado y ha seleccionado un centro de estética propio.
- El centro dispone de cuota gratuita disponible ($< 2$ cálculos de escandallo) o cuenta con una suscripción activa en Stripe (`BR-FREEMIUM-QUOTA-001`).

## 3. Main Flow

1. El usuario crea, selecciona o **duplica** (Mejora M4) un servicio/tratamiento dentro de su centro de estética.
2. En el **Constructor Dinámico de Receta** (Mejora M3), el usuario introduce el `Nombre` del servicio, la duración en `Minutos`, la categoría de `Persona` que lo realiza y añade dinámicamente las líneas de productos consumidos (`Producto i`, `Cantidad i` con asistente de conversión de gotas/pulsaciones y `% Merma i`, Mejora M5), además de poder fijar su **`PVP Comercial Fijado`** de carta (Mejora M2).
3. El sistema calcula en servidor:
   - $\text{Gastos Generales Imputados} = \text{Minutos} \times \text{Tasa Total Gastos Generales (€/min)}$
   - $\text{Coste Persona} = \text{Minutos} \times \text{Precio Unitario Personal (€/min)}$
   - $\sum \text{Coste Producto}_i = \sum (\text{Cantidad}_i \times \text{Precio Unitario Producto}_i \times (1 + \text{Merma}_i))$
   - $\text{Escandallo Total} = \text{Gastos Generales Imputados} + \text{Coste Persona} + \sum \text{Coste Producto}_i$
   - $\text{Precio con Beneficio (Base Imponible)} = \text{Escandallo Total} \times (1 + \text{Beneficio})$
   - $\text{Beneficio Neto Unitario (€)} = \text{Escandallo Total} \times \text{Beneficio}$
   - $\text{PVP Calculado} = \text{Precio con Beneficio} \times (1 + \text{IVA})$
   - **Simulación Inversa y Evaluación de `PVP Comercial Fijado` (Mejora M2)**: Si el usuario introduce o mantiene un `PVP Comercial Fijado`, el sistema calcula $\text{Precio sin IVA Real} = \text{PVP Comercial Fijado} / (1 + \text{IVA})$, $\text{Beneficio Neto Real (€)} = \text{Precio sin IVA Real} - \text{Escandallo Total}$, el `% Beneficio Real` y el estado del **Semáforo de Erosión de Margen (`🟢/🟡/🔴`)**.
4. El sistema almacena la instantánea en el histórico de escandallos del servicio y muestra el desglose porcentual y monetario de costes, `Beneficio Neto (€)`, `Precio sin IVA`, `PVP Calculado`, `PVP Comercial Fijado` y el semáforo de rentabilidad.

## 4. Alternative and Exception Flows

- **E1 – Cuota gratuita superada**: Si el centro ya tiene 2 escandallos activos en plan gratuito y no tiene suscripción activa en Stripe, el sistema bloquea el alta o duplicación de un 3er escandallo activo (permitiendo recalcular los 2 existentes). Si el centro tenía $> 2$ escandallos activos tras cancelar su suscripción, el sistema bloquea la edición o exportación de cualquier escandallo hasta archivar los excedentes ($\le 2$ activos, Mejora M1) o reactivar la suscripción (`UC-SUBSCRIPTION-001`).
- **A1 – Recálculo automático por cambio de coste**: Cuando cambia un coste en `UC-COST-CATALOG-001` que está siendo usado por el escandallo, el sistema recalcula automáticamente el escandallo, actualiza el semáforo de erosión de margen respecto al `PVP Comercial Fijado` y añade una entrada en el histórico del servicio.

## 5. Postconditions

- El escandallo, su desglose, el beneficio neto en euros, el precio sin IVA, el PVP calculado, el PVP comercial fijado y el semáforo de margen quedan persistidos y versionados en el histórico del centro de estética.

## 6. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del caso de uso de cálculo de escandallo y PVP. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Simulación inversa desde PVP, recálculo permitido dentro de los 2 gratuitos y bloqueo de edición si se excede el límite tras cancelación. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de mejoras M1 a M5 (archivado, PVP comercial + semáforo, constructor dinámico, duplicación y merma/dosis). |
