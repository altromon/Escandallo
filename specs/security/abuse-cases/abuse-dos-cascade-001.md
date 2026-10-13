---
id: ABUSE-DOS-CASCADE-001
type: abuse-case
title: "Denegación de Servicio Asimétrica por Tormenta de Recálculos en Cascada y Renderizado PDF"
status: draft
version: "1.1.0"
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

Dado que la plataforma se despliega en un nodo Cloud VPS europeo con límite de `2 GB` de RAM para el contenedor **.NET 10 LTS + Blazor Web App** (`CON-TECH-FINOPS-003`, `ADR-001-COST-EFFECTIVE-DEPLOYMENT`), un atacante (`ACT-THREAT-EXT-001`) busca agotar la memoria del proceso `Kestrel`, el *Large Object Heap (LOH)* y las conexiones a PostgreSQL 16 mediante operaciones técnicas asimétricas en `SEC-ENC-EDGE-WAF-001` y `SEC-ENC-APP-CORE-001`.

## 2. Attack Flow (Step-by-Step)

1. **Vector A (Tormenta de Cascada e Inflación de Snapshots en `CMP-COST-ENGINE-001`)**: El atacante automatiza decenas de modificaciones por minuto sobre un coste base vinculado a hasta `200` escandallos activos (`UC-COST-CATALOG-001`), forzando transacciones pesadas de recálculo síncrono en `System.Decimal` y escrituras en `escandallo_snapshots` (`BR-RECALC-HISTORY-001`).
2. **Vector B (Agotamiento de LOH por Renderizado Concurrente `PDFsharp`/`ClosedXML` en `CMP-DOC-IO-001`)**: En paralelo, dispara exportaciones concurrentes de informes ejecutivos `.pdf` y `.xlsx` (`UC-EXEC-REPORT-001`) para provocar asignaciones $> 85\text{ KB}$ en el *Large Object Heap (LOH)* de .NET 10 (`RSK-DOC-MEM-SPIKE-003`).
3. **Vector C (Inundación de Circuitos Blazor Interactive Server `/_blazor` y Spoofing de `X-Forwarded-For`)**: El atacante abre cientos de circuitos WebSocket SignalR (`EscandalloBuilderPage.razor`) dejando conexiones zombis sin cerrar o enviando lotes de eventos rápidos (`RSK-BLAZOR-CIRCUIT-001`), mientras falsifica cabeceras `X-Forwarded-For` / `CF-Connecting-IP` o ataca directamente la IP del VPS para evadir el `RateLimiter` de `CMP-IAM-TENANT-001`.

## 3. Business & Security Impact

- **Caída Global del Contenedor .NET 10 por `OutOfMemoryException` (OOM Kill)**: Indisponibilidad total del monolito modular para todos los centros de estética legítimos y degradación de latencia por encima de los `2000 ms`.

## 4. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del caso de abuso de DoS asimétrico. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Incorporación de vectores técnicos sobre circuitos Blazor SignalR, LOH en .NET 10 y spoofing de cabeceras IP. |
