using Escandallo.Web;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();

EscandalloApiRoutes.MapStateAndCatalogEndpoints(app);
EscandalloExportRoutes.MapDocumentAndBillingEndpoints(app);

app.Run();

public partial class Program { }
