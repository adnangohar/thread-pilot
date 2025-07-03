using System;
using System.Net;
using System.Text.Json;
using Insurance.Application.Interfaces;
using Insurance.Contracts;
using Microsoft.Extensions.Configuration;

namespace Insurance.Infrastructure.Services;

public class VehicleServiceClient : IVehicleService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public VehicleServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        var vehicleServiceUrl = configuration["VehicleService:BaseUrl"] ?? "https://localhost:5001";
        _httpClient.BaseAddress = new Uri(vehicleServiceUrl);

        _jsonOptions = new JsonSerializerOptions {PropertyNameCaseInsensitive = true };
    }
    public async Task<VehicleInfoDto?> GetVehicleInfoAsync(string registrationNumber)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/vehicles/{registrationNumber}");
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // _logger.LogInformation("Vehicle not found: {RegistrationNumber}", registrationNumber);
                return null;
            }
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VehicleInfoDto>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Failed to get vehicle info for {RegistrationNumber}", registrationNumber);
            return null;
        }
    }
}
