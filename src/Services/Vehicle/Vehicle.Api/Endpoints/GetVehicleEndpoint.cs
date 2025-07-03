using Microsoft.AspNetCore.Http.HttpResults;
using Vehicle.Application.DTOs;
using Vehicle.Application.Interfaces;
using Vehicle.Contracts;
using ProblemDetails = FastEndpoints.ProblemDetails;
using IMapper = AutoMapper.IMapper;

namespace Vehicle.Api.Endpoints;

public class GetVehicleEndpoint : Endpoint<GetVehicleRequest, Results<Ok<GetVehicleResponse>, NotFound<ProblemDetails>>>
{
    private readonly IVehicleService _vehicleService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetVehicleEndpoint> _logger;

    public GetVehicleEndpoint(IVehicleService vehicleService, ILogger<GetVehicleEndpoint> logger, IMapper mapper)
    {
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public override void Configure()
    {
        Get("/api/v1/vehicles/{RegistrationNumber}");
        AllowAnonymous();
        Description(b => b
            .Produces<VehicleDto>(200, "application/json")
            .ProducesProblem(404)
            .ProducesProblemFE<InternalErrorResponse>(500)
            .WithTags("Vehicles")
            .WithSummary("Get vehicle by registration number")
            .WithDescription("Retrieves vehicle information by its registration number"));
    }

    public override async Task<Results<Ok<GetVehicleResponse>, NotFound<ProblemDetails>>> ExecuteAsync(
        GetVehicleRequest req,
        CancellationToken ct)
    {
        //using var activity = Activity.StartActivity("GetVehicle");
        //activity?.SetTag("registration.number", req.RegistrationNumber);

        _logger.LogInformation("Retrieving vehicle with registration number {RegistrationNumber}",
            req.RegistrationNumber);

        var vehicle = await _vehicleService.GetVehicleByRegistrationNumberAsync(req.RegistrationNumber);

        if (vehicle == null)
        {
            _logger.LogWarning("Vehicle not found with registration number {RegistrationNumber}",
                req.RegistrationNumber);

            return TypedResults.NotFound(new ProblemDetails
            {
                Status = 404,
                Detail = $"No vehicle found with registration number {req.RegistrationNumber}",
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Successfully retrieved vehicle {RegistrationNumber}",
            req.RegistrationNumber);

        return TypedResults.Ok(_mapper.Map<GetVehicleResponse>(vehicle));
    }
}