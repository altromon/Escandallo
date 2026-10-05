# Expert User Design Evaluation & MVP Boundary Analysis

> **Acting Role**: `agent-expert-user` (Expert User & Domain Evaluator)
> **Evaluated Baseline**: `specs/product/` (`v1.1.0` tras incorporar feedback del Product Owner)
> **Date**: 2026-10-05

---

## 1. Incorporación de Comentarios del Product Owner (v1.1.0)

Se han actualizado y verificado criptográficamente los artefactos `UC-*`, `BR-*`, `FR-*` y `QR-*` con todas tus directrices:
1. **Informes Ejecutivos ([`UC-EXEC-REPORT-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/use-cases/uc-exec-report-001.md), [`FR-REPORT-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/requirements/fr-report-001.md))**: Exportación acotada exclusivamente a **PDF (`.pdf`)** y **Excel (`.xlsx`)**.
2. **Observabilidad del Mantenedor ([`UC-OBSERVABILITY-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/use-cases/uc-observability-001.md), [`FR-OBS-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/requirements/fr-obs-001.md))**: Soporte dual (descarga directa desde UI web + envío a stack externo) con **logs estructurados en formato estándar** y **granularidad configurable en caliente** (`DEBUG`, `INFO`, `WARN`, `ERROR`, `SECURITY`).
3. **Bloqueo Estricto al Superar Límite Gratuito ([`UC-SUBSCRIPTION-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/use-cases/uc-subscription-001.md), [`BR-FREEMIUM-QUOTA-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/business-rules/br-freemium-quota-001.md), [`FR-BILLING-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/requirements/fr-billing-001.md))**: Se permite recalcular libremente dentro de los 2 escandallos gratuitos por centro; si tras cancelar/impagar la suscripción el centro supera los 2 escandallos, **se bloquea también la edición y exportación de cualquier escandallo** hasta volver a respetar el límite ($\le 2$) o reactivar la suscripción. Incluye propuesta de precio benchmark (**14,90 €/mes** o **149 €/año** por centro).
4. **Configurabilidad Total y Simulación Inversa ([`BR-CALC-FORMULAS-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/business-rules/br-calc-formulas-001.md), [`FR-COST-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/requirements/fr-cost-001.md), [`FR-CALC-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/requirements/fr-calc-001.md))**: Todos los valores numéricos (minutos/mes, horas/día, días/mes, coeficiente empresa `1.5`, `% IVA`, `% Beneficio`) son configurables o introducidos por el usuario; se muestra `Beneficio Neto (€)` y `Precio sin IVA` y se soporta simulación inversa desde un `PVP Objetivo`.
5. **Histórico Condicional de Costes ([`BR-RECALC-HISTORY-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/business-rules/br-recalc-history-001.md), [`FR-HIST-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/requirements/fr-hist-001.md))**: Solo se registra histórico de un coste cuando al menos un escandallo lo estaba utilizando en el momento del cambio.
6. **Orientaciones de Pantalla ([`QR-UI-A11Y-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/requirements/qr-ui-a11Y-001.md))**: Soporte y validación explícita en orientación vertical (`portrait`) y horizontal (`landscape`) en móvil, tablet y PC.

---

## 2. Escrutinio Experto y Desafíos a la Especificación Actual

Adoptando la perspectiva real de **`ACT-ESTHETIC-USER-001`** (directora de clínica/centro de estética operando desde móvil/tablet entre cabinas) y **`ACT-MAINTAINER-001`**, identifico **6 puntos ciegos operativos y de usabilidad** en la especificación actual:

### Desafío 1: Cómo desbloquear un centro tras cancelar la suscripción sin destruir datos (`BR-FREEMIUM-QUOTA-001`)
- **Fricción detectada**: Si una clienta pagaba suscripción, creó 15 escandallos y cancela su suscripción un mes, la regla bloquea crear, editar y exportar cualquier escandallo hasta volver al límite gratuito ($\le 2$). Si la única forma de bajar de 15 a 2 escandallos es **borrarlos definitivamente**, perderá todo su trabajo e histórico y jamás volverá a suscribirse. Además, si borra un producto del catálogo que estaba en un escandallo antiguo, rompería el histórico.
- **Propuesta MVP Imprescindible**:
  - **Archivado / Desactivación de Escandallos y Baja Lógica de Costes**: Permitir marcar escandallos como `Archivados` (congelados/ocultos) para quedarse con $\le 2$ escandallos `Activos` y recuperar la edición/exportación gratuita sin destruir la base de datos. Al reactivar Stripe, los escandallos archivados se restauran en 1 clic. Los costes con histórico usan baja lógica (`Archivado`) en lugar de borrado físico.

### Desafío 2: PVP Recomendado vs. PVP Comercial de Carta y Alertas de Erosión de Margen (`BR-CALC-FORMULAS-001` + `BR-RECALC-HISTORY-001`)
- **Fricción detectada**: La fórmula calcula un `PVP` exacto con decimales (ej. `43,78 €`). Pero ningún centro de estética pone `43,78 €` en su tarifa impresa o web: fija un **PVP Comercial** redondeado (ej. `45,00 €`). Cuando meses después sube el precio de un cosmético o la factura de la luz y se recalculan todos los escandallos en cascada, si solo recalculamos el PVP teórico, la dueña no se entera de que su tarifa actual de `45,00 €` acaba de perder rentabilidad.
- **Propuesta MVP Imprescindible**:
  - Persistir en cada servicio tanto el **`PVP Calculado (Teórico)`** como el **`PVP Comercial Fijado (Tarifa Real del Centro)`**, mostrando un **Semáforo de Rentabilidad (🟢 Margen OK / 🟡 Margen reducido / 🔴 Pérdida o bajo umbral mínimo)** cuando un recálculo automático en cascada erode el margen del PVP comercial actual.

### Desafío 3: La estructura de 30 columnas del Excel es inviable en pantallas móviles (`QR-UI-A11Y-001`)
- **Fricción detectada**: En `CalculadoraEscandallo.xlsx` (`Tabla5`), cada fila tiene 30 ranuras fijas (`Producto 1..30`, `Cantidad 1..30`, `Coste 1..30` = 90 columnas). En un móvil en vertical (`375px`) o incluso en horizontal, una tabla de 97 columnas rompe cualquier experiencia de usuario.
- **Propuesta MVP Imprescindible**:
  - **Editor de Receta Dinámico (Maestro-Detalle / Acordeón Táctil)** con líneas dinámicas (*"+ Añadir producto al protocolo"*), buscador instantáneo, sin límite arbitrario de 30 (o soportando $\ge 30$ dinámicamente) y barra visual de distribución del coste (`% Gastos Generales` vs `% Personal` vs `% Productos`).

### Desafío 4: Fricción de "Arranque en Frío" (Cold-Start) antes de probar los 2 Escandallos Gratuitos (`JRN-PRICING-DECISION-001`)
- **Fricción detectada**: Para calcular el primer escandallo gratuito, el usuario está obligado a introducir antes decenas de productos, sueldos, retenciones y gastos de local. En móvil, esto provoca abandono temprano antes de percibir el valor de la herramienta.
- **Propuesta MVP Imprescindible**:
  - **Importador de Excel (`.xlsx`) compatible con `CalculadoraEscandallo.xlsx` + Catálogo Semilla en 1 Clic**: Permitir al crear un centro nuevo elegir entre *(a) Empezar con datos de ejemplo editables (basados en el Excel)*, *(b) Importar su archivo `CalculadoraEscandallo.xlsx`*, o *(c) Empezar en blanco*.

### Desafío 5: Unidades de Dosificación en Cabina (Gotas / Pulsaciones / Gramos) y Mermas (`FR-COST-001`)
- **Fricción detectada**: En cabina estética los envases se compran en `ml` o `g`, pero en el protocolo se dosifica en **pulsaciones (`pump`)**, **gotas** o **gramos (`g`)**, y existe desperdicio/merma en guantes, espátulas y fondo de envase.
- **Propuesta MVP Imprescindible**:
  - Soportar unidades `ml`, `g`, `ud`, incluir una **calculadora rápida de equivalencia de dosis** en el formulario del escandallo (*1 gota = 0,05 ml, 1 pulsación = 2 ml*) y un porcentaje opcional de **Merma (`%`, por defecto `0%`)**.

### Desafío 6: Duplicación de Escandallos y Copia de Catálogo entre Centros Propios (`FR-TENANT-001`)
- **Fricción detectada**: Muchos tratamientos comparten el 80% de los pasos (ej. *Higiene Facial Básica* vs *Higiene Facial con Punta de Diamante*), y un propietario con 2 centros usa los mismos productos cosméticos en ambos.
- **Propuesta MVP Imprescindible**:
  - Botón **"Duplicar Escandallo"** (sujeto a la validación de cuota de Stripe) y opción **"Copiar catálogo de productos desde otro de mis centros"** al dar de alta un segundo centro.

---

## 3. Matriz de Frontera: MVP Estricto vs. Roadmap Posterior

| Funcionalidad / Mejora Propuesta | Clasificación | Justificación de Negocio y Esfuerzo |
| :--- | :--- | :--- |
| **1. Archivado/Activación de Escandallos (para cumplir límite $\le 2$ sin borrar datos) y Baja Lógica de Costes** | **🟢 MVP Core** | Imprescindible para que la regla de bloqueo tras cancelación sea operable sin corromper históricos ni destruir datos del cliente. |
| **2. Doble PVP (`PVP Calculado` vs `PVP Comercial Fijado`) + Semáforo de Erosión de Margen tras recálculo** | **🟢 MVP Core** | Es el verdadero valor ejecutivo del recálculo en cascada: avisar a la dueña qué tarifas de su carta han dejado de ser rentables cuando sube un coste. |
| **3. Constructor Dinámico de Receta + Previsualización de Impacto antes de cambiar un coste en uso** | **🟢 MVP Core** | Esencial para usabilidad móvil/tablet (`portrait`/`landscape`) y para evitar recálculos accidentales por errores de dedo. |
| **4. Carga de Plantilla Semilla (datos del Excel) / Importación `.xlsx` + Duplicar Escandallo/Catálogo** | **🟢 MVP Core** | Elimina la fricción de alta inicial y acelera la llegada al *paywall* del 3er escandallo en Stripe. |
| **5. Unidades adicionales (`g`), Equivalencias rápidas (gotas/pulsaciones) y `% Merma` configurable (defecto 0%)** | **🟢 MVP Core** | Coste de implementación mínimo (1 fórmula adicional) y altísimo ajuste al trabajo real en cabina estética. |
| **6. Creación de "Bonos / Packs de Sesiones" (ej. Bono 5 sesiones con 15% dto.)** | **🔵 Roadmap (v1.2)** | Muy habitual en estética, pero puede simularse inicialmente como un escandallo multiplicado o abordarse tras lanzar el MVP. |
| **7. Coste de Amortización de Aparatología por disparo/minuto (Láser, HIFU, Indiba)** | **🔵 Roadmap (v1.2)** | En el MVP puede meterse como un Producto en `ud`/`min` o como Gasto General; más adelante merece su propia pestaña de amortización de equipos. |
| **8. Escaneo de facturas de proveedores con IA (OCR) para actualizar precios de productos** | **🟣 Roadmap (v2.0)** | Gran diferenciador futuro, pero innecesario para validar el MVP transaccional. |

---

## 4. Open Questions para el Product Owner (`open-questions`)

1. **Tarifa de Suscripción en Stripe (`BR-FREEMIUM-QUOTA-001`)**: ¿Confirmas la tarifa propuesta de **14,90 €/mes** (y **149 €/año** con 2 meses gratis, más IVA) por centro de estética?
2. **Incorporación de Mejoras al MVP Core**: ¿Apruebas incorporar al alcance del MVP las **5 mejoras marcadas como 🟢 MVP Core** (Archivado para cumplir cuota sin pérdida de datos, `PVP Comercial Fijado` con semáforo de margen, Constructor dinámico con previsualización de impacto, Plantilla semilla/Importación `.xlsx` + Duplicación, y unidad `g` / `% merma`) para reflejarlas en los `FR-*` antes de pasar al modelado de amenazas?
