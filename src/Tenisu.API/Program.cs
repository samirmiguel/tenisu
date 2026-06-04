using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tenisu.API.HealthChecks;
using Tenisu.API.Middleware;
using Tenisu.Application.Interfaces;
using Tenisu.Application.Services;
using Tenisu.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Tenisu API", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddMemoryCache();

var dataFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "players.json");
builder.Services.AddSingleton<IPlayerRepository>(_ => new JsonPlayerRepository(dataFilePath));
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IStatsService, StatsService>();

builder.Services.AddHealthChecks()
    .AddCheck<DataFileHealthCheck>("data-file");

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tenisu API v1"));

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
