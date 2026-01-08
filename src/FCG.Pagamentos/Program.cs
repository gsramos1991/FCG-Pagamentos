using FCG.Pagamentos.Infra.Ioc;
using FCG.Pagamentos.API.Middlewares;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog from serilog.json and appsettings
builder.Configuration.AddJsonFile("serilog.json", optional: true, reloadOnChange: true);
builder.Host.UseSerilog((ctx, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

// Add services to the container.
builder.Services.AddDataService(builder.Configuration)
                .AddBusinessServices();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FCG Pagamentos API",
        Version = "v1",
        Description = "API de Pagamentos da FCG para operações de compra, cancelamento e consulta.",
        Contact = new OpenApiContact { Name = "FCG", Email = "contato@fcg.local" }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

builder.Services.AddProblemDetails();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG Pagamentos API v1");
    c.DocumentTitle = "FCG Pagamentos - Swagger";
});

//app.UseHttpsRedirection();
app.UseRouting();

// Error and request logging middleware
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting FCG Pagamentos API");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
