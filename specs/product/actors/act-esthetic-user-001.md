---
id: ACT-ESTHETIC-USER-001
type: actor
title: "Usuario Normal (Gestor de Centro de Estética)"
status: draft
version: "1.0.0"
schema-version: "1.0"
supersedes: null
superseded-by: null
---

# Actor: Usuario Normal (Gestor de Centro de Estética)

## 1. Profile and Role Description

Propietario, director o gestor de uno o varios centros de estética o clínicas médico-estéticas que utiliza la aplicación web desde PC, tablet o móvil para controlar los costes reales de sus tratamientos y fijar precios de venta al público (PVP) rentables.

## 2. Responsibilities and Capabilities

- Dar de alta y gestionar uno o varios centros de estética bajo su cuenta, con aislamiento estricto respecto a otros clientes.
- Registrar y actualizar el catálogo de costes de cada centro: Productos (`ml`/`ud`), categorías de Personal (sueldo neto, retenciones, coste empresa), Gastos Generales (`Local`, `Consumos`, `Consumibles`) y parámetros globales (`IVA`, `% Beneficio`).
- Configurar servicios/tratamientos y calcular su escandallo, beneficio y PVP (hasta 2 cálculos gratuitos por usuario y centro de estética, o ilimitados mediante suscripción activa en Stripe).
- Consultar el histórico de costes y escandallos recalculados automáticamente cuando varía cualquier insumo.
- Extraer informes ejecutivos con el desglose detallado de costes y beneficios por servicio.

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del actor Usuario Normal. |
