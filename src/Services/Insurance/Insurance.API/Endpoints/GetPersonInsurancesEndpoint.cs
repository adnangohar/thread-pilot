using Insurance.Application.Queries.GetPersonInsurances;
using Insurance.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using ProblemDetails = FastEndpoints.ProblemDetails;
using IMapper = AutoMapper.IMapper;

namespace Insurance.Api.Endpoints;

public class GetPersonInsurancesEndpoint : Endpoint<GetPersonInsurancesRequest, Results<Ok<PersonInsurancesResponse>, NotFound<ProblemDetails>>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public GetPersonInsurancesEndpoint(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public override void Configure()
    {
        Post("/insurances/get-by-person");
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
        var query = new GetPersonInsurancesQuery(req.PersonalIdentificationNumber);
        var insurances = await _mediator.Send(query, ct);

        if (insurances == null)
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Status = 404,
                Detail = $"No insurances found for person with identification number {req.PersonalIdentificationNumber}",
                Instance = HttpContext.Request.Path
            });
        }

        return TypedResults.Ok(_mapper.Map<PersonInsurancesResponse>(insurances));
    }
}
