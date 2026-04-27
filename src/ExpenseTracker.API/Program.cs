using ExpenseTracker.API.Extensions;
using ExpenseTracker.API.Middleware;
using ExpenseTracker.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ExpenseTracker.Infrastructure.Data;

// ============================================================================
// Bootstrap logger — active until the host finishes wiring up
// ============================================================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Expense Tracker API");

    var builder = WebApplication.CreateBuilder(args);

    // ============================================================================
    // Serilog — reads configuration from appsettings.json "Serilog" section
    // ============================================================================
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

    // ============================================================================
    // Services
    // ============================================================================
    builder.Services.AddControllers();
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddSwaggerDocumentation();

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("database");

    // CORS — allow Power Apps origin (configure specific origin in production)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("PowerApps", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    // ============================================================================
    // Build
    // ============================================================================
    var app = builder.Build();

    // Apply EF migrations automatically on startup (safe for dev & Azure deploy)
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Log.Information("Database migrations applied");
    }

    // ============================================================================
    // Middleware pipeline
    // ============================================================================
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    });

    if (app.Environment.IsDevelopment())
        app.UseSwaggerDocumentation();

    app.UseHttpsRedirection();
    app.UseCors("PowerApps");
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program visible for integration tests
public partial class Program { }
