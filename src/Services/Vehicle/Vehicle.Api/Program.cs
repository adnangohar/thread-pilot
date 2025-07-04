using Scalar.AspNetCore;
using Vehicle.Infrastructure.Persistence;
using FastEndpoints.Swagger;
using ThreadPilot.Common.Extensions;
using Vehicle.Infrastructure.Extensions;
using Vehicle.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Common Services
builder.Services.AddCommonServices();

// Add Application
builder.Services.AddApplication();

// Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Add FastEndpoints
builder.Services.AddFastEndpoints().AddSwaggerDocument();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
 builder.Services.AddOpenApi();

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

app.UseHttpsRedirection();
// app.UseCors("AllowAll");

// Use FastEndpoints and FastEndpoints Swagger
// Use FastEndpoints
app.UseFastEndpoints().UseSwaggerGen();

// Log.Logger = new LoggerConfiguration()
//     .ReadFrom.Configuration(builder.Configuration)
//     .Enrich.FromLogContext()
//     .Enrich.WithProperty("Service", "Vehicle.API")
//     .WriteTo.Console()
//     .WriteTo.File("logs/vehicle-api-.txt", rollingInterval: RollingInterval.Day)
//     .CreateLogger();

// Add Serilog
// builder.Host.UseSerilog((context, services, configuration) => configuration
//     .ReadFrom.Configuration(context.Configuration)
//     .ReadFrom.Services(services)
//     .Enrich.FromLogContext()
//     .WriteTo.Console(new CompactJsonFormatter()));

// Configure JSON options for .NET 9
// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
//     options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
//     options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
// });

// Add API Versioning
// builder.Services.AddApiVersioning(options =>
// {
//     options.DefaultApiVersion = new ApiVersion(1, 0);
//     options.AssumeDefaultVersionWhenUnspecified = true;
//     options.ReportApiVersions = true;
//     options.ApiVersionReader = new UrlSegmentApiVersionReader();
// });

// Add CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy.AllowAnyOrigin()
//               .AllowAnyMethod()
//               .AllowAnyHeader();
//     });
// });

app.Run();

public partial class Program { } // For integration tests