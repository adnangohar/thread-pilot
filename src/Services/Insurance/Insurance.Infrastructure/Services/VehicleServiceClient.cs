using System.Net;
using System.Text.Json;
using Insurance.Application.Interfaces;
using Insurance.Contracts;
using Microsoft.Extensions.Logging;

namespace Insurance.Infrastructure.Services;

public class VehicleServiceClient : IVehicleService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<VehicleServiceClient> _logger;

    public VehicleServiceClient(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger<VehicleServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<VehicleResponse?> GetVehicleInfoAsync(string registrationNumber, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/vehicles/{registrationNumber}", cancellationToken);
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Vehicle not found: {RegistrationNumber}", registrationNumber);
                return null;
            }
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VehicleResponse>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get vehicle info for {RegistrationNumber}", registrationNumber);
            return null;
        }
    }
}
