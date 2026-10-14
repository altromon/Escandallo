# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-auth-001.md
# ID Requerimiento: FR-AUTH-001
# Versión: 1.1.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-AUTH-001 @automated @regression
Feature: Registro de Cuenta de Usuario, Inicio de Sesión Obligatorio y Gestión de Centros por Titular

  Scenario: Registro de una nueva cuenta de cliente con creación automática de su primer centro de estética
    Given un visitante accede por primera vez a la pantalla de acceso de la aplicación sin sesión activa
    When completa el registro de cuenta con correo "nuevo@estetica.es", contraseña válida y nombre de centro "Estética Serrano"
    Then el servidor crea la cuenta y su centro inicial respondiendo con código HTTP 201
    And la sesión queda autenticada mostrando únicamente el centro "Estética Serrano" bajo la titularidad de "nuevo@estetica.es"

  Scenario Outline: Inicio de sesión con credenciales válidas, cierre de sesión, rechazo de credenciales incorrectas y bloqueo de intentos de escalada de rol
    Given el sistema de autenticación recibe una solicitud para el correo "<correo>" con la operación "<operacion>"
    When el servidor verifica las credenciales y políticas de seguridad de la cuenta
    Then el servidor responde con código HTTP <codigo_http> y estado de sesión "<estado_sesion>"

    Examples:
      | correo                | operacion                      | codigo_http | estado_sesion          |
      | cliente-a@estetica.es | login_credenciales_validas     | 200         | AUTENTICADO_CLIENTE_A  |
      | cliente-b@estetica.es | login_credenciales_validas     | 200         | AUTENTICADO_CLIENTE_B  |
      | cliente-a@estetica.es | logout_sesion_activa           | 200         | SESION_CERRADA         |
      | cliente-a@estetica.es | login_password_incorrecto      | 401         | RECHAZADO_CREDENCIALES |
      | atacante@estetica.es  | registro_con_rol_maintainer    | 403         | BLOQUEADO_OVERPOSTING  |
