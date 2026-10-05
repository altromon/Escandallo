---
id: FR-TENANT-001
type: requirement
title: "Aislamiento Multi-Tenant de Centros de Estética por Cliente"
status: draft
version: "1.0.0"
schema-version: "1.0"
category: functional
derives-from:
  - UC-COST-CATALOG-001
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - "@FR-TENANT-001"
  - "@automated"
  - "@regression"
supersedes: null
superseded-by: null
---

# Requirement: Aislamiento Multi-Tenant de Centros de Estética por Cliente

## 1. Normative Statement

El sistema DEBE permitir a cada cliente autenticado crear y gestionar uno o varios centros de estética bajo su titularidad —incluyendo la opción de copiar el catálogo de productos entre centros pertenecientes al mismo cliente propietario (Mejora M4)—, garantizando mediante controles de autorización en servidor que ningún usuario pueda visualizar, copiar, modificar, recalcular ni exportar costes, escandallos o informes pertenecientes a centros de estética de otros clientes.

## 2. Acceptance Criteria (Gherkin)

```gherkin
@FR-TENANT-001 @automated @regression @security
Feature: Aislamiento Multi-Tenant de Centros de Estética por Cliente

  Scenario: Usuario con múltiples centros copia su catálogo de productos entre sus propios centros manteniendo el aislamiento frente a terceros
    Given el usuario "cliente-a@estetica.es" es propietario de "Centro Madrid" y "Centro Valencia"
    And el usuario "cliente-b@estetica.es" es propietario de "Centro Sevilla"
    When "cliente-a@estetica.es" solicita copiar el catálogo de productos desde "Centro Madrid" hacia "Centro Valencia"
    Then el servidor copia los productos hacia "Centro Valencia" con código HTTP 200
    And los datos de "Centro Sevilla" permanecen inaccesibles para "cliente-a@estetica.es"

  @boundary @idempotence
  Scenario Outline: Casos límite de titularidad y volumen al clonar o consultar catálogos entre centros del mismo propietario
    Given el usuario "<usuario_autenticado>" posee <num_centros> centros registrados y el centro origen tiene <items_catalogo> productos
    And el centro "<centro_objetivo>" pertenece al propietario "<propietario_real>"
    When "<usuario_autenticado>" ejecuta la acción "<accion>" sobre "<centro_objetivo>"
    Then el servidor responde con código HTTP <codigo_http> y procesa <items_copiados> productos

    Examples:
      | usuario_autenticado   | num_centros | items_catalogo | centro_objetivo | propietario_real      | accion                    | codigo_http | items_copiados |
      | cliente-a@estetica.es | 1           | 0              | Centro Madrid   | cliente-a@estetica.es | consultar_catalogo_vacio  | 200         | 0              |
      | cliente-a@estetica.es | 2           | 1              | Centro Valencia | cliente-a@estetica.es | copiar_catalogo_minimo    | 200         | 1              |
      | cliente-a@estetica.es | 2           | 5000           | Centro Valencia | cliente-a@estetica.es | copiar_catalogo_max_filas | 200         | 5000           |

  @invalid @security
  Scenario Outline: Rechazo de accesos fuera de rango, identificadores malformados o copia cruzada entre distintos clientes
    Given el usuario "<usuario_autenticado>" está autenticado en el servidor
    And el centro objetivo tiene identificador "<centro_id>" perteneciente a "<propietario_real>"
    When "<usuario_autenticado>" intenta ejecutar la acción "<accion>" sobre "<centro_id>"
    Then el servidor rechaza la petición con código HTTP <codigo_http>

    Examples:
      | usuario_autenticado   | centro_id                            | propietario_real      | accion                    | codigo_http |
      | cliente-a@estetica.es | 00000000-0000-4000-8000-000000000099 | cliente-b@estetica.es | consultar_catalogo_costes | 403         |
      | cliente-b@estetica.es | 00000000-0000-4000-8000-000000000001 | cliente-a@estetica.es | copiar_catalogo_ajeno     | 403         |
      | cliente-a@estetica.es | id-no-uuid-invalido                  | ninguno               | consultar_catalogo_costes | 400         |
      | cliente-a@estetica.es | ""                                   | ninguno               | copiar_catalogo_propio    | 400         |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del requisito de aislamiento multi-centro y multi-cliente. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de Mejora M4 (copia de catálogo entre centros del mismo propietario). |
| 1.3.0 | 2026-10-05 | agent-qa-engineer | Descomposición BDD en escenario nominal, casos límite (Boundary) y casos inválidos/fuera de rango. |

