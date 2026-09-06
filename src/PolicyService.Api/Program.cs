using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PolicyService.Application.Policies.GetByReference;
using PolicyService.Application.Policies.Sell;
using PolicyService.Infrastructure;
using PolicyService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("PolicyDatabase")
    ?? throw new InvalidOperationException(
        "The PolicyDatabase connection string is missing.");

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(
                JsonNamingPolicy.CamelCase,
                allowIntegerValues: false));
    });

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddScoped<GetPolicyByReferenceHandler>();
builder.Services.AddScoped<SellPolicyHandler>();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    await using var scope =
        app.Services.CreateAsyncScope();

    var context = scope.ServiceProvider
        .GetRequiredService<PolicyDbContext>();

    await context.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = _ => false
    });
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready")
    });
app.MapControllers();

app.Run();

public partial class Program;