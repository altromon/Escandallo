---
id: FR-COST-001
type: requirement
title: Alta y Cálculo Unitario de Productos, Personal, Gastos Generales y Parámetros
status: active
version: 1.0.0
schema-version: '1.0'
category: functional
derives-from:
  - UC-COST-CATALOG-001
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - '@FR-COST-001'
  - '@automated'
  - '@regression'
supersedes: null
superseded-by: null
---


# Requirement: Alta y Cálculo Unitario de Productos, Personal, Gastos Generales y Parámetros

## 1. Normative Statement

El sistema DEBE permitir registrar, inicializar mediante plantilla semilla editable o importación de archivo `.xlsx` (Mejora M4), y actualizar para cada centro de estética los tres tipos de costes y los parámetros globales contemplados en `CalculadoraEscandallo.xlsx`, garantizando que todos los valores numéricos sean introducidos o configurables por el usuario y calculando en servidor: (a) el precio unitario de Productos como `Precio / Cantidad` en `ml`, `g`, `ud` u otra unidad configurada junto con su `% Merma` opcional (por defecto `0%`, Mejora M5), (b) el coste de empresa de Personal como `((Neto * Retenciones) + Neto) * Coeficiente_Empresa` (por defecto `1.5`) y su coste por minuto dividiendo entre la `Cantidad` de minutos mensuales configurados (por defecto `10560`), (c) el coste por minuto de cada Gasto General (`Coste / Cantidad`) y su suma total por minuto, y (d) los parámetros globales configurables de `IVA` y `Beneficio`.

## 2. Acceptance Criteria (Gherkin)

```gherkin
@FR-COST-001 @automated @regression
Feature: Alta y Cálculo Unitario de Productos, Personal, Gastos Generales y Parámetros

  Scenario: Inicialización con plantilla semilla y cálculo de tasa por minuto de Gastos Generales
    Given un centro de estética recién creado carga la plantilla semilla o importa su fichero ".xlsx" con 10560 minutos laborables al mes
    When el sistema procesa los gastos generales mensuales "Local" por 335.00 EUR, "Consumos" por 100.00 EUR y "Consumibles" por 440.00 EUR
    Then el coste total mensual de gastos generales es 875.00 EUR
    And la tasa total de gastos generales por minuto es 0.08285985 EUR/min

  Scenario Outline: Cálculo del precio unitario de productos en ml, g o ud y categorías de personal con parámetros configurables
    Given un centro de estética activo con capacidad mensual configurada de <cantidad> minutos y coeficiente empresa <coef_empresa>
    When el usuario da de alta el ítem "<nombre>" de tipo "<tipo>" con cantidad <cantidad>, unidad "<unidad>", importe base <importe> y retención <retencion>
    Then el sistema calcula un coste empresa de <coste_empresa> EUR y un precio unitario de <precio_unitario> EUR por "<unidad_destino>"

    Examples:
      | nombre           | tipo     | cantidad | unidad      | importe | retencion | coef_empresa | coste_empresa | precio_unitario | unidad_destino |
      | Cleasing Milk    | producto | 500      | ml          | 34.51   | 0.00      | 1.0          | 34.51         | 0.06902         | ml             |
      | Crema Reafirmante| producto | 250      | g           | 50.00   | 0.00      | 1.0          | 50.00         | 0.20            | g              |
      | Colagen Mask     | producto | 12       | ud          | 86.52   | 0.00      | 1.0          | 86.52         | 7.21            | ud             |
      | Tecnico          | personal | 10560    | minutos/mes | 1100.00 | 0.20      | 1.5          | 1980.00       | 0.1875          | min            |
      | Enfermero        | personal | 10560    | minutos/mes | 1800.00 | 0.22      | 1.5          | 3294.00       | 0.31193182      | min            |
      | Doctor           | personal | 10560    | minutos/mes | 3000.00 | 0.25      | 1.5          | 5625.00       | 0.53267045      | min            |

  Scenario Outline: Rechazo controlado de costes con valores fuera de dominio, negativos o divisores cero
    Given un centro de estética activo intenta registrar o actualizar un coste de tipo "<tipo>"
    When introduce cantidad <cantidad_inv>, importe <importe_inv> y porcentaje de merma <merma_inv>
    Then el servidor rechaza la operación con código HTTP <codigo_error> y mensaje de validación "<motivo_rechazo>"

    Examples:
      | tipo     | cantidad_inv | importe_inv | merma_inv | codigo_error | motivo_rechazo                  |
      | producto | 0            | 34.51       | 0.05      | 422          | CANTIDAD_DIVISOR_CERO           |
      | producto | -100         | 34.51       | 0.05      | 422          | CANTIDAD_NEGATIVA_INVALIDA      |
      | producto | 500          | -15.00      | 0.05      | 422          | IMPORTE_NEGATIVO_INVALIDO       |
      | producto | 250          | 50.00       | 1.05      | 422          | MERMA_FUERA_DE_RANGO_0_A_99_PCT |
      | personal | 0            | 1100.00     | 0.00      | 422          | MINUTOS_MENSUALES_DIVISOR_CERO  |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial con datos de prueba reales extraídos de `CalculadoraEscandallo.xlsx`. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Todos los valores y coeficientes numéricos son configurables por el usuario. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de mejoras M4 (plantilla semilla e importación `.xlsx`) y M5 (unidad `g` y `% Merma`). |


## Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-07 | agent-devops | Implementación verificada (TSK-001, TSK-002, TSK-003, TSK-004, TSK-005, TSK-006, TSK-007) | chg-001-escandallo-mvp |
