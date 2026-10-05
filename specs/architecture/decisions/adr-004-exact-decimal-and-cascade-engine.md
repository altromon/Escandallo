---
id: ADR-004-EXACT-DECIMAL-AND-CASCADE-ENGINE
type: architecture-decision-record
title: "Aritmética Decimal Exacta (decimal.js / NUMERIC) y Recálculo Transaccional Síncrono en Cascada con Snapshots Inmutables"
status: proposed
version: "1.0.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
  - "Product Owner"
decision-date: "2026-10-05"
affects-components:
  - CMP-COST-ENGINE-001
supersedes: null
superseded-by: null
---

# ADR-004: Aritmética Decimal Exacta (decimal.js / NUMERIC) y Recálculo Transaccional Síncrono en Cascada con Snapshots Inmutables

## 1. Context and Problem Statement

Las fórmulas financieras del dominio (`BR-CALC-FORMULAS-001`) combinan micro-costes por minuto de aparatología y gastos generales con dosis fraccionarias de cosméticos en gramos o mililitros, porcentajes de merma (`M5`), márgenes porcentuales, comisiones de venta e impuestos (IVA). Asimismo, cuando un usuario modifica cualquier coste base o parámetro global, el sistema debe mostrar antes una previsualización de impacto (*"Este cambio recalculará N escandallos"*, `M3`) y, al confirmar, recalcular en cascada todos los escandallos afectados guardando histórico inmutable solo si el coste estaba en uso activo (`BR-RECALC-HISTORY-001`).

## 2. Considered Technology Options

1. **Opción A (Elegida): Aritmética Decimal Exacta (`decimal.js` + PostgreSQL `NUMERIC(14,4)`) y Recálculo Síncrono en Transacción ACID Única**:
   - **Precisión**: Funciones puras de dominio utilizando `decimal.js` (`MIT`) configuradas con redondeo `ROUND_HALF_UP`, conservando 4 decimales en cálculos intermedios y 2 decimales en importes finales de presentación (`EUR`).
   - **Recálculo en Cascada Síncrono**: Dado que un centro de estética típico tiene entre 10 y 150 servicios en su carta y cada receta está acotada a $\le 100$ líneas (`SEC-REQ-DOS-001`), recalcular 150 escandallos en memoria y actualizar sus filas junto con sus snapshots históricos en PostgreSQL toma **$< 15\text{ ms}$** dentro de una única transacción ACID.
2. **Opción B: Coma Flotante IEEE 754 (`number` en JS / `DOUBLE PRECISION` en SQL)**:
   - **Contras**: Produce errores de redondeo acumulativos (ej. `0.1 + 0.2 = 0.30000000000000004`) inaceptables en informes ejecutivos y comparativas con la hoja Excel de referencia.
3. **Opción C: Recálculo Asíncrono Eventual mediante Colas Externas (Redis/BullMQ/RabbitMQ)**:
   - **Contras**: Introduce inconsistencia temporal en la interfaz (la gestora edita un coste y al volver al listado ve precios antiguos o semáforos `🟢/🟡/🔴` desactualizados), además de requerir un servidor Redis adicional que encarece la memoria del despliegue (`ADR-001`).

## 3. Decision Outcome

Se adopta la **Opción A (Motor de Dominio Puro con `decimal.js`, Tipos `NUMERIC(14,4)` en PostgreSQL y Recálculo en Cascada Síncrono Transaccional)**.

### Reglas de Ejecución del Motor (`CMP-COST-ENGINE-001`)

1. **Endpoint de Previsualización de Impacto (`M3`)**: Una consulta ligera (`SELECT COUNT(DISTINCT escandallo_id) FROM escandallo_items WHERE cost_id = $1`) devuelve instantáneamente el número $N$ de escandallos afectados y cuántos de ellos cambiarán de color en el **Semáforo de Erosión de Margen (`🟢/🟡/🔴`, M2)** con respecto a su `PVP Comercial Fijado`.
2. **Transacción Atómica de Recálculo e Histórico Condicional (`BR-RECALC-HISTORY-001`)**:
   - Si $N = 0$ (el coste no está vinculado a ningún escandallo), se actualiza el coste directamente sin insertar registro en `cost_history`.
   - Si $N \ge 1$, dentro de la misma transacción `BEGIN ... COMMIT`:
     1. Se inserta el snapshot anterior del coste en `cost_history ( append-only )`.
     2. Se actualiza el registro en `costs`.
     3. Se cargan los $N$ escandallos vinculados, se insertan sus estados previos en `escandallo_history ( append-only )` con motivo `CASCADE_COST_UPDATE`, y se actualizan sus importes (`Coste Total`, `Beneficio Neto`, `Precio sin IVA`, `PVP Calculado`, `Margen Real %` y estado del semáforo `🟢/🟡/🔴`).
3. **Protección de Integridad Histórica (`M1`)**: La tabla `costs` impide el `DELETE` físico si existen filas en `cost_history` o `escandallo_items`, permitiendo únicamente el cambio a `status = 'archived'`.

## 4. Consequences

- **Positive**:
  - Consistencia inmediata 100% determinista tras editar cualquier coste: el usuario ve al instante los nuevos márgenes y alertas de erosión sin estados intermedios.
  - Funciones de cálculo puras y pequeñas ($\le 40$ líneas, complejidad ciclomática $\le 10$), ideales para verificación unitaria exhaustiva.
- **Negative / Trade-offs**:
  - Se protege el endpoint de mutación de costes con el límite de `30 mutaciones/minuto` por inquilino (`SEC-REQ-DOS-001`) para evitar abuso computacional.

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Formal decision proposal | ARCH-INIT-001 |
