using Insurance.Infrastructure.Persistence;
using FastEndpoints.Swagger;
using ThreadPilot.Common.Extensions;
using Insurance.Infrastructure.Extensions;
using Insurance.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Common Services
builder.Services.AddCommonServices();

// Add Application
builder.Services.AddApplication();

// Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Add FastEndpoints
builder.Services.AddFastEndpoints().SwaggerDocument();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
    
    // Add Swagger UI for FastEndpoints
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Insurance Service API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// Use FastEndpoints
app.UseFastEndpoints().UseSwaggerGen();

app.Run();

public partial class Program { } // For integration tests