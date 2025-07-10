using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using ProblemDetails = FastEndpoints.ProblemDetails;
using FluentValidation;
using Insurance.Api.Contracts;
using Insurance.Core.Common;
using Insurance.Core.Queries.GetPersonInsurances;

namespace Insurance.Api.Endpoints;

public class GetPersonInsurancesEndpoint : Endpoint<GetPersonInsurancesRequest, Results<Ok<PersonInsurancesResult>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>>
{
    private readonly IMediator _mediator;
    private readonly IValidator<GetPersonInsurancesRequest> _validator;

    public GetPersonInsurancesEndpoint(IMediator mediator, IValidator<GetPersonInsurancesRequest> validator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _validator = validator;
    }

    public override void Configure()
    {
        Post("/insurances");
        AllowAnonymous();
        Description(b => b
            .Produces<PersonInsurancesResult>(200, "application/json")
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblemFE<InternalErrorResponse>(500)
            .WithTags("Insurances")
            .WithSummary("Get person's insurances")
            .WithDescription("Retrieves all insurances for a person by their personal identification number (sent in the request body)"));
    }
        
    public override async Task<Results<Ok<PersonInsurancesResult>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>> ExecuteAsync(
        GetPersonInsurancesRequest req, 
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(req, cancellationToken);

         if (!validationResult.IsValid)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Status = 400,
                Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                Instance = HttpContext.Request.Path
            });
        }
        
        var query = new GetPersonInsurancesQuery(req.PersonalIdentificationNumber);
        var insurances = await _mediator.Send(query, cancellationToken);

        if (insurances == null)
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Status = 404,
                Detail = $"No insurances found for person with identification number {req.PersonalIdentificationNumber}",
                Instance = HttpContext.Request.Path
            });
        }

        return TypedResults.Ok(insurances);
    }
}
