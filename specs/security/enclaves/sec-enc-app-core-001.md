---
id: SEC-ENC-APP-CORE-001
type: security-enclave
title: "Enclave de Servidor de Aplicación Multi-Tenant, Motor de Escandallos y Pagos"
status: draft
version: "1.0.0"
schema-version: "1.0"
trust-zone: "application-server-runtime"
supersedes: null
superseded-by: null
---

# Security Enclave: Enclave de Servidor de Aplicación Multi-Tenant, Motor de Escandallos y Pagos

## 1. Enclave Description & Trust Boundary

Zona de ejecución privada del servidor de aplicación donde se evalúa la autorización por centro de estética (`tenant`), se procesan los archivos `.xlsx` en memoria aislada sin evaluación de entidades externas, se ejecutan las transacciones de cuota/archivado y se verifica criptográficamente la firma HMAC de los webhooks de `ACT-STRIPE-001`.

## 2. Security Controls Enforced

- Verificación obligatoria de propiedad del centro de estética (`owner_id` + `center_id`) en cada lectura, escritura, copia de catálogo y exportación (`SEC-REQ-TENANT-001`).
- Verificación criptográfica HMAC-SHA256 de `Stripe-Signature` con ventana anti-replay de 300 segundos y bloqueo transaccional de cuota de escandallos activos (`SEC-REQ-BILLING-001`).
- Deserialización segura de `.xlsx` sin resolución XXE, control de ratio de compresión anti-Zip-Bomb y neutralización de inyección de fórmulas (`SEC-REQ-XLSX-001`).

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Definición inicial del enclave de servidor de aplicación multi-tenant. |
