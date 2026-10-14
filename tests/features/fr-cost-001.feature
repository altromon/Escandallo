# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-cost-001.md
# ID Requerimiento: FR-COST-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

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
    Given un centro de estética activo intenta registrar un coste de tipo "<tipo>"
    When envía una cantidad <cantidad>, importe base <importe> y porcentaje de merma <merma_pct>
    Then el servidor rechaza el registro con código HTTP 422 y motivo "<error_validacion>"

    Examples:
      | tipo          | cantidad | importe | merma_pct | error_validacion                |
      | producto      | 0        | 34.51   | 0         | CANTIDAD_DIVISOR_CERO           |
      | producto      | -100     | 34.51   | 0         | CANTIDAD_NEGATIVA_INVALIDA      |
      | producto      | 500      | -10.00  | 0         | IMPORTE_NEGATIVO_INVALIDO       |
      | producto      | 500      | 34.51   | 100       | MERMA_FUERA_DE_RANGO_0_A_99_PCT |
      | gasto_general | 0        | 875.00  | 0         | MINUTOS_MENSUALES_DIVISOR_CERO  |
