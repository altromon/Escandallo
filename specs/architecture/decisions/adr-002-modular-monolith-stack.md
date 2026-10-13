---
id: ADR-002-MODULAR-MONOLITH-STACK
type: architecture-decision-record
title: "Adopción de Monolito Modular Hexagonal en .NET 10 LTS con UI Unificada en Blazor Web App"
status: proposed
version: "1.1.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
  - "Product Owner"
decision-date: "2026-10-08"
affects-components:
  - CMP-ESCANDALLO-SYS-001
  - CMP-WEB-UI-001
  - CMP-COST-ENGINE-001
  - CMP-IAM-TENANT-001
supersedes: null
superseded-by: null
---

# ADR-002: Adopción de Monolito Modular Hexagonal en .NET 10 LTS con UI Unificada en Blazor Web App

## 1. Contexto y Planteamiento del Problema

El sistema requiere una interfaz web altamente interactiva y accesible (`QR-UI-A11Y-001`) para simular el Semáforo de Erosión de Margen (`🟢/🟡/🔴`) y previsualizar recálculos en cascada (`FR-CALC-001`, `FR-HIST-001`), respaldada por un motor financiero riguroso sin desviaciones de coma flotante (`BR-CALC-FORMULAS-001`). Además, todas las dependencias deben cumplir estrictamente `license-policy.yaml` (`MIT`, `Apache-2.0`, `BSD`) y la superficie de ataque propia de **Blazor SSR / Interactive Server** (manipulación de parámetros en circuitos SignalR `/_blazor`, agotamiento de memoria por circuitos abiertos y *Mass Assignment* en Identity) debe quedar mitigada por diseño (`SEC-REQ-TENANT-001`, `SEC-REQ-DOS-001`, `SEC-REQ-OBS-001`).

## 2. Opciones Tecnológicas Consideradas

1. **Opción A (Recomendada — Monolito Modular Hexagonal en .NET 10 LTS + ASP.NET Core Blazor Web App)**:
   - Un único lenguaje (**C# 14**) y runtime (**.NET 10 LTS**, licencia `MIT`) tanto para el motor de dominio (`CMP-COST-ENGINE-001`) como para la interfaz de usuario en **Blazor Web App** (`CMP-WEB-UI-001`, combinando SSR e Interactive Server) y los endpoints REST para webhooks de Stripe.
2. **Opción B (SPA Separada en React/Angular + API Backend en .NET)**:
   - Duplica modelos de dominio, obliga a mantener dos pipelines de construcción/despliegue (Node.js + .NET) e introduce riesgo de pérdida de precisión al serializar decimales a `number` (IEEE 754 de doble precisión) en JavaScript.
3. **Opción C (Microservicios Distribuídos en .NET)**:
   - Introduce serialización de red innecesaria entre catálogo, calculadora y facturación, haciendo más complejo el cumplimiento de transacciones ACID síncronas en cascada (`< 2000 ms`).

## 3. Decisión Adoptada

Se elige la **Opción A (Monolito Modular Hexagonal en .NET 10 LTS con UI en ASP.NET Core Blazor Web App)**.

**Justificación técnica, de negocio y de seguridad**:
- **Simplicidad e Integridad de Tipos Fin-a-Fin**: Los componentes Razor (`EscandalloBuilderPage.razor`) enlazan directamente propiedades `System.Decimal` de C# con el motor `IEscandalloCalculatorService`, eliminando discrepancias de redondeo entre cliente y servidor.
- **Blindaje Nativo de Circuitos Blazor y Revalidación Zero-Trust (`SEC-REQ-TENANT-001`, `SEC-REQ-DOS-001`, `SEC-REQ-OBS-001`)**:
  - **Anti-Tampering en Circuitos**: Cada evento interactivo y formulario revalida el `userId` desde `AuthenticationStateProvider` en servidor mediante `ITenantIsolationGuard`, desconfiando de cualquier `[Parameter]` o `[SupplyParameterFromForm]` manipulable desde el cliente.
  - **Cotas de Memoria SignalR (`/_blazor`)**: Se configuran `DisconnectedCircuitRetentionPeriod = 3 min`, `MaxBufferedUnacknowledgedRenderBatches = 10`, `MaximumReceiveMessageSize = 32 KB` y un máximo de `5 circuitos simultáneos por usuario` (`RSK-BLAZOR-CIRCUIT-001`).
  - **Anti-Overposting y RBAC**: Los formularios de registro/perfil rechazan el atributo `role` (*Mass Assignment*) y `MaintainerObservabilityPage.razor` exige `[Authorize(Roles = "Maintainer")]` verificado en servidor.
- **Mantenibilidad Estricta (`quality-policy.yaml`) y Licenciamiento 100% Permisivo**: Separación limpia por módulos con Complejidad Ciclomática $\le 10$ y funciones $\le 40$ líneas, usando exclusivamente paquetes `MIT` y `Apache-2.0` (`ALLOW`).

## 4. Consecuencias

- **Positivas**:
  - Productividad máxima con un único stack tipado (`C# 14`), pruebas unitarias y de componentes rápidas (`xUnit` + `bUnit`) y despliegue en un solo contenedor ligero.
  - Latencia de simulación de precios inferior a `20 ms` en servidor (`< 100 ms` percibidos).
- **Negativas / Compensaciones (Trade-offs)**:
  - El ciclo de vida de los circuitos interactivos de Blazor Server está estrictamente acotado (`<= 5` por usuario, `32 KB` por mensaje), priorizando SSR estático donde no se requiera reactividad continua.

---

## 5. Historial de Revisiones y Control de Versiones

| Versión | Fecha | Decisores | Estado de la Decisión | Referencia (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-08 | `agent-system-architect` & Product Owner | Propuesta formal aceptada | `chg-001-escandallo-mvp` |
| **1.1.0** | 2026-10-08 | `agent-system-architect` & Product Owner | Incorporación de blindaje de circuitos SignalR, revalidación de parámetros en servidor y anti-Mass-Assignment (`SEC-REQ-*` v1.1.0) | `chg-001-escandallo-mvp` |
