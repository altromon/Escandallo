---
id: SEC-ENC-EDGE-WAF-001
type: security-enclave
title: "Perímetro Edge WAF, TLS Termination y Rate Limiter"
status: draft
version: "1.0.0"
schema-version: "1.0"
trust-zone: "public-edge-dmz"
supersedes: null
superseded-by: null
---

# Security Enclave: Perímetro Edge WAF, TLS Termination y Rate Limiter

## 1. Enclave Description & Trust Boundary

Frontera perimetral pública compuesta por el WAF/CDN (ej. Cloudflare) y el proxy inverso de entrada al servidor. Absorbe ataques volumétricos DDoS, aplica limitación de tasa (*rate limiting*) por IP/usuario, bloquea cargas útiles sobredimensionadas y garantiza cifrado TLS 1.3 antes de enrutar tráfico hacia `SEC-ENC-APP-CORE-001`.

## 2. Security Controls Enforced

- Terminación TLS 1.3 y cabeceras estrictas (`HSTS`, `CSP`, `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`).
- Rate limiting diferenciado para endpoints de autenticación, recálculo en cascada y generación de informes PDF/Excel (`SEC-REQ-DOS-001`).
- Restricción de tamaño máximo de subida en importación de ficheros `.xlsx` (`<= 2 MB`).

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Definición inicial del enclave perimetral Edge/WAF. |
