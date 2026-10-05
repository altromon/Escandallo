---
id: JRN-PRICING-DECISION-001
type: journey
title: "Recorrido de Configuración de Costes, Escandallo y Decisión de PVP por Centro de Estética"
status: draft
version: "1.0.0"
schema-version: "1.0"
persona: ACT-ESTHETIC-USER-001
related-use-cases:
  - UC-COST-CATALOG-001
  - UC-ESCANDALLO-CALC-001
  - UC-EXEC-REPORT-001
  - UC-SUBSCRIPTION-001
supersedes: null
superseded-by: null
---

# Journey: Recorrido de Configuración de Costes, Escandallo y Decisión de PVP por Centro de Estética

## 1. Overview

Describe el recorrido de negocio extremo a extremo que realiza `ACT-ESTHETIC-USER-001` desde el alta de su centro de estética y sus costes base hasta la determinación del PVP óptimo, la suscripción ilimitada y la descarga de informes ejecutivos.

## 2. Journey Stages

1. **Alta de Centro y Onboarding Rápido (`UC-COST-CATALOG-001`)**: El usuario crea o selecciona su centro de estética y puede inicializar su catálogo en 1 clic con la **Plantilla Semilla Editable** (basada en `CalculadoraEscandallo.xlsx`), **importar su archivo `.xlsx`** o **copiar el catálogo de productos desde otro de sus centros**.
2. **Configuración de Costes y Unidades de Cabina (`UC-COST-CATALOG-001`)**: Ajusta productos cosméticos (`ml`, `g` o `ud`, con `% Merma` opcional), perfiles de personal (`Neto`, `Retenciones`, `Coeficiente Empresa`, `Minutos/mes`), gastos generales mensuales y parámetros (`IVA`, `% Beneficio`), con baja lógica (`Archivado`) para costes con histórico.
3. **Constructor Dinámico de Escandallos y Fijación de PVP Comercial (`UC-ESCANDALLO-CALC-001`)**: Crea o duplica sus primeros tratamientos (hasta 2 escandallos activos gratuitos por centro) usando el constructor dinámico de productos con asistente de dosis (gotas/pulsaciones), compara el `PVP Calculado` con su `PVP Comercial Fijado` y simula precios en directo o a la inversa.
4. **Suscripción Ilimitada y Gestión de Archivado (`UC-SUBSCRIPTION-001`)**: Al necesitar $> 2$ escandallos activos, contrata la suscripción en Stripe (**14,90 €/mes** o **149,00 €/año** por centro). Si en algún momento cancela, puede archivar escandallos hasta dejar $\le 2$ activos sin perder sus datos históricos.
5. **Previsualización de Impacto, Recálculo Histórico y Semáforo de Margen (`UC-ESCANDALLO-CALC-001`)**: Al editar un coste en uso, el sistema advierte a cuántos escandallos afectará, guarda el histórico, recalcula en cascada y alerta con el **Semáforo de Erosión de Margen (`🟢/🟡/🔴`)** si algún `PVP Comercial Fijado` ha perdido rentabilidad.
6. **Extracción de Informes Ejecutivos en PDF y Excel (`UC-EXEC-REPORT-001`)**: Descarga informes ejecutivos en `.pdf` o `.xlsx` con el desglose completo de costes, beneficios y alertas de margen.

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Creación inicial del recorrido de usuario. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de mejoras MVP M1 a M5 y tarifa confirmada de Stripe. |
