using System;
using Escandallo.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Escandallo.Web;

public static class EscandalloAuthRoutes
{
    public static void MapAuthAndTenantEndpoints(WebApplication app) {
        app.MapGet("/api/auth/me", (HttpContext ctx) => HandleMe(ctx));
        app.MapPost("/api/auth/login", (AuthLoginRequest req, HttpContext ctx) => HandleLogin(req, ctx));
        app.MapPost("/api/auth/register", (AuthRegisterRequest req, HttpContext ctx) => HandleRegister(req, ctx));
        app.MapPost("/api/auth/logout", (HttpContext ctx) => HandleLogout(ctx));
        app.MapPost("/api/centers/create", (CreateCenterRequest req, HttpContext ctx) => HandleCreateCenter(req, ctx));
        app.MapPost("/api/centers/copy", (CopyCenterCatalogRequest req, HttpContext ctx) => HandleCopyCenter(req, ctx));
    }

    public static string? ResolveAuthenticatedEmail(HttpContext ctx) {
        if (ctx.Request.Headers.TryGetValue("X-Escandallo-User", out var hdr) && !string.IsNullOrWhiteSpace(hdr)) return hdr.ToString().Trim().ToLowerInvariant();
        if (ctx.Request.Cookies.TryGetValue("escandallo_session", out var tok) && !string.IsNullOrWhiteSpace(tok) && MultiTenantAppStore.Sessions.TryGetValue(tok, out var email)) return email;
        return null;
    }

    private static IResult HandleMe(HttpContext ctx) {
        var email = ResolveAuthenticatedEmail(ctx);
        if (string.IsNullOrEmpty(email) || !MultiTenantAppStore.Users.TryGetValue(email, out var u)) return Results.Json(new { Authenticated = false }, statusCode: 401);
        return Results.Ok(new { Authenticated = true, User = new { u.Email, u.DisplayName, u.Role }, Centers = MultiTenantAppStore.ListCentersSummary(u.Email) });
    }

    private static IResult HandleLogin(AuthLoginRequest req, HttpContext ctx) {
        var (status, state, token, u) = TenantAuthService.Login(req.Email, req.Password);
        if (status != 200 || u is null || token is null) return Results.Json(new { Authenticated = false, State = state }, statusCode: status);
        ctx.Response.Cookies.Append("escandallo_session", token, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Lax, Path = "/" });
        return Results.Ok(new { Authenticated = true, State = state, Token = token, User = new { u.Email, u.DisplayName, u.Role }, Centers = MultiTenantAppStore.ListCentersSummary(u.Email) });
    }

    private static IResult HandleRegister(AuthRegisterRequest req, HttpContext ctx) {
        var (status, state, token, u, center) = TenantAuthService.Register(req);
        if (status != 201 || u is null || token is null || center is null) return Results.Json(new { Authenticated = false, State = state }, statusCode: status);
        ctx.Response.Cookies.Append("escandallo_session", token, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Lax, Path = "/" });
        return Results.Json(new { Authenticated = true, State = state, Token = token, User = new { u.Email, u.DisplayName, u.Role }, InitialCenterId = center.CenterId, Centers = MultiTenantAppStore.ListCentersSummary(u.Email) }, statusCode: 201);
    }

    private static IResult HandleLogout(HttpContext ctx) {
        ctx.Request.Cookies.TryGetValue("escandallo_session", out var tok);
        var (status, state) = TenantAuthService.Logout(tok);
        ctx.Response.Cookies.Delete("escandallo_session", new CookieOptions { Path = "/" });
        return Results.Json(new { Authenticated = false, State = state }, statusCode: status);
    }

    private static IResult HandleCreateCenter(CreateCenterRequest req, HttpContext ctx) {
        var owner = ResolveAuthenticatedEmail(ctx) ?? req.OwnerEmail;
        if (string.IsNullOrWhiteSpace(owner)) return Results.StatusCode(401);
        var created = TenantAuthService.CreateCenterForOwner(owner, string.IsNullOrWhiteSpace(req.CenterName) ? "Nuevo Centro" : req.CenterName.Trim());
        return Results.Json(new { Center = created, Centers = MultiTenantAppStore.ListCentersSummary(owner) }, statusCode: 201);
    }

    private static IResult HandleCopyCenter(CopyCenterCatalogRequest req, HttpContext ctx) {
        var owner = ResolveAuthenticatedEmail(ctx) ?? req.OwnerEmail;
        if (string.IsNullOrWhiteSpace(owner)) return Results.StatusCode(401);
        var (status, state, target) = TenantAuthService.CopyCatalogBetweenCenters(owner, req.SourceCenterId, req.TargetCenterId);
        return Results.Json(new { Status = status, CopyState = state, TargetCenter = target }, statusCode: status);
    }
}
