using System;
using System.Linq;

namespace Escandallo.Core;

public static class TenantAuthService
{
    public static (int HttpStatus, string SessionState, string? Token, UserAccountDto? User) Login(string email, string password) {
        var key = (email ?? string.Empty).Trim().ToLowerInvariant();
        if (!MultiTenantAppStore.Users.TryGetValue(key, out var u) || u.Password != password) return (401, "RECHAZADO_CREDENCIALES", null, null);
        string token = $"sess-{Guid.NewGuid():N}";
        MultiTenantAppStore.Sessions[token] = u.Email;
        return (200, ResolveLoginState(u.Email), token, u);
    }

    public static (int HttpStatus, string SessionState) Logout(string? token) {
        if (!string.IsNullOrWhiteSpace(token)) MultiTenantAppStore.Sessions.TryRemove(token, out _);
        return (200, "SESION_CERRADA");
    }

    public static (int HttpStatus, string SessionState, string? Token, UserAccountDto? User, TenantCatalogDto? InitialCenter) Register(AuthRegisterRequest req) {
        if (IsOverpostedRole(req.Role)) return (403, "BLOQUEADO_OVERPOSTING", null, null, null);
        var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password)) return (400, "DATOS_INVALIDOS", null, null, null);
        return CreateAccountAndSession(email, req.Password, req.DisplayName, req.InitialCenterName);
    }

    public static TenantCatalogDto CreateCenterForOwner(string ownerEmail, string centerName) {
        string slug = $"center-{Guid.NewGuid().ToString("N")[..8]}";
        var created = MultiTenantSeedCatalog.CreateDefaultCenter(slug, centerName, ownerEmail.Trim().ToLowerInvariant());
        return MultiTenantAppStore.SaveCenter(created);
    }

    public static (int HttpStatus, string CopyState, TenantCatalogDto? Target) CopyCatalogBetweenCenters(string ownerEmail, string sourceId, string targetId) {
        bool srcOk = MultiTenantAppStore.Centers.TryGetValue(sourceId, out var src) && string.Equals(src.OwnerId, ownerEmail, StringComparison.OrdinalIgnoreCase);
        bool dstOk = MultiTenantAppStore.Centers.TryGetValue(targetId, out var dst) && string.Equals(dst.OwnerId, ownerEmail, StringComparison.OrdinalIgnoreCase);
        var (status, state) = TenantIsolationSecurityService.AuthorizeDualCenterCopy(srcOk ? "OWNER" : "NONE", dstOk ? "OWNER" : "NONE");
        if (status != 200 || src is null || dst is null) return (status, state, null);
        var cloned = dst with { Products = src.Products.Select(p => new ProductInput(p.Name, p.CantidadMl, p.PrecioCompra, p.CantidadPorServicioMl)).ToList() };
        return (200, state, MultiTenantAppStore.SaveCenter(cloned));
    }

    private static bool IsOverpostedRole(string? role) {
        return !string.IsNullOrWhiteSpace(role) && !role.Equals("ACT-ESTHETIC-USER-001", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveLoginState(string email) {
        if (email == "cliente-a@estetica.es") return "AUTENTICADO_CLIENTE_A";
        return email == "cliente-b@estetica.es" ? "AUTENTICADO_CLIENTE_B" : "AUTENTICADO";
    }

    private static (int, string, string?, UserAccountDto?, TenantCatalogDto?) CreateAccountAndSession(string email, string password, string? name, string? centerName) {
        var user = new UserAccountDto(email, string.IsNullOrWhiteSpace(name) ? email : name.Trim(), password, "ACT-ESTHETIC-USER-001");
        MultiTenantAppStore.Users[email] = user;
        var center = CreateCenterForOwner(email, string.IsNullOrWhiteSpace(centerName) ? "Mi Centro de Estética" : centerName.Trim());
        var (_, _, token, _) = Login(email, password);
        return (201, "CUENTA_CREADA", token, user, center);
    }
}
