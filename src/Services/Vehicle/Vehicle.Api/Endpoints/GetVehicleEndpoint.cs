using Microsoft.AspNetCore.Http.HttpResults;
using Vehicle.Contracts;
using ProblemDetails = FastEndpoints.ProblemDetails;
using IMapper = AutoMapper.IMapper;
using MediatR;
using Vehicle.Application.Queries.GetVehicle;
using Vehicle.Api.Validation;

namespace Vehicle.Api.Endpoints;

public class GetVehicleEndpoint : EndpointWithoutRequest<Results<Ok<VehicleResponse>, NotFound<ProblemDetails>>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<GetVehicleEndpoint> _logger;

    public override void Configure()
    {
        Get("/vehicles/{registrationNumber}");
        AllowAnonymous();
        Description(b => b
            .Produces<VehicleResponse>(200, "application/json")
            .ProducesProblem(404)
            .ProducesProblem(500)
            .WithTags("Vehicles")
            .WithSummary("Get vehicle by registration number")
            .WithDescription("Retrieves vehicle information by its registration number"));
    }

    public GetVehicleEndpoint(IMediator mediator, IMapper mapper, ILogger<GetVehicleEndpoint> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<Results<Ok<VehicleResponse>, NotFound<ProblemDetails>>> ExecuteAsync(CancellationToken ct)
    {
        var registrationNumber = Route<string>("registrationNumber");
        var validationError = RegistrationNumberValidator.Validate(registrationNumber);
        if (validationError != null)
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Status = 404,
                Detail = validationError,
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Retrieving vehicle with registration number {RegistrationNumber}", registrationNumber);

        var vehicle = await _mediator.Send(new GetVehicleByRegistrationNumberQuery(registrationNumber!), ct);

        if (vehicle == null)
        {
            _logger.LogWarning("Vehicle not found with registration number {RegistrationNumber}", registrationNumber);
            return TypedResults.NotFound(new ProblemDetails
            {
                Status = 404,
                Detail = $"No vehicle found with registration number {registrationNumber}",
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Successfully retrieved vehicle {RegistrationNumber}", registrationNumber);
        return TypedResults.Ok(_mapper.Map<VehicleResponse>(vehicle));
    }
}