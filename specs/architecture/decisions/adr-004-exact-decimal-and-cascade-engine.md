---
id: ADR-004-EXACT-DECIMAL-AND-CASCADE-ENGINE
type: architecture-decision-record
title: "Aritmética Decimal Exacta en C# (System.Decimal 128-bit / NUMERIC) y Recálculo Transaccional Síncrono en Cascada con Snapshots Inmutables"
status: proposed
version: "1.1.0"
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

# ADR-004: Aritmética Decimal Exacta en C# (System.Decimal 128-bit / NUMERIC) y Recálculo Transaccional Síncrono en Cascada con Snapshots Inmutables

## 1. Context and Problem Statement

Las fórmulas financieras del dominio (`BR-CALC-FORMULAS-001`) combinan micro-costes por minuto de aparatología y gastos generales con dosis fraccionarias de cosméticos en gramos o mililitros, porcentajes de merma (`M5`), márgenes porcentuales y tipos de IVA. Asimismo, cuando un usuario modifica cualquier coste base o parámetro global, el sistema debe mostrar antes una previsualización de impacto (*"Este cambio recalculará N escandallos"*, `M3`) y, al confirmar, recalcular en cascada todos los escandallos afectados guardando histórico inmutable solo si el coste estaba en uso activo (`BR-RECALC-HISTORY-001`).

## 2. Considered Technology Options

1. **Opción A (Elegida): Aritmética Decimal Exacta Nativa en C# (`System.Decimal` de 128 bits + PostgreSQL `NUMERIC(14,8)`) y Recálculo Síncrono en Transacción ACID Única**:
   - **Precisión**: Métodos puros de dominio utilizando el tipo nativo `decimal` de C# con redondeo `MidpointRounding.AwayFromZero` a 8 decimales (`Math.Round(val, 8, MidpointRounding.AwayFromZero)`), garantizando paridad exacta al octavo decimal con `Escandallo.xlsx`.
   - **Recálculo en Cascada Síncrono**: Dado que un centro de estética típico tiene entre 10 y 150 servicios en su carta y cada receta está acotada a $\le 100$ líneas (`SEC-REQ-DOS-001`), recalcular 150 escandallos en memoria en el CLR de `.NET` y actualizar sus filas en PostgreSQL toma **$< 5\text{ ms}$** dentro de una única transacción ACID.
2. **Opción B: Coma Flotante Binaria IEEE 754 (`double` / `float` en C# o `DOUBLE PRECISION` en SQL)**:
   - **Contras**: Produce errores de representación binaria (`0.1 + 0.2 != 0.3`) inaceptables en informes ejecutivos y comparativas con la hoja Excel de referencia.
3. **Opción C: Recálculo Asíncrono Eventual mediante Colas Externas (Redis/RabbitMQ)**:
   - **Contras**: Introduce inconsistencia temporal en la interfaz y requiere procesos adicionales que encarecen la memoria del despliegue (`ADR-001`).

## 3. Decision Outcome

Se adopta la **Opción A (Motor de Dominio Puro en C# con `System.Decimal`, Tipos `NUMERIC` en PostgreSQL y Recálculo en Cascada Síncrono Transaccional)**.

### Reglas de Ejecución del Motor (`CMP-COST-ENGINE-001`)

1. **Endpoint de Previsualización de Impacto (`M3`)**: Devuelve instantáneamente el número $N$ de escandallos afectados y cuántos de ellos cambiarán de estado en el **Semáforo de Erosión de Margen (`OPTIMO`/`ALERTA`/`CRITICO`, M2)** con respecto a su `PVP Comercial Fijado`.
2. **Transacción Atómica de Recálculo e Histórico Condicional (`BR-RECALC-HISTORY-001`)**:
   - Si $N = 0$ (el coste no está vinculado a ningún escandallo), se actualiza el coste directamente sin insertar registro en `cost_history`.
   - Si $N \ge 1$, dentro de la misma transacción `BEGIN ... COMMIT`:
     1. Se inserta el snapshot anterior del coste en `cost_history (append-only)`.
     2. Se actualiza el registro en `costs`.
     3. Se cargan los $N$ escandallos vinculados, se insertan sus estados previos en `escandallo_history (append-only)` con incremento de versión (`version + 1`), y se actualizan sus importes con precisión `decimal`.
3. **Protección de Integridad Histórica (`M1`)**: Se impide el borrado físico (`409 Conflict`) si existen filas vinculadas en histórico o escandallos, aplicando baja lógica (`status = 'archived'`).

## 4. Consequences

- **Positive**:
  - Cero dependencias externas para cálculo financiero gracias a `System.Decimal` de C#.
  - Consistencia inmediata 100% determinista tras editar cualquier coste.
  - Métodos de cálculo puros y concisos ($\le 40$ líneas, complejidad ciclomática $\le 10$), verificados con `xUnit`.
- **Negative / Trade-offs**:
  - Se protege el endpoint de mutación de costes con el límite de `30 mutaciones/minuto` por inquilino (`SEC-REQ-DOS-001`).

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Formal decision proposal | ARCH-INIT-001 |
| **1.1.0** | 2026-10-05 | Product Owner, agent-system-architect | Adopción de System.Decimal nativo de C# (.NET) | CHG-001-ESCANDALLO-MVP |
