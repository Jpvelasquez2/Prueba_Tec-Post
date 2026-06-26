using System.Reflection;
using FutbolManager.Api.Middlewares;
using FutbolManager.Application;
using FutbolManager.Persistence;
using Microsoft.OpenApi.Models;

// =============================================================================
// FutbolManager.Api — Composition Root.
// Cablea Application + Persistence, configura Swagger con comentarios XML,
// CORS, manejo global de excepciones y el pipeline HTTP.
// =============================================================================

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 1) Servicios MVC + validación automática.
// -----------------------------------------------------------------------------
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(_ =>
    {
        // Por defecto Web API ya devuelve ValidationProblemDetails (400) cuando
        // falla la validación de DataAnnotations en los DTOs. Lo dejamos así.
    });

builder.Services.AddEndpointsApiExplorer();

// -----------------------------------------------------------------------------
// 2) Swagger / OpenAPI con comentarios XML de cada ensamblado.
// -----------------------------------------------------------------------------
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FutbolManager API",
        Version = "v1",
        Description = "Gestión de equipos, partidos y tabla de posiciones.",
    });

    // Incluir los XML docs generados por todos los proyectos.
    foreach (var xml in EncontrarArchivosXmlDocs())
    {
        options.IncludeXmlComments(xml, includeControllerXmlComments: true);
    }
});

// -----------------------------------------------------------------------------
// 3) Capas de la aplicación.
// -----------------------------------------------------------------------------
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);

// -----------------------------------------------------------------------------
// 4) CORS abierto para desarrollo. En producción restringir a dominios concretos.
// -----------------------------------------------------------------------------
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());
});

var app = builder.Build();

// -----------------------------------------------------------------------------
// 5) Pipeline HTTP.
// -----------------------------------------------------------------------------

// El middleware global debe ir lo más arriba posible para capturar cualquier
// excepción del resto del pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt => opt.SwaggerEndpoint("/swagger/v1/swagger.json", "FutbolManager API v1"));
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();

// -----------------------------------------------------------------------------
// Helpers
// -----------------------------------------------------------------------------

// Devuelve las rutas a los archivos XML generados (Api + Application + Domain)
// para que Swagger pueda exponer los <summary> en la UI.
static IEnumerable<string> EncontrarArchivosXmlDocs()
{
    var baseDir = AppContext.BaseDirectory;
    return Directory.EnumerateFiles(baseDir, "FutbolManager.*.xml", SearchOption.TopDirectoryOnly);
}

/// <summary>
/// Clase parcial generada para <c>WebApplicationFactory&lt;Program&gt;</c>
/// en pruebas funcionales.
/// </summary>
public partial class Program { }
