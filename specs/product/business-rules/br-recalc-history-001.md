---
id: BR-RECALC-HISTORY-001
type: business-rule
title: "Inmutabilidad del Histórico y Recálculo en Cascada de Escandallos"
status: draft
version: "1.0.0"
schema-version: "1.0"
governs:
  - UC-COST-CATALOG-001
  - UC-ESCANDALLO-CALC-001
supersedes: null
superseded-by: null
---

# Business Rule: Inmutabilidad del Histórico y Recálculo en Cascada de Escandallos

## 1. Rule Definition

1. **Histórico Condicional de Costes (Solo en Uso)**: Cada vez que se modifica un Producto, una categoría de Personal, un Gasto General o un parámetro global (`IVA`, `Beneficio`), el sistema debe conservar un registro histórico inmutable con marca temporal (`timestamp`), valores anteriores y valores nuevos **únicamente si existe al menos un escandallo en el centro que esté utilizando dicho coste en el momento del cambio**. Si ningún escandallo referencia el coste modificado, el valor se actualiza directamente sin generar entradas superfluas en el histórico.
2. **Previsualización de Impacto y Recálculo Automático en Cascada (Mejora M3)**: Antes de confirmar la modificación de un coste en uso, la interfaz informa del número de escandallos que se verán afectados (*"Este cambio recalculará N escandallos"*). Al confirmar, se dispara de forma atómica el recálculo de todos los escandallos afectados del centro.
3. **Histórico de Escandallos y Evaluación del Semáforo de Margen (Mejora M2)**: Cada vez que un escandallo se recalcula, el sistema almacena una instantánea histórica del desglose (`Gastos Generales`, `Coste Persona`, `Costes de Productos`, `Escandallo Total`, `Beneficio Neto`, `Precio sin IVA`, `PVP Calculado` y `PVP Comercial Fijado`) y actualiza el estado del **Semáforo de Erosión de Margen (`🟢/🟡/🔴`)**.
4. **Baja Lógica Obligatoria de Costes con Histórico (Mejora M1)**: Si un Producto, categoría de Personal o Gasto General está en uso por algún escandallo o posee entradas en el histórico, el sistema impide su borrado físico (`DELETE`) y aplica exclusivamente **baja lógica (`Archivado`)** para preservar la integridad referencial histórica.

## 2. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial de la regla de histórico y recálculo automático. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Acotación del histórico de costes únicamente a costes referenciados por algún escandallo en el momento del cambio. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de mejoras M1 (baja lógica de costes con histórico), M2 (semáforo de margen) y M3 (previsualización de impacto). |
