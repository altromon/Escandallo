# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-cost-001.md
# ID Requerimiento: FR-COST-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-COST-001 @automated @regression @idempotence
Feature: Alta y Cálculo Unitario de Productos, Personal, Gastos Generales y Parámetros

  Scenario: Inicialización con plantilla semilla y cálculo de tasa por minuto de Gastos Generales
    Given un centro de estética recién creado carga la plantilla semilla o importa su fichero ".xlsx" con 10560 minutos laborables al mes
    When el sistema procesa los gastos generales mensuales "Local" por 335.00 EUR, "Consumos" por 100.00 EUR y "Consumibles" por 440.00 EUR
    Then el coste total mensual de gastos generales es 875.00 EUR
    And la tasa total de gastos generales por minuto es 0.08285985 EUR/min

  @boundary
  Scenario Outline: Cálculo exacto del precio unitario de productos en ml, g o ud y categorías de personal en límites del dominio
    Given un centro de estética activo con capacidad mensual configurada de <cantidad> minutos y coeficiente empresa <coef_empresa>
    When el usuario da de alta el ítem "<nombre>" de tipo "<tipo>" con cantidad <cantidad>, unidad "<unidad>", importe base <importe>, retención <retencion> y merma <merma_pct>
    Then el sistema calcula un coste empresa de <coste_empresa> EUR y un precio unitario de <precio_unitario> EUR por "<unidad_destino>"

    Examples:
      | nombre            | tipo     | cantidad | unidad      | importe | retencion | coef_empresa | merma_pct | coste_empresa | precio_unitario | unidad_destino |
      | Cleasing Milk     | producto | 500      | ml          | 34.51   | 0.00      | 1.0          | 0.00      | 34.51         | 0.06902         | ml             |
      | Crema Reafirmante | producto | 250      | g           | 50.00   | 0.00      | 1.0          | 0.05      | 50.00         | 0.20            | g              |
      | Colagen Mask      | producto | 12       | ud          | 86.52   | 0.00      | 1.0          | 0.00      | 86.52         | 7.21            | ud             |
      | Vial Monodosis    | producto | 1        | ud          | 0.01    | 0.00      | 1.0          | 0.00      | 0.01          | 0.01            | ud             |
      | Tecnico           | personal | 10560    | minutos/mes | 1100.00 | 0.20      | 1.5          | 0.00      | 1980.00       | 0.1875          | min            |
      | Enfermero         | personal | 10560    | minutos/mes | 1800.00 | 0.22      | 1.5          | 0.00      | 3294.00       | 0.31193182      | min            |
      | Doctor            | personal | 10560    | minutos/mes | 3000.00 | 0.25      | 1.5          | 0.00      | 5625.00       | 0.53267045      | min            |

  @invalid
  Scenario Outline: Rechazo de costes con cantidades nulas, divisores cero, importes negativos o mermas fuera de rango
    Given un centro de estética activo intenta registrar un ítem en el catálogo de costes
    When el usuario envía el ítem "<nombre>" de tipo "<tipo>" con cantidad <cantidad>, importe <importe>, retención <retencion> y merma <merma_pct>
    Then el servidor rechaza la operación con código HTTP <codigo_http> y código de error "<error_code>"

    Examples:
      | nombre          | tipo     | cantidad | importe  | retencion | merma_pct | codigo_http | error_code              |
      | Envase Vacío    | producto | 0        | 34.51    | 0.00      | 0.00      | 422         | INVALID_ZERO_QUANTITY   |
      | Crema Negativa  | producto | -100     | 25.00    | 0.00      | 0.00      | 422         | NEGATIVE_QUANTITY       |
      | Producto Gratis | producto | 500      | -5.00    | 0.00      | 0.00      | 422         | NEGATIVE_PRICE          |
      | Merma Excesiva  | producto | 200      | 40.00    | 0.00      | 1.50      | 422         | WASTAGE_OUT_OF_RANGE    |
      | ""              | personal | 10560    | 1100.00  | 0.20      | 0.00      | 400         | EMPTY_ITEM_NAME         |
      | Tecnico Sin Min | personal | 0        | 1100.00  | 0.20      | 0.00      | 422         | DIVISION_BY_ZERO_GUARD  |
