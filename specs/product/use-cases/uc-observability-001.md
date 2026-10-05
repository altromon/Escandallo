---
id: UC-OBSERVABILITY-001
type: use-case
title: "Extracción de Logs, Trazas y Métricas del Sistema por Usuario Mantenedor"
status: draft
version: "1.0.0"
schema-version: "1.0"
primary-actor: ACT-MAINTAINER-001
supersedes: null
superseded-by: null
---

# Use Case: Extracción de Logs, Trazas y Métricas del Sistema por Usuario Mantenedor

## 1. Intent and Outcome

As a `ACT-MAINTAINER-001` I want to configure log granularity, export structured logs, traces, and metrics directly from the administration web interface, and integrate telemetry with external observability stacks in addition to all standard user capabilities To monitor system health, diagnose incidents, and detect cyberattacks.

## 2. Preconditions

- El usuario está autenticado con el rol `ACT-MAINTAINER-001` verificado en servidor.

## 3. Main Flow

1. El usuario mantenedor accede al panel de Observabilidad y Mantenimiento del sistema.
2. El usuario mantenedor puede consultar o ajustar en caliente el nivel de **granularidad de logs estructurados** (`DEBUG`, `INFO`, `WARN`, `ERROR`, `SECURITY`), los cuales se emiten en formato estándar (JSON / OpenTelemetry Log Data Model).
3. El usuario selecciona el tipo de telemetría a consultar, exportar o enviar al stack externo (OpenTelemetry / Prometheus / Grafana):
   - **Logs**: Registros estructurados en formato estándar con granularidad configurable de aplicación, errores y eventos de seguridad (intentos de acceso no autorizado, fallos de autenticación, webhooks de Stripe).
   - **Trazas**: Trazas de ejecución (formato W3C Trace Context / OTLP) de peticiones HTTP, recálculos en cascada de escandallos y llamadas a base de datos/Stripe.
   - **Métricas**: Latencia de peticiones, uso de CPU/memoria, número de cálculos ejecutados, centros activos y tasa de errores.
4. El usuario filtra por rango temporal, nivel de severidad o tipo de evento y descarga el paquete directamente desde la interfaz web (`JSON`/`CSV`/`OTLP`) o verifica su exportación continua hacia el colector externo.

## 4. Alternative and Exception Flows

- **E1 – Intento de acceso por `ACT-ESTHETIC-USER-001`**: El servidor deniega el acceso con código `403 Forbidden` y registra un evento de auditoría de seguridad con severidad `SECURITY`.

## 5. Postconditions

- El mantenedor obtiene los logs estructurados, trazas y métricas tanto por descarga directa como mediante integración estándar, con el nivel de granularidad configurado.

## 6. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del caso de uso de observabilidad para el mantenedor. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Soporte dual (descarga web + stack externo) y logs estructurados estándar con granularidad configurable. |
