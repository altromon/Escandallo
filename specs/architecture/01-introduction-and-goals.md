---
id: ARCH-INTRO-001
type: architecture-introduction
title: "01. Introducción y Objetivos del Sistema (arc42 Sec. 1 / NAF Enterprise)"
status: proposed
version: "1.1.0"
schema-version: "1.0"
arc42-section: 1
naf-perspective: "Enterprise & Capability"
cites-product-actors:
  - ACT-ESTHETIC-USER-001
  - ACT-MAINTAINER-001
  - ACT-STRIPE-001
cites-quality-goals:
  - QR-UI-A11Y-001
  - QR-SEC-001
  - SEC-REQ-TENANT-001
  - SEC-REQ-BILLING-001
  - SEC-REQ-XLSX-001
  - SEC-REQ-DOS-001
  - SEC-REQ-OBS-001
cites-use-cases:
  - UC-COST-CATALOG-001
  - UC-ESCANDALLO-CALC-001
  - UC-EXEC-REPORT-001
  - UC-SUBSCRIPTION-001
  - UC-OBSERVABILITY-001
supersedes: null
superseded-by: null
---

# 01. Introducción y Objetivos del Sistema (arc42 Sec. 1 / NAF Enterprise)

## 1. Visión del Sistema y Resumen Ejecutivo

### 1.1 Declaración de Misión
> *La plataforma SaaS **Escandallo de Tratamientos Estéticos** (`CMP-ESCANDALLO-SYS-001`) proporciona a propietarias, gerentes y directoras técnicas de centros de estética y bienestar (`ACT-ESTHETIC-USER-001`) un motor financiero determinista en tiempo real para calcular costes por sesión, detectar erosiones de margen y fijar precios rentables en segundos, operando sobre una arquitectura nativa **.NET 10 LTS + Blazor Web App** ultra-rentable, accesible (WCAG 2.1 AA) y protegida bajo aislamiento criptográfico multi-tenant (Zero Trust).*

### 1.2 Impulsores de Negocio y Valor Entregado
1. **Eliminación de la Ceguera Financiera**: Sustituye hojas de cálculo frágiles por un motor aritmético exacto de 128 bits (`System.Decimal`) que computa instantáneamente el coste de consumibles con merma, amortización de aparatología, mano de obra por minuto, gastos fijos y comisiones de pasarela (`BR-CALC-FORMULAS-001`).
2. **Protección Activa del Margen ante Inflación**: Cuando un proveedor incrementa el precio de un insumo en el catálogo (`UC-COST-CATALOG-001`), el motor previsualiza el impacto (M3) y recalcula síncronamente en cascada todos los tratamientos dependientes (`BR-RECALC-HISTORY-001`), alertando con el **Semáforo de Erosión de Margen** (`🟢` Óptimo, `🟡` En Riesgo, `🔴` Crítico).
3. **Sostenibilidad Económica SaaS (Break-Even Inmediato)**: Diseñada como un **Monolito Modular Hexagonal en .NET 10 LTS** desplegable en un único nodo Cloud VPS europeo con **PostgreSQL 16** y **Cloudflare Edge WAF** (`ADR-001-COST-EFFECTIVE-DEPLOYMENT`), manteniendo un coste operativo fijo inferior a **12 EUR/mes** y alcanzando rentabilidad neta desde el primer cliente de pago (`14,90 EUR/mes`).

---

## 2. Objetivos de Calidad Prioritarios (Citas Canónicas ProductShape y Seguridad)

