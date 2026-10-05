---
id: BR-CALC-FORMULAS-001
type: business-rule
title: "Fórmulas Matemáticas Canónicas de Costes, Escandallo, Beneficio y PVP"
status: draft
version: "1.0.0"
schema-version: "1.0"
governs:
  - UC-COST-CATALOG-001
  - UC-ESCANDALLO-CALC-001
  - UC-EXEC-REPORT-001
supersedes: null
superseded-by: null
---

# Business Rule: Fórmulas Matemáticas Canónicas de Costes, Escandallo, Beneficio y PVP

## 1. Rule Definition

Todos los cálculos del sistema deben reproducir con exactitud determinista las fórmulas de la hoja de cálculo de referencia `CalculadoraEscandallo.xlsx`, garantizando que **todos los valores numéricos y coeficientes sean introducidos por el usuario o configurables desde la propia aplicación** (los valores del Excel actúan únicamente como valores iniciales por defecto):

1. **Parámetros Operativos y Fiscales Configurables por Centro (`Datos` y Capacidad)**:
   - `Horas laborables/día` (por defecto `8`), `Minutos/hora` (`60`) y `Días laborables/mes` (por defecto `22`), o directamente `Capacidad Mensual (minutos/mes)` configurable por centro y por categoría de personal (por defecto $8 \times 60 \times 22 = 10560\text{ minutos/mes}$).
   - `Coeficiente de Coste Empresa / Seguridad Social` (por defecto `1.5`, configurable por centro o categoría de personal).
   - `IVA` aplicable (por defecto `0.21` = 21%, configurable por centro o servicio).
   - `Beneficio` objetivo (por defecto `0.50` = 50% sobre coste, configurable por centro o servicio).

2. **Coste Unitario de Producto, Unidades de Cabina y Merma (`Tabla3 - Productos`, Mejora M5)**:
   - Datos introducidos por el usuario: `Nombre`, `Cantidad` (capacidad del envase $> 0$), `Ud medida` (`ml`, `g`, `ud` u otra unidad configurada), `Precio` (€ de compra del envase) y `% Merma` opcional configurable (por defecto `0%`).
   - El sistema incluye un asistente de equivalencia rápida de dosificación en cabina (*1 gota ≈ 0,05 ml, 1 pulsación/pump ≈ 2 ml, 1 cucharadita ≈ 5 ml*) que convierte la dosis a la unidad del envase.
   - Fórmula:
     $$\text{Precio Unitario Producto} = \frac{\text{Precio}}{\text{Cantidad}}$$
   - Restricción: `Cantidad` debe ser estrictamente mayor que cero para evitar división por cero (`#DIV/0!`).

3. **Coste por Minuto de Personal (`Tabla1 - Personal`)**:
   - Datos introducidos/configurados por el usuario: `Nombre` de categoría, `Cantidad` de minutos laborables mensuales ($\text{Horas/día} \times 60 \times \text{Días/mes}$, por defecto `10560`), `Neto` (€/mes), `Retenciones` (tanto por uno, ej. `0.20`, `0.22`, `0.25`) y `Coeficiente Empresa` (por defecto `1.5`).
   - Fórmulas:
     $$\text{Coste para empresa} = ((\text{Neto} \times \text{Retenciones}) + \text{Neto}) \times \text{Coeficiente Empresa}$$
     $$\text{Precio Unitario Personal (€/min)} = \frac{\text{Coste para empresa}}{\text{Cantidad}}$$

4. **Tasa por Minuto de Gastos Generales (`Tabla13 - Gastos Generales`)**:
   - Datos introducidos/configurados por el usuario para cada concepto (`Local`, `Consumos`, `Consumibles`, etc.): `Nombre`, `Cantidad` de minutos mensuales configurados (por defecto `10560`), `Coste para empresa` (€/mes, directo o calculado como $\text{Coste diario} \times \text{Días laborables/mes}$).
   - Fórmulas:
     $$\text{Precio Unitario Gasto Individual (€/min)} = \frac{\text{Coste para empresa}}{\text{Cantidad}}$$
     $$\text{Tasa Total Gastos Generales (€/min)} = \sum_{g=1}^{M} \text{Precio Unitario Gasto Individual}_g$$

5. **Composición Dinámica del Escandallo por Servicio (`Tabla5 - Escandallo`, Mejoras M3 y M5)**:
   - Datos introducidos por el usuario mediante constructor dinámico de líneas: `Nombre` del servicio, `Minutos` (duración $> 0$), `Persona` (categoría de personal asignada) y lista dinámica de productos consumidos $(i = 1 \dots N)$ con `Producto i`, `Cantidad i` y `% Merma i` (por defecto `0%`).
   - Fórmulas:
     $$\text{Gastos Generales Imputados} = \text{Minutos} \times \text{Tasa Total Gastos Generales (€/min)}$$
     $$\text{Coste Persona} = \text{Precio Unitario Personal (€/min)} \times \text{Minutos}$$
     $$\text{Coste Producto}_i = \text{Precio Unitario Producto}_i \times \text{Cantidad}_i \times (1 + \text{Merma}_i)$$
     $$\text{Escandallo Total} = \text{Gastos Generales Imputados} + \text{Coste Persona} + \sum_{i=1}^{N} \text{Coste Producto}_i$$

6. **Cálculo de Beneficio Neto, PVP Calculado, PVP Comercial Fijado y Semáforo de Margen (`Tabla57`, Mejora M2)**:
   - **Modo Directo (a partir de `% Beneficio` objetivo)**:
     $$\text{Beneficio Neto Unitario (€)} = \text{Escandallo Total} \times \text{Beneficio}$$
     $$\text{Precio con Beneficio (Base Imponible sin IVA)} = \text{Escandallo Total} \times (1 + \text{Beneficio})$$
     $$\text{PVP Calculado (con IVA)} = \text{Precio con Beneficio} \times (1 + \text{IVA})$$
   - **Modo Simulación Inversa y `PVP Comercial Fijado` (a partir del precio de carta fijado por el centro)**:
     $$\text{Precio sin IVA Real} = \frac{\text{PVP Comercial Fijado}}{1 + \text{IVA}}$$
     $$\text{Beneficio Neto Real (€)} = \text{Precio sin IVA Real} - \text{Escandallo Total}$$
     $$\text{Beneficio Real (\%)} = \frac{\text{Beneficio Neto Real (€)}}{\text{Escandallo Total}}$$
   - **Semáforo de Erosión de Margen (`🟢 / 🟡 / 🔴`)**:
     - `🟢 Óptimo`: $\text{Beneficio Real (\%)} \ge \text{Beneficio Objetivo (\%)}$.
     - `🟡 Alerta (Margen erosionado)`: $0 < \text{Beneficio Real (\%)} < \text{Beneficio Objetivo (\%)}$.
     - `🔴 Crítico (Pérdida)`: $\text{Beneficio Real (\%)} \le 0$.

## 2. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Extracción íntegra de las fórmulas de `CalculadoraEscandallo.xlsx`. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Todos los valores numéricos configurables por el usuario, visualización de Beneficio Neto (€) y simulación inversa desde PVP. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de mejoras M2 (`PVP Comercial Fijado` y Semáforo de Margen), M3 (constructor dinámico) y M5 (`g`, dosis asistida y `% Merma`). |
