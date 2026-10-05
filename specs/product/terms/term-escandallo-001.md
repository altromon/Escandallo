---
id: TERM-ESCANDALLO-001
type: term
title: "Glosario de Dominio de Escandallo para Centros de Estética"
status: draft
version: "1.0.0"
schema-version: "1.0"
bounded-context: "costing-and-pricing"
supersedes: null
superseded-by: null
---

# Term: Glosario de Dominio de Escandallo para Centros de Estética

## 1. Definiciones Canónicas

- **Escandallo Total (`Escandallo Total`)**: Coste total unitario de ejecución de un servicio o tratamiento estético, resultante de sumar la imputación por minuto de los **Gastos Generales**, el **Coste de Personal** según la duración del servicio y la suma de los **Costes de Productos** consumidos.
- **Coste de Producto (`Tabla3 - Productos`)**: Coste directo de los cosméticos, principios activos o fungibles específicos empleados en una sesión, calculado como $\text{Precio Unitario} = \frac{\text{Precio Envase}}{\text{Cantidad Envase}}$ (€/ml o €/ud) multiplicado por las unidades o mililitros consumidos en el servicio.
- **Coste de Personal (`Tabla1 - Personal`)**: Coste directo de mano de obra por categoría profesional (`Técnico`, `Técnico Superior`, `Encargado`, `Enfermero`, `Doctor`), donde $\text{Coste Empresa} = \text{Neto} \times (1 + \text{Retenciones}) \times 1.5$ dividido entre los minutos laborables mensuales (`10560 min/mes` por defecto).
- **Gastos Generales (`Tabla13 - Gastos Generales`)**: Costes estructurales e indirectos mensuales del centro de estética (`Local`, `Consumos`, `Consumibles`), prorrateados por minuto laborable mensual (`10560 min/mes` por defecto).
- **Precio con Beneficio / Base Imponible (`Beneficio` en `Tabla57`)**: Precio del servicio antes de impuestos tras aplicar el porcentaje de beneficio/recargo ($\text{Escandallo Total} \times (1 + \text{Beneficio\%})$).
- **Beneficio Neto Unitario**: Ganancia neta antes de impuestos obtenida por cada ejecución del servicio ($\text{Precio con Beneficio} - \text{Escandallo Total}$).
- **PVP Calculado (`PVP Recomendado`)**: Precio final teórico con IVA incluido ($\text{Precio con Beneficio} \times (1 + \text{IVA\%})$).
- **PVP Comercial Fijado (`Tarifa de Carta`)**: Precio real con IVA que el centro de estética cobra al público en su carta comercial, utilizado para contrastar la rentabilidad real frente al `PVP Calculado` tras cualquier recálculo de costes.
- **Semáforo de Erosión de Margen (`🟢 / 🟡 / 🔴`)**: Indicador visual de alerta por servicio que compara el margen real del `PVP Comercial Fijado` frente al objetivo del centro: `🟢 Óptimo` (margen real $\ge$ objetivo), `🟡 Alerta` (margen positivo pero inferior al objetivo tras subida de costes) y `🔴 Crítico` (beneficio nulo o pérdida, $\text{PVP sin IVA} \le \text{Escandallo Total}$).
- **Merma (`% Merma`)**: Porcentaje configurable de desperdicio operativo de producto en cabina (por defecto `0%`), aplicado al consumo unitario ($\text{Cantidad} \times (1 + \text{Merma})$).
- **Estado Archivado (`archived`)**: Baja lógica reversible aplicada a escandallos (para respetar el límite gratuito de $\le 2$ escandallos activos sin perder datos ni histórico) o a costes que ya poseen registros en el histórico.

## 2. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del glosario extraído de `CalculadoraEscandallo.xlsx`. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de términos de las mejoras MVP M1–M5 (PVP Comercial Fijado, Semáforo de Margen, Merma y Archivado). |