| Prioridad | Objetivo de Calidad | ID de Requisito | Racional Arquitectónico y Mecanismo de Garantía |
| :---: | :--- | :--- | :--- |
| **1** | **Aislamiento Multi-Tenant Zero-Trust e Integridad Criptográfica** | `QR-SEC-001`, `SEC-REQ-TENANT-001`, `SEC-REQ-BILLING-001` | Revalidación continua de `AuthenticationStateProvider` en servidor en componentes Blazor (`ITenantIsolationGuard`), aislamiento en pool `Npgsql` con rol `NOSUPERUSER NOBYPASSRLS`, `SET LOCAL app.current_center_id` y `FORCE ROW LEVEL SECURITY` *fail-closed*, bloqueo pesimista `SELECT ... FOR UPDATE` para cuota Freemium (`BR-FREEMIUM-QUOTA-001`), vinculación en servidor de `metadata.center_id` y verificación `HMAC-SHA256` sobre cuerpo crudo ($\le 64\text{ KB}$) con idempotencia monótona en Stripe (`ADR-003-MULTITENANT-RLS-AND-ATOMIC-QUOTA`). |
| **2** | **Exactitud Financiera Determinista y Latencia Sub-Segundo** | `FR-CALC-001`, `FR-HIST-001`, `SEC-REQ-DOS-001`, `SEC-REQ-XLSX-001` | Aritmética decimal pura de 128 bits (`System.Decimal` con `MidpointRounding.AwayFromZero`) protegida con guardas de rango anti-`OverflowException` y columnas `NUMERIC(18,6)` en base de datos, ejecutando simulaciones ($\le 100$ líneas/escandallo) en `< 100 ms` y recálculos transaccionales en cascada (`<= 200` escandallos, máx. `30 mutaciones/minuto`) en `< 2000 ms` (`ADR-004-EXACT-DECIMAL-AND-CASCADE-ENGINE`). |
| **3** | **Experiencia Responsiva Mobile-First y Accesibilidad WCAG 2.1 AA** | `QR-UI-A11Y-001` | Interfaz unificada en **Blazor Web App** (`CMP-WEB-UI-001`) con renderizado híbrido (SSR + Interactive Server acotado a $\le 5$ circuitos/usuario y mensajes $\le 32\text{ KB}$), componentes semánticos accesibles por teclado, contraste $\ge 4.5:1$, `aria-live="polite"` para el semáforo de margen y diseño adaptativo sin scroll horizontal desde `360px` hasta `4K`. |
| **4** | **Procesamiento Seguro de Documentos y Resiliencia Anti-DoS** | `SEC-REQ-XLSX-001`, `SEC-REQ-DOS-001`, `FR-REPORT-001` | Inspección `ZipArchive` con stream contador real `BoundedReadStream` ($\le 10\text{ MB}$ descomprimidos sobre `RecyclableMemoryStream`, inmune a cabeceras ZIP falsificadas), desactivación total de DTD/XXE (`DtdProcessing.Prohibit`), neutralización de fórmulas DDE (`TrimStart` + `'` + `XLDataType.Text`) y exportación acotada con `SemaphoreSlim(2, 2)` y `10 exp/min` usando librerías 100% `MIT` (`ClosedXML` + `PDFsharp`) (`ADR-005-DUAL-OBSERVABILITY-AND-SAFE-DOCUMENTS`). |
| **5** | **Observabilidad Dual en Caliente con Redacción Garantizada (`[REDACTED]`)** | `FR-OBS-001`, `SEC-REQ-OBS-001` | Pipeline estructurado con `Serilog` + `OpenTelemetry .NET` (`CMP-OBS-TELEMETRY-001`), RBAC estricto `[Authorize(Roles = "Maintainer")]` anti-Overposting, conmutación en caliente (`LoggingLevelSwitch`), cota `ToMaximumDepth(3)`, filtro inmutable `[REDACTED]` sobre propiedades y `Exception.Message` (`Password=...`, `whsec_`), y canal `SECURITY` aislado contra evicción. |

---

## 3. Matriz de Partes Interesadas y Actores Primarios

| Rol / Parte Interesada | ID de Actor | Expectativas Arquitectónicas Clave |
| :--- | :--- | :--- |
| **Propietaria / Directora de Centro de Estética** | `ACT-ESTHETIC-USER-001` | Interfaz Blazor intuitiva desde móvil/tablet en cabina o PC en recepción, simulación de PVP en tiempo real (`< 100 ms`), importación sencilla de tarifas Excel y exportación ejecutiva limpia (`.pdf`/`.xlsx`). |
| **Administrador / Mantenedor de Plataforma (DevSecOps)** | `ACT-MAINTAINER-001` | Despliegue ligero en contenedores Docker con mínimo mantenimiento, telemetría estructurada OpenTelemetry, cambio de nivel de log en caliente (`DEBUG`/`SECURITY`) y exportación de auditoría sin exposición de PII. |
| **Pasarela de Pagos Stripe (Sistema Externo)** | `ACT-STRIPE-001` | Endpoint receptor de webhooks TLS 1.3 de baja latencia, validación criptográfica estricta `Stripe-Signature` (`HMAC-SHA256`) y respuesta idempotente determinista ante reintentos de red. |

---

## 4. Historial de Revisiones y Control de Versiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-08 | `agent-system-architect` | Definición inicial de introducción y objetivos de arquitectura (.NET 10 LTS + Blazor) | `chg-001-escandallo-mvp` |
| **1.1.0** | 2026-10-08 | `agent-system-architect` | Integración de mecanismos de garantía técnica para los requisitos `SEC-REQ-*` v1.1.0 | `chg-001-escandallo-mvp` |
