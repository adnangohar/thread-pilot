using Microsoft.AspNetCore.HttpLogging;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vehicle.Application.Interfaces;
using Vehicle.Application.Mappings;
using Vehicle.Application.Services;
using Vehicle.Domain.Repositories;
using MediatR;
using Vehicle.Api.Mappings;
using FluentValidation;
using Vehicle.Application.Queries.GetVehicle;
using ThreadPilot.Common.Behaviors;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    //.Enrich.WithMachineName()
    //.Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Service", "VehicleService")
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File(
        new CompactJsonFormatter(),
        "logs/vehicle-service-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_485_760, // 10MB
        rollOnFileSizeLimit: true)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter()));

// Add services
builder.Services.AddFastEndpoints().AddSwaggerDocument();

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure JSON options for .NET 9
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(VehicleMappingProfile), typeof(GetVehicleResponseProfile));

// Add repositories
builder.Services.AddSingleton<IVehicleRepository, Vehicle.Infrastructure.Repositories.InMemoryVehicleRepository>();

// Add services
builder.Services.AddScoped<IVehicleService, VehicleService>();

// Add HTTP logging
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
});

// Add OpenTelemetry
// builder.Services.AddOpenTelemetry()
//     .ConfigureResource(resource => resource
//         .AddService("VehicleService")
//         .AddAttributes(new Dictionary<string, object>
//         {
//             ["environment"] = builder.Environment.EnvironmentName,
//             ["version"] = "1.0.0"
//         }))
//     .WithTracing(tracing => tracing
//         .AddAspNetCoreInstrumentation()
//         .AddHttpClientInstrumentation()
//         .AddConsoleExporter())
//     .WithMetrics(metrics => metrics
//         .AddAspNetCoreInstrumentation()
//         .AddHttpClientInstrumentation()
//         .AddConsoleExporter());

// Add SwaggerGen for Swashbuckle
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetVehicleByRegistrationNumberQuery>());
// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<GetVehicleByRegistrationNumberQuery>();
// Add MediatR pipeline behavior for validation
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

// Use FastEndpoints and FastEndpoints Swagger
app.UseFastEndpoints();
app.UseOpenApi();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vehicle Service API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();



app.Run();