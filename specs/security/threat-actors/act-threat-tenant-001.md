---
id: ACT-THREAT-TENANT-001
type: threat-actor
title: "Inquilino Malicioso o Competidor Infiltrado (Malicious Tenant)"
status: draft
version: "1.0.0"
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

Usuario registrado legítimamente en la plataforma (en plan gratuito o con una suscripción cancelada) que actúa con intención maliciosa para:
1. **Espionaje de Competencia**: Acceder mediante ataques IDOR/BOLA a los sueldos netos (`Tabla1`), precios de compra de cosméticos (`Tabla3`), gastos de local (`Tabla13`) o informes ejecutivos de otros centros de estética.
2. **Fraude de Suscripción (Paywall Bypass)**: Evadir el límite gratuito de 2 escandallos activos por centro o desbloquear la edición/exportación de escandallos archivados sin pagar la suscripción de Stripe (**14,90 EUR/mes** o **149,00 EUR/año**).

## 2. Attack Capabilities & Vectors

- Manipulación de parámetros HTTP/JSON (`center_id`, `source_center_id`, `escandallo_id`, `status`).
- Ejecución de peticiones paralelas (*race conditions*) sobre endpoints de creación, duplicación y desarchivado de escandallos.

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Modelado inicial del actor de amenaza inquilino malicioso / competidor. |
