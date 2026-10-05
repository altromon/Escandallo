---
id: ARCH-RISK-001
type: risks-and-technical-debt
title: "11. Architecture Risks and Technical Debt"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 11
naf-perspective: "Risk & Technical Debt"
risks:
  - RSK-SINGLE-VPS-001
  - RSK-OBS-DISK-002
  - RSK-STRIPE-SYNC-003
supersedes: null
superseded-by: null
---

# 11. Risks and Technical Debt (arc42 Sec. 11 / NAF Risk & Debt)

## 1. Architectural Risks Matrix (`RSK-*`)

| Risk ID | Risk Description | Probability | Impact | Mitigation Strategy |
| :--- | :--- | :---: | :---: | :--- |
| `RSK-SINGLE-VPS-001` | Indisponibilidad temporal del nodo VPS único (`RES-VPS-EU-001`) ante fallo físico de hipervisor en el proveedor cloud | Low | Medium | Backups incrementales cifrados cada hora/día hacia Cloudflare R2 (`RES-STORAGE-R2-001`) + snapshots de volumen que permiten restaurar el stack Docker Compose en un nodo nuevo en $< 10\text{ minutos}$ (RTO $< 15\text{ min}$, RPO $< 1\text{ h}$). |
| `RSK-OBS-DISK-002` | Llenado de disco NVMe si el mantenedor olvida el nivel de log en `DEBUG` durante varios días | Medium | Medium | Expiración automática (TTL de `60 minutos`) del modo `DEBUG` con reversión automática a `INFO`, más rotación circular en la partición `telemetry_events` (retención máxima acotada a `2 GB`). |
| `RSK-STRIPE-SYNC-003` | Desincronización temporal si un webhook de Stripe falla durante un mantenimiento del servidor | Low | High | Política nativa de reintentos exponenciales de Stripe (hasta 72h) + tabla de idempotencia `stripe_webhook_events` + endpoint de reconciliación bajo demanda al abrir el panel de facturación. |

---

## 2. Technical Debt and Pragmatic MVP Trade-offs Log

| Debt Item | Affected Component | Trade-off Justification (Simplicity & ROI) | Payoff / Evolution Trigger |
| :--- | :--- | :--- | :--- |
| **Despliegue Mononodo (Single-Node Docker Compose)** | `CMP-ESCANDALLO-SYS-001` | Mantiene el TCO en `~5,50–9,50 EUR/mes` para alcanzar rentabilidad desde la primera suscripción (`ADR-001`). | Migrar PostgreSQL a instancia gestionada HA cuando la plataforma supere los **500 centros de pago** ($>7.400\text{ EUR/mes}$ MRR). |
| **Generación Síncrona en Streaming de PDF/Excel** | `CMP-DOC-IO-001` | Evita desplegar Redis + colas de workers en segundo plano para informes que tardan $< 300\text{ ms}$ con `pdfkit`/`exceljs`. | Extraer a un pool de Worker Threads dedicados si el volumen de informes concurrentes supera las 50 peticiones/minuto. |

---

## 3. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Definición inicial de matriz de riesgos y compromisos pragmáticos del MVP | ARCH-INIT-001 |
