---
id: SEC-ENC-EDGE-WAF-001
type: security-enclave
title: "Perímetro Edge WAF, TLS Termination y Rate Limiter"
status: draft
version: "1.1.0"
schema-version: "1.0"
trust-zone: "public-edge-dmz"
supersedes: null
superseded-by: null
---

# Security Enclave: Perímetro Edge WAF, TLS Termination y Rate Limiter

## 1. Enclave Description & Trust Boundary

Frontera perimetral pública compuesta por **Cloudflare Edge WAF** (`RES-EDGE-CF-001`) y el proxy inverso **Caddy** en el nodo VPS europeo (`RES-VPS-EU-001`, `ADR-001-COST-EFFECTIVE-DEPLOYMENT`). Absorbe ataques volumétricos DDoS, verifica mTLS (*Authenticated Origin Pulls*) para impedir el *bypass* directo a la IP de origen, aplica limitación de tasa (*rate limiting*) sobre peticiones HTTP y conexiones WebSocket (`WSS /_blazor`) de Blazor Interactive Server, bloquea cargas útiles sobredimensionadas y garantiza cifrado TLS 1.3 antes de enrutar tráfico por la red interna `net-app-core` hacia `SEC-ENC-APP-CORE-001`.

## 2. Security Controls Enforced

- **Aislamiento de Origen mTLS y Anti-Spoofing de IP**: Firewall de host (`ufw`) cerrado a Internet salvo `:443` desde rangos oficiales de Cloudflare con verificación obligatoria de certificado cliente mTLS (*Strict Origin Pulls*) en Caddy, y configuración de `ForwardedHeadersMiddleware` en ASP.NET Core 10 confiando únicamente en el proxy interno (`KnownProxies`) para evitar falsificación de `X-Forwarded-For` / `CF-Connecting-IP` (`SEC-REQ-DOS-001`).
- **Terminación TLS 1.3 y Cabeceras Defensivas para Blazor**: Emisión de `Strict-Transport-Security`, `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY` y `Content-Security-Policy` adaptada a Blazor SSR e Interactive Server (`wss:` restringido al propio origen, sin `unsafe-eval`).
- **Rate Limiting HTTP y Control de Circuitos SignalR (`/_blazor`)**: Estrangulamiento diferenciado para autenticación, recálculo en cascada, generación documental PDF/Excel y límite de conexiones WebSocket simultáneas por inquilino para proteger el techo de `2 GB` de RAM del contenedor .NET 10 (`SEC-REQ-DOS-001`, `RSK-BLAZOR-CIRCUIT-001`).
- **Restricción Perimetral de Subida `.xlsx`**: Bloqueo en proxy de cuerpos `multipart/form-data` superiores a `2 MB` (`2_097_152` bytes) antes de alcanzar `CMP-DOC-IO-001` (`SEC-REQ-XLSX-001`).

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Definición inicial del enclave perimetral Edge/WAF. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Auditoría técnica .NET 10 LTS + Blazor: mTLS Origin Pulls en Caddy, protección anti-spoofing X-Forwarded-For y control perimetral de circuitos WSS `/_blazor`. |
