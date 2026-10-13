---
id: ACT-THREAT-TENANT-001
type: threat-actor
title: "Inquilino Malicioso o Competidor Infiltrado (Malicious Tenant)"
status: draft
version: "1.1.0"
schema-version: "1.0"
category: "internal-adversary"
skill-level: "medium"
motivation: "Espionaje industrial de costes/márgenes de otros centros de estética y evasión del pago de suscripción en Stripe"
stride-categories:
  - "Information Disclosure"
  - "Tampering"
  - "Elevation of Privilege"
supersedes: null
superseded-by: null
---

# Threat Actor: Inquilino Malicioso o Competidor Infiltrado (Malicious Tenant)

## 1. Adversary Profile & Motivation

Usuario registrado legítimamente en la plataforma (en plan gratuito o con una suscripción cancelada) que actúa con intención maliciosa desde una sesión autenticada en **Blazor Web App** (`CMP-WEB-UI-001`) para:
1. **Espionaje de Competencia**: Acceder mediante ataques IDOR/BOLA o contaminación de sesiones en el pool de conexiones `Npgsql` (`RSK-RLS-POOL-LEAK-004`) a los sueldos netos (`Tabla1`), precios de compra de cosméticos (`Tabla3`), gastos de local (`Tabla13`) o informes ejecutivos de otros centros de estética.
2. **Fraude de Suscripción (Paywall Bypass)**: Evadir el límite gratuito de 2 escandallos activos por centro (`BR-FREEMIUM-QUOTA-001`) o desbloquear la edición/exportación de escandallos archivados sin pagar la suscripción de Stripe (**14,90 EUR/mes** o **149,00 EUR/año**).

## 2. Attack Capabilities & Vectors

- **Manipulación de Estado en Circuitos Blazor Interactive Server y Parámetros SSR (`SEC-ENC-APP-CORE-001`)**: Alteración de parámetros `[Parameter]`, `[SupplyParameterFromForm]` o mensajes WebSocket en el circuito SignalR (`/_blazor`) para sustituir `center_id`, `source_center_id`, `escandallo_id` o `status` tras el renderizado inicial.
- **Explotación de Contaminación de Contexto RLS en Pool `Npgsql` (`SEC-ENC-DATA-OBS-001`)**: Disparo de consultas concurrentes buscando reutilizar una conexión física del pool de `Npgsql` que haya conservado el `app.current_center_id` de otro inquilino si no se empleó `SET LOCAL` transaccional (`RSK-RLS-POOL-LEAK-004`).
- **Condiciones de Carrera TOCTOU y Manipulación de Metadatos Checkout (`SEC-ENC-APP-CORE-001`)**: Ejecución de peticiones paralelas sobre creación, duplicación (`M4`) y desarchivado (`M1`) de escandallos, o intento de inyectar un `center_id` ajeno/múltiple al iniciar `CreateStripeCheckoutSessionAsync`.

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Modelado inicial del actor de amenaza inquilino malicioso / competidor. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Incorporación de vectores técnicos sobre circuitos Blazor SignalR y pool de conexiones Npgsql/RLS. |
