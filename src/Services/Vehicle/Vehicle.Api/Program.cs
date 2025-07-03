using Serilog.Formatting.Compact;
using Vehicle.Application;
using Vehicle.Infrastructure;
using ThreadPilot.Common.ServiceExtensions;

var builder = WebApplication.CreateBuilder(args);

// Log.Logger = new LoggerConfiguration()
//     .ReadFrom.Configuration(builder.Configuration)
//     .Enrich.FromLogContext()
//     .Enrich.WithProperty("Service", "Vehicle.API")
//     .WriteTo.Console()
//     .WriteTo.File("logs/vehicle-api-.txt", rollingInterval: RollingInterval.Day)
//     .CreateLogger();

// Add Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter()));

// Add Application
builder.Services.AddCommonDependencies();

// Add Application
builder.Services.AddApplication();

// Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Add FastEndpoints
builder.Services.AddFastEndpoints().AddSwaggerDocument();

// Add services to the container.
// builder.Services.AddOpenApi();

// Configure JSON options for .NET 9
// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
//     options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
//     options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
// });


// Add SwaggerGen for Swashbuckle
builder.Services.AddSwaggerGen();

// Add API Versioning
// builder.Services.AddApiVersioning(options =>
// {
//     options.DefaultApiVersion = new ApiVersion(1, 0);
//     options.AssumeDefaultVersionWhenUnspecified = true;
//     options.ReportApiVersions = true;
//     options.ApiVersionReader = new UrlSegmentApiVersionReader();
// });

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