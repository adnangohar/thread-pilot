using Scalar.AspNetCore;
using Vehicle.Infrastructure.Persistence;
using FastEndpoints.Swagger;
using ThreadPilot.Common.Extensions;
using Vehicle.Infrastructure.Extensions;
using Vehicle.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Vehicle.Api")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add Common Services
builder.Services.AddCommonServices();

// Add Application
builder.Services.AddCore();

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

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VehicleDbContext>();
    context.Database.EnsureCreated(); // This creates the database if it doesn't exist
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Vehicle Service API")
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

// Add API Versioning
// builder.Services.AddApiVersioning(options =>
// {
//     options.DefaultApiVersion = new ApiVersion(1, 0);
//     options.AssumeDefaultVersionWhenUnspecified = true;
//     options.ReportApiVersions = true;
//     options.ApiVersionReader = new UrlSegmentApiVersionReader();
// });

app.Run();

public partial class Program { } // For integration tests