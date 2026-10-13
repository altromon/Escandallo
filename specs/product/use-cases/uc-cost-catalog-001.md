---
id: UC-COST-CATALOG-001
type: use-case
title: "Alta, Edición e Histórico del Catálogo de Costes y Parámetros por Centro de Estética"
status: draft
version: "1.0.0"
schema-version: "1.0"
primary-actor: ACT-ESTHETIC-USER-001
governed-by:
  - BR-CALC-FORMULAS-001
  - BR-RECALC-HISTORY-001
  - BR-TENANT-ISOLATION-001
supersedes: null
superseded-by: null
---

# Use Case: Alta, Edición e Histórico del Catálogo de Costes y Parámetros por Centro de Estética

## 1. Intent and Outcome

As a `ACT-ESTHETIC-USER-001` I want to register and update the cost catalog (`Productos`, `Personal`, `Gastos Generales`) and global parameters (`IVA`, `Beneficio`) for each of my beauty centers while preserving a complete change history To keep unit costs accurate and trigger automatic recalculation of all dependent service escandallos.

## 2. Preconditions

- El usuario está autenticado en el sistema web y ha seleccionado un centro de estética sobre el cual tiene permisos de titularidad.

## 3. Main Flow

1. El usuario selecciona el centro de estética activo o, al crear un centro nuevo, puede inicializar su catálogo en 1 clic mediante la **Plantilla Semilla Editable** (con datos base de `CalculadoraEscandallo.xlsx`), **importar un archivo `.xlsx`** o **copiar el catálogo de productos desde otro centro de estética propio** (Mejora M4).
2. El usuario da de alta o modifica uno de los bloques de costes contemplados en el modelo:
   - **Productos (`Tabla3`, Mejora M5)**: Introduce `Nombre`, `Cantidad` ($>0$), `Ud medida` (`ml`, `g`, `ud` o configurable), `Precio` (€) y `% Merma` opcional (por defecto `0%`); el sistema calcula $\text{Precio Unitario} = \text{Precio} / \text{Cantidad}$.
   - **Personal (`Tabla1`)**: Introduce `Nombre` de categoría, `Cantidad` de minutos mensuales configurables (por defecto $8 \times 60 \times 22 = 10560$), `Neto` (€/mes), `Retenciones` (decimal) y `Coeficiente Empresa` configurable (por defecto `1.5`); el sistema calcula $\text{Coste para empresa} = ((\text{Neto} \times \text{Retenciones}) + \text{Neto}) \times \text{Coeficiente Empresa}$ y $\text{Precio Unitario (€/min)} = \text{Coste para empresa} / \text{Cantidad}$.
   - **Gastos Generales (`Tabla13`)**: Introduce concepto (`Local`, `Consumos`, `Consumibles`, etc.), `Cantidad` de minutos mensuales configurables (por defecto `10560`) y `Coste para empresa` (€/mes); el sistema calcula $\text{Precio Unitario (€/min)} = \text{Coste para empresa} / \text{Cantidad}$ y actualiza el subtotal por minuto de gastos generales.
   - **Parámetros Globales (`Datos`)**: Configura los porcentajes de `IVA` (por defecto `0.21`), `Beneficio` (por defecto `0.50`), horas/día, días/mes y coeficiente de coste empresa.
3. Si el coste o parámetro modificado está siendo utilizado por al menos un escandallo del centro en el momento del cambio, el sistema muestra una **previsualización de impacto** (*"Este cambio recalculará N escandallos"*, Mejora M3) y, tras confirmar, registra la versión anterior y el nuevo valor en el histórico de costes, dispara el recálculo en cascada y añade la nueva versión al histórico de cada escandallo. Si ningún escandallo lo utiliza, actualiza el valor directamente sin generar entradas en el histórico.

## 4. Alternative and Exception Flows

- **E1 – Cantidad cero o negativa en Producto o Capacidad**: El sistema rechaza la operación con error de validación evitando división por cero (`#DIV/0!`).
- **E2 – Acceso a centro de otro cliente**: El sistema deniega la operación conforme a `BR-TENANT-ISOLATION-001`.
- **A1 – Baja lógica de coste con histórico (Mejora M1)**: Si el usuario solicita eliminar un coste que está en uso o posee registros históricos, el sistema impide el borrado físico y lo pasa a estado `Archivado` (`archived`).

## 5. Postconditions

- El catálogo de costes del centro queda actualizado y, para aquellos costes en uso, se conserva el histórico y todos los escandallos dependientes quedan recalculados.

## 6. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del caso de uso de catálogo de costes. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Parámetros numéricos 100% configurables e histórico condicional solo cuando un escandallo usa el coste. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de mejoras M1 (baja lógica), M3 (previsualización de impacto), M4 (plantilla semilla, importación `.xlsx` y copia entre centros) y M5 (`g` y `% Merma`). |
