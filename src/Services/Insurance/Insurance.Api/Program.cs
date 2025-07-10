using Insurance.Infrastructure.Persistence;
using FastEndpoints.Swagger;
using ThreadPilot.Common.Extensions;
using Insurance.Infrastructure.Extensions;
using Scalar.AspNetCore;
using FluentValidation;
using Insurance.Api.Validation;
using System.Text.Json;
using Microsoft.FeatureManagement;
using Insurance.Api.Contracts;
using Insurance.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Insurance.Api")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add Common Services
builder.Services.AddCommonServices();

// Add Application Services
builder.Services.AddApplication();

// Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Add FastEndpoints
builder.Services.AddFastEndpoints().AddSwaggerDocument();

// Add OpenAPI support
 builder.Services.AddOpenApi();

// Register health checks
builder.Services.AddHealthChecks();

// Add Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});

// HttpJson options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // Use default property names
    options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; // Ignore null value
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()); // Handle enums as strings
});

// Add Feature Management
builder.Services.AddFeatureManagement();

builder.Services.AddTransient<IValidator<GetPersonInsurancesRequest>, GetPersonInsurancesRequestValidator>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
    context.Database.EnsureCreated(); // This creates the database if it doesn't exist
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Insurance Service API")
            .WithDarkMode(true)
            .WithTheme(ScalarTheme.BluePlanet)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.AsyncHttp);
    });
}

// Map the health check endpoint
app.MapHealthChecks("/health");

// Enable CORS
app.UseCors();

// Use HTTPS redirection only if HTTPS is configured
if (app.Configuration.GetValue<string>("ASPNETCORE_URLS")?.Contains("https") == true)
{
    app.UseHttpsRedirection();
}

// Use FastEndpoints and FastEndpoints Swagger
app.UseFastEndpoints().UseSwaggerGen();

app.Run();

public partial class Program { } // For integration tests