---
id: SEC-ENC-APP-CORE-001
type: security-enclave
title: "Enclave de Servidor de Aplicación Multi-Tenant, Motor de Escandallos y Pagos"
status: draft
version: "1.1.0"
schema-version: "1.0"
trust-zone: "application-server-runtime"
supersedes: null
superseded-by: null
---

# Security Enclave: Enclave de Servidor de Aplicación Multi-Tenant, Motor de Escandallos y Pagos

## 1. Enclave Description & Trust Boundary

Zona de ejecución privada del monolito modular **.NET 10 LTS + ASP.NET Core Blazor Web App** (`CMP-ESCANDALLO-SYS-001`) sobre contenedor endurecido `mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled` (`UID 1654`, *read-only rootfs*, red interna `net-app-core` sin exposición directa al host). En este enclave se hospedan `CMP-WEB-UI-001`, `CMP-IAM-TENANT-001`, `CMP-COST-ENGINE-001`, `CMP-DOC-IO-001` y `CMP-BILLING-STRIPE-001`.

## 2. Security Controls Enforced

- **Autorización Continua en Blazor SSR e Interactive Server (`SEC-REQ-TENANT-001`)**: `ITenantIsolationGuard` revalida la membresía activa (`userId` extraído del `AuthenticationStateProvider` en servidor + `centerId`) en cada mutación, invocación de circuito SignalR y copia dual `M4`, impidiendo la manipulación de `[Parameter]` o `[SupplyParameterFromForm]` entre centros (`CMP-WEB-UI-001`, `CMP-IAM-TENANT-001`).
- **Acotación de Memoria de Circuitos Blazor y Renderizado Documental (`SEC-REQ-DOS-001`)**: Configuración estricta de `CircuitOptions` (`DisconnectedCircuitRetentionPeriod = 3 min`, `MaxBufferedUnacknowledgedRenderBatches = 10`, `MaximumReceiveMessageSize = 32 KB`) y semáforo `SemaphoreSlim(2, 2)` con `RecyclableMemoryStream` en `ClosedXML` / `PDFsharp` para evitar fragmentación del *Large Object Heap (LOH)* dentro del límite de `2 GB` de RAM (`RSK-BLAZOR-CIRCUIT-001`, `RSK-DOC-MEM-SPIKE-003`).
- **Verificación Criptográfica sobre Stream Crudo de Stripe y Bloqueo Pesimista (`SEC-REQ-BILLING-001`)**: Lectura acotada del cuerpo crudo (`<= 64 KB`) para `Stripe.EventUtility.ConstructEvent` (`HMAC-SHA256`, ventana $\le 300\text{ s}$), vinculación en servidor de `metadata.center_id` al crear sesiones Checkout y bloqueo `SELECT ... FOR UPDATE` sobre `centers` (`CMP-BILLING-STRIPE-001`).
- **Inspección de Stream Contado Anti-Zip-Bomb, Anti-XXE y Guardas `System.Decimal` (`SEC-REQ-XLSX-001`)**: Conteo real de bytes descomprimidos ($\le 10\text{ MB}$, inmune a cabeceras ZIP falsificadas), `XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }`, neutralización DDE (`'`) en `ClosedXML` y cotas de dominio en `System.Decimal` (128 bits) contra `OverflowException` (`CMP-DOC-IO-001`, `CMP-COST-ENGINE-001`).

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Definición inicial del enclave de servidor de aplicación multi-tenant. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Auditoría técnica .NET 10 LTS + Blazor: blindaje de circuitos SignalR, revalidación en componentes Razor, stream contado en ZipArchive/ClosedXML y protección LOH. |
