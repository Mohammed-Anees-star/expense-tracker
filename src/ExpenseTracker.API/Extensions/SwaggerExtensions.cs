using System.Reflection;
using Microsoft.OpenApi.Models;

namespace ExpenseTracker.API.Extensions;

/// <summary>
/// Extension methods for configuring Swagger / OpenAPI documentation.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds Swagger generation with XML comments and API metadata.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Expense Tracker API",
                Version = "v1",
                Description = "A clean-architecture REST API for tracking personal expenses. " +
                              "Supports full CRUD operations and spending summaries by category.",
                Contact = new OpenApiContact
                {
                    Name = "Expense Tracker",
                    Email = "support@expensetracker.example.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT"
                }
            });

            // Include XML documentation comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);

            options.UseInlineDefinitionsForEnums();
        });

        return services;
    }

    /// <summary>
    /// Configures the Swagger UI middleware.
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense Tracker API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Expense Tracker API";
        });

        return app;
    }
}
