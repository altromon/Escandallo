using System.IO;
using System.Linq;
using Escandallo.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

Program.CreateWebApp(args).Run();

public partial class Program
{
    public static WebApplication CreateWebApp(string[] args, string? url = null) {
        var root = ResolveContentRoot();
        var webRoot = Path.Combine(root, "wwwroot");
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, ContentRootPath = root, WebRootPath = webRoot });
        if (!string.IsNullOrEmpty(url)) builder.WebHost.UseUrls(url);
        var app = builder.Build();
        ConfigureSecurityAndStaticFiles(app, webRoot);
        EscandalloAuthRoutes.MapAuthAndTenantEndpoints(app);
        EscandalloApiRoutes.MapStateAndCatalogEndpoints(app);
        EscandalloExportRoutes.MapDocumentAndBillingEndpoints(app);
        return app;
    }

    private static void ConfigureSecurityAndStaticFiles(WebApplication app, string webRoot) {
        app.Use(async (ctx, next) => {
            ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
            ctx.Response.Headers["X-Frame-Options"] = "DENY";
            ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            ctx.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            ctx.Response.Headers["Pragma"] = "no-cache";
            await next();
        });
        app.UseDefaultFiles(); app.UseStaticFiles();
        app.MapGet("/", () => Results.File(Path.Combine(webRoot, "index.html"), "text/html; charset=utf-8"));
        app.MapGet("/styles.css", () => Results.File(Path.Combine(webRoot, "app.css"), "text/css; charset=utf-8"));
        app.MapGet("/favicon.ico", () => Results.NoContent());
    }

    private static string ResolveContentRoot() {
        string[] c = [Directory.GetCurrentDirectory(), Path.Combine(Directory.GetCurrentDirectory(), "src", "Escandallo.Web"), System.AppContext.BaseDirectory, Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Escandallo.Web"))];
        return c.FirstOrDefault(p => Directory.Exists(Path.Combine(p, "wwwroot"))) ?? Directory.GetCurrentDirectory();
    }
}
