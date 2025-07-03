using FastEndpoints;
using Insurance.Application.Queries.GetPersonInsurances;
using Insurance.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using ProblemDetails = FastEndpoints.ProblemDetails;
using IMapper = AutoMapper.IMapper;

namespace Insurance.Api.Endpoints;

public class GetPersonInsurancesEndpoint : Endpoint<GetPersonInsurancesRequest, Results<Ok<PersonInsurancesResponse>, NotFound<ProblemDetails>>>
{
    private readonly ILogger<GetPersonInsurancesEndpoint> _logger;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public GetPersonInsurancesEndpoint(IMediator mediator, ILogger<GetPersonInsurancesEndpoint> logger, IMapper mapper)
    {
        _mediator = mediator;
        _logger = logger;
        _mapper = mapper;
    }

    public override void Configure()
    {
        Post("/api/v1/insurances/get-by-person");
        AllowAnonymous();
        Description(b => b
            .Produces<PersonInsurancesResponse>(200, "application/json")
            .ProducesProblem(404)
            .ProducesProblemFE<InternalErrorResponse>(500)
            .WithTags("Insurances")
            .WithSummary("Get person's insurances")
            .WithDescription("Retrieves all insurances for a person by their personal identification number (sent in the request body)"));
    }
        
    public override async Task<Results<Ok<PersonInsurancesResponse>, NotFound<ProblemDetails>>> ExecuteAsync(
        GetPersonInsurancesRequest req, 
        CancellationToken ct)
    {
        _logger.LogInformation("Retrieving insurances for person {PersonalIdentificationNumber}", 
            req.PersonalIdentificationNumber);

        var query = new GetPersonInsurancesQuery(req.PersonalIdentificationNumber);
        var insurances = await _mediator.Send(query, ct);

        if (insurances == null)
        {
            _logger.LogWarning("No insurances found for person {PersonalIdentificationNumber}", 
                req.PersonalIdentificationNumber);

            return TypedResults.NotFound(new ProblemDetails
            {
                Status = 404,
                Detail = $"No insurances found for person with identification number {req.PersonalIdentificationNumber}",
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation(
            "Successfully retrieved {InsuranceCount} insurances for person {PersonalIdentificationNumber} with total cost {TotalCost}", 
            insurances.Insurances.Count,
            req.PersonalIdentificationNumber,
            insurances.TotalMonthlyCost);

        return TypedResults.Ok(_mapper.Map<PersonInsurancesResponse>(insurances));
    }
}
