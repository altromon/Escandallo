---
id: ARCH-RUN-001
type: runtime-view
title: "06. Runtime View and Dynamic Behavior"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 6
naf-perspective: "Behaviour & Sequences"
sequences:
  - SEQ-CASCADE-RECALC-001
  - SEQ-QUOTA-STRIPE-002
  - SEQ-SEC-MITIGATION-003
flows:
  - FLW-COST-TO-PVP-001
satisfies-requirements:
  - FR-TENANT-001
  - FR-COST-001
  - FR-CALC-001
  - FR-HIST-001
  - FR-REPORT-001
  - FR-BILLING-001
  - FR-OBS-001
  - QR-UI-A11Y-001
  - QR-SEC-001
  - SEC-REQ-TENANT-001
  - SEC-REQ-BILLING-001
  - SEC-REQ-XLSX-001
  - SEC-REQ-DOS-001
  - SEC-REQ-OBS-001
supersedes: null
superseded-by: null
---

# 06. Runtime View (arc42 Sec. 6 / NAF Behaviour)

## 1. Nominal Scenario: Previsualización de Impacto (M3), Actualización de Coste y Recálculo en Cascada (`SEQ-CASCADE-RECALC-001`)

Describe la interacción dinámica cuando una gestora modifica el precio de compra de un cosmético o el sueldo de un empleado, visualiza cuántos escandallos se verán afectados (`M3`) y confirma la actualización atómica con evaluación del Semáforo de Erosión de Margen (`M2`).

```mermaid
sequenceDiagram
    autonumber
    actor User as Gestora (ACT-ESTHETIC-USER-001)
    participant UI as Cliente Web (CMP-WEB-UI-001)
    participant IAM as Guardia RLS (CMP-IAM-TENANT-001)
    participant Engine as Motor Costes (CMP-COST-ENGINE-001)
    participant DB as PostgreSQL 16 (RLS Activo)

    User->>UI: Edita precio de "Vial Ácido Hialurónico" (30€ -> 39€)
    UI->>IAM: POST /api/v1/centers/{cId}/costs/{id}/preview-impact
    IAM->>DB: SET LOCAL app.current_center_id = cId
    IAM->>Engine: calculateImpactPreview(costId, newPrice)
    Engine->>DB: SELECT escandallos vinculados a costId
    DB-->>Engine: 4 escandallos activos vinculados
    Engine-->>UI: Impacto: 4 escandallos (2 pasan a Semáforo 🟡/🔴 respecto a su PVP Comercial Fijado)
    UI-->>User: Muestra aviso M3: "Este cambio recalculará 4 escandallos"
    User->>UI: Confirma actualización
    UI->>IAM: PATCH /api/v1/centers/{cId}/costs/{id}
    IAM->>Engine: updateCostWithCascade(costId, newPrice)
    Engine->>DB: BEGIN TRANSACTION (SET LOCAL app.current_center_id = cId)
    Engine->>DB: INSERT INTO cost_history (snapshot previo porque N=4 >= 1)
    Engine->>DB: UPDATE costs SET purchase_price = 39.0000
    Engine->>DB: INSERT INTO escandallo_history (4 snapshots previos)
    Engine->>DB: UPDATE escandallos (nuevos Costes, Beneficios, PVP Calculado y Semáforo 🟢/🟡/🔴)
    Engine->>DB: COMMIT
    Engine-->>UI: 200 OK (Coste actualizado + 4 escandallos recalculados)
```

---

## 2. Nominal & Security Scenario: Control Atómico de Cuota Freemium y Suscripción Stripe (`SEQ-QUOTA-STRIPE-002`)

Modela la serialización con `SELECT ... FOR UPDATE` para impedir condiciones de carrera sobre el límite de 2 escandallos `active` (`SEC-REQ-BILLING-001`) y la activación verificada mediante webhook de Stripe.

