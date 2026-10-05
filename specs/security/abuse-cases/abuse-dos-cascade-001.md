---
id: ABUSE-DOS-CASCADE-001
type: abuse-case
title: "Denegación de Servicio Asimétrica por Tormenta de Recálculos en Cascada y Renderizado PDF"
status: draft
version: "1.0.0"
schema-version: "1.0"
primary-threat-actor: ACT-THREAT-EXT-001
targets-use-cases:
  - UC-COST-CATALOG-001
  - UC-ESCANDALLO-CALC-001
  - UC-EXEC-REPORT-001
stride-category: "Denial of Service"
severity: "HIGH"
likelihood: "HIGH"
mitigated-by:
  - SEC-REQ-DOS-001
supersedes: null
superseded-by: null
---

# Abuse Case: Denegación de Servicio Asimétrica por Tormenta de Recálculos en Cascada y Renderizado PDF

## 1. Attack Scenario & Preconditions

Dado que el sistema debe desplegarse en una infraestructura rentable y ajustada en recursos, un atacante externo (`ACT-THREAT-EXT-001`) busca agotar la CPU, memoria y almacenamiento de base de datos mediante operaciones asimétricas (bajo coste para el atacante, alto coste computacional para el servidor).

## 2. Attack Flow (Step-by-Step)

1. El atacante automatiza cientos de modificaciones por minuto sobre un coste base vinculado a múltiples escandallos activos (`UC-COST-CATALOG-001`), forzando en cada petición una transacción pesada de recálculo en cascada y la inserción masiva de snapshots en el histórico (`BR-RECALC-HISTORY-001`).
2. En paralelo, solicita de forma concurrente la generación y descarga de decenas de informes ejecutivos en formato `.pdf` y `.xlsx` (`UC-EXEC-REPORT-001`), saturando los workers de renderizado y bloqueando el bucle de eventos o el pool de conexiones para el resto de centros de estética.

## 3. Business & Security Impact

- **Caída Global del Servicio Multi-Tenant (Noisy Neighbor / DoS)**: Indisponibilidad de la aplicación web para todos los clientes legítimos y crecimiento descontrolado del almacenamiento por inflación artificial del histórico de versiones.

## 4. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del caso de abuso de DoS asimétrico. |
