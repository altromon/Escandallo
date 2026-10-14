---
id: ADR-004-EXACT-DECIMAL-AND-CASCADE-ENGINE
type: architecture-decision-record
title: "Aritmética Financiera Exacta de 128 Bits (System.Decimal / NUMERIC 18,6) y Motor Transaccional Síncrono de Cascada"
status: proposed
version: "1.1.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
decision-date: "2026-10-08"
affects-components:
  - CMP-COST-ENGINE-001
  - CMP-WEB-UI-001
  - CMP-IAM-TENANT-001
supersedes: null
superseded-by: null
---

# ADR-004: Aritmética Financiera Exacta de 128 Bits (System.Decimal / NUMERIC 18,6) y Motor Transaccional Síncrono de Cascada

## 1. Contexto y Planteamiento del Problema

Las fórmulas de escandallo estético (`BR-CALC-FORMULAS-001`) dividen precios de envases cosméticos por dosis milimétricas (ej. `1,5 ml` de un vial de `50 ml` con `5%` de merma), suman amortizaciones por minuto y calculan márgenes netos descontando IVA y comisiones TPV. El uso de coma flotante binaria (`double`/`float`) introduce errores de representación (ej. `0.1 + 0.2 != 0.3`) que pueden provocar cambios espurios en el Semáforo de Erosión de Margen o generar snapshots históricos falsos cuando el coste real no ha variado (`BR-RECALC-HISTORY-001`). Asimismo, el motor `System.Decimal` debe estar protegido contra entradas extremas que provoquen `OverflowException` o `DivideByZeroException` (`SEC-REQ-XLSX-001`), y el recálculo en cascada debe ser resistente a denegación de servicio asimétrica (`SEC-REQ-DOS-001`).

## 2. Opciones Tecnológicas Consideradas

1. **Opción A (Recomendada — `System.Decimal` 128-bit con Guardas de Rango en C# + `NUMERIC(18,6)` en PostgreSQL + Cascada Transaccional Síncrona Acotada a $\le 200$ Escandallos y $\le 100$ Líneas por Servicio)**:
   - Todos los cálculos intermedios operan en base 10 exacta con 6 decimales de precisión interna (`MidpointRounding.AwayFromZero`) y 2 decimales en presentación monetaria, precedidos por guardas deterministas de dominio. El recálculo en cascada se ejecuta síncronamente dentro de una transacción ACID en PostgreSQL acotada a `200` escandallos por insumo (`< 2000 ms`) y `30 mutaciones/minuto` por inquilino.
2. **Opción B (Aritmética en Enteros de Céntimos `long` + Cola Asíncrona Externa con Redis/RabbitMQ)**:
   - Trabajar en céntimos enteros pierde precisión en costes unitarios por mililitro o segundo de aparatología; añadir Redis/RabbitMQ incrementa el consumo de RAM y el coste del VPS sin necesidad real para volúmenes de centros de estética ($\le 200$ tratamientos por centro).

## 3. Decisión Adoptada

Se elige la **Opción A (`System.Decimal` 128-bit con Guardas de Rango + `NUMERIC(18,6)` + Cascada Transaccional Síncrona con Historial Condicional)**.

**Justificación técnica (`SEC-REQ-XLSX-001`, `SEC-REQ-DOS-001`)**:
- **Determinismo Absoluto**: `System.Decimal` en .NET y `NUMERIC(18,6)` en PostgreSQL representan fracciones en base 10 de forma exacta, garantizando que $\Delta \text{Coste} = 0$ cuando se editan únicamente metadatos cosméticos (cumpliendo estrictamente `BR-RECALC-HISTORY-001` y `FR-HIST-001`).
- **Prevención de `OverflowException` y `DivideByZeroException` (`SEC-REQ-XLSX-001`)**: Antes de operar en `System.Decimal` o persistir en `NUMERIC(18,6)`, el motor rechaza con `422 Unprocessable Entity` entradas no numéricas (`NaN`/`Infinity`), negativas, divisores `0` o valores que excedan las cotas operativas (`PurchasePrice > 1_000_000m`, `UnitsPerPackage < 0.0001m`, `WastePct > 100m`).
- **Experiencia de Usuario Inmediata (M3) y Protección Anti-DoS (`SEC-REQ-DOS-001`)**: La usuaria visualiza el impacto antes de confirmar (`PreviewCascadeImpactAsync`) y actualiza todos sus tratamientos en `< 2000 ms`. Las cotas duras de `100 líneas de coste máximas por escandallo` (`422`), `200 escandallos vinculados por insumo` y `30 mutaciones de catálogo por minuto` (`429`) aseguran un techo determinista de CPU y memoria.

## 4. Consecuencias

- **Positivas**:
  - Cero discrepancias de redondeo y cero caídas por `OverflowException` entre simulación en pantalla, base de datos e informes ejecutivos `.pdf`/`.xlsx`.
  - Arquitectura libre de brokers de mensajería externos en el MVP.
- **Negativas / Compensaciones (Trade-offs)**:
  - Queda prohibido por regla de compilación/revisión el uso de `double` o `float` en cualquier clase del espacio de nombres `Escandallo.Domain`.

---

## 5. Historial de Revisiones y Control de Versiones

| Versión | Fecha | Decisores | Estado de la Decisión | Referencia (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-04-12 | `agent-system-architect` | Propuesta formal aceptada | `chg-001-escandallo-mvp` |
| **1.1.0** | 2026-10-08 | `agent-system-architect` | Incorporación de guardas anti-`OverflowException` en `System.Decimal` (`SEC-REQ-XLSX-001`), cota de 100 líneas/servicio y 30 mutaciones/min (`SEC-REQ-DOS-001`) | `chg-001-escandallo-mvp` |