```mermaid
sequenceDiagram
    autonumber
    actor User as Gestora (ACT-ESTHETIC-USER-001)
    participant IAM as Guardia RLS (CMP-IAM-TENANT-001)
    participant Bill as Módulo Billing (CMP-BILLING-STRIPE-001)
    participant DB as PostgreSQL 16
    participant Stripe as Stripe (ACT-STRIPE-001)

    User->>IAM: POST /api/v1/centers/{cId}/escandallos (Intento de 3er escandallo activo)
    IAM->>Bill: enforceQuotaAndCreate(cId, payload)
    Bill->>DB: BEGIN; SELECT subscription_status FROM centers WHERE id = cId FOR UPDATE
    Bill->>DB: SELECT COUNT(*) FROM escandallos WHERE center_id = cId AND status = 'active'
    DB-->>Bill: active_count = 2, subscription_status = 'free'
    Bill->>DB: ROLLBACK
    Bill-->>User: 402 Payment Required (Opciones: Archivar 1 activo [M1] o Suscribirse 14,90€/mes | 149€/año)
    User->>Bill: POST /api/v1/centers/{cId}/billing/checkout (Plan Mensual 14,90 EUR)
    Bill->>Stripe: Crear Checkout Session (metadata: center_id=cId)
    Stripe-->>User: Redirección a pasarela de pago Stripe
    Stripe->>Bill: POST /api/v1/billing/webhooks/stripe (Stripe-Signature HMAC-SHA256)
    Bill->>Bill: Verifica firma HMAC y ventana timestamp <= 300s
    Bill->>DB: BEGIN; INSERT INTO stripe_webhook_events (event_id)
    Bill->>DB: UPDATE centers SET subscription_status = 'active' WHERE id = cId; COMMIT
    Bill-->>Stripe: 200 OK
```

---

## 3. Security Scenario: Mitigación de Ataques IDOR, Inyección `.xlsx` y Fuga en Modo `DEBUG` (`SEQ-SEC-MITIGATION-003`)

```mermaid
sequenceDiagram
    autonumber
    actor Attacker as Actor de Amenaza (ACT-THREAT-*)
    participant IAM as Guardia IAM/RLS (CMP-IAM-TENANT-001)
    participant Doc as Motor Documental (CMP-DOC-IO-001)
    participant Obs as Telemetría & Redactor (CMP-OBS-TELEMETRY-001)

    Attacker->>IAM: POST /api/v1/centers/C_PROPIO/catalog/copy (source_center_id = C_AJENO)
    IAM->>IAM: Verifica titularidad dual (U_ATACANTE no es dueño de C_AJENO)
    IAM->>Obs: Emitir evento SECURITY (CROSS_TENANT_IDOR_ATTEMPT)
    Obs->>Obs: Redactar headers sensibles ([REDACTED] incluso en nivel DEBUG) y escapar CRLF
    IAM-->>Attacker: 403 Forbidden (0 registros leídos de C_AJENO)

    Attacker->>IAM: POST /api/v1/centers/C_PROPIO/catalog/import-xlsx (ZipBomb 15MB descomprimido / XXE)
    IAM->>Doc: inspectAndImportXlsx(buffer)
    Doc->>Doc: Pre-inspección cabecera ZIP detecta tamaño > 10MB o entidad DOCTYPE externa
    Doc->>Obs: Emitir evento SECURITY (MALICIOUS_XLSX_REJECTED)
    Doc-->>Attacker: 422 Unprocessable Entity (Lectura abortada sin agotar RAM)
```

---

## 4. Dynamic Scenarios and Associated Requirements Matrix

| Scenario ID | Type | Satisfied Requirements | Participating Components |
| :--- | :---: | :--- | :--- |
| `SEQ-CASCADE-RECALC-001` | Nominal | `FR-COST-001`, `FR-CALC-001`, `FR-HIST-001`, `QR-UI-A11Y-001` | `CMP-WEB-UI-001`, `CMP-IAM-TENANT-001`, `CMP-COST-ENGINE-001` |
| `SEQ-QUOTA-STRIPE-002` | Nominal & Security | `FR-BILLING-001`, `QR-SEC-001`, `SEC-REQ-BILLING-001` | `CMP-IAM-TENANT-001`, `CMP-BILLING-STRIPE-001` |
| `SEQ-SEC-MITIGATION-003` | Security | `FR-TENANT-001`, `FR-REPORT-001`, `FR-OBS-001`, `SEC-REQ-TENANT-001`, `SEC-REQ-XLSX-001`, `SEC-REQ-DOS-001`, `SEC-REQ-OBS-001` | `CMP-IAM-TENANT-001`, `CMP-DOC-IO-001`, `CMP-OBS-TELEMETRY-001` |

---

## 5. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Definición inicial de escenarios dinámicos y diagramas de secuencia | ARCH-INIT-001 |
