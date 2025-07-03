using System;
using FastEndpoints;
using Insurance.Application.Interfaces;
using Insurance.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using ProblemDetails = FastEndpoints.ProblemDetails;

namespace Insurance.Api.Endpoints;

public class GetPersonInsurancesEndpoint : Endpoint<GetPersonInsurancesRequest, Results<Ok<PersonInsurancesDto>, NotFound<ProblemDetails>>>
{
    private readonly ILogger<GetPersonInsurancesEndpoint> _logger;
    private readonly IInsuranceService _insuranceService;

    public GetPersonInsurancesEndpoint(IInsuranceService insuranceService, ILogger<GetPersonInsurancesEndpoint> logger)
    {
        _insuranceService = insuranceService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/v1/insurances/{PersonalIdentificationNumber}");
        AllowAnonymous();
        Description(b => b
            .Produces<PersonInsurancesDto>(200, "application/json")
            .ProducesProblem(404)
            .ProducesProblemFE<InternalErrorResponse>(500)
            .WithTags("Insurances")
            .WithSummary("Get person's insurances")
            .WithDescription("Retrieves all insurances for a person by their personal identification number"));
    }
        
    public override async Task<Results<Ok<PersonInsurancesDto>, NotFound<ProblemDetails>>> ExecuteAsync(
        GetPersonInsurancesRequest req, 
        CancellationToken ct)
    {
        // using var activity = Activity.StartActivity("GetPersonInsurances");
        // activity?.SetTag("person.id", req.PersonalIdentificationNumber);
        
        _logger.LogInformation("Retrieving insurances for person {PersonalIdentificationNumber}", 
            req.PersonalIdentificationNumber);
        
        var insurances = await _insuranceService.GetPersonInsurancesAsync(req.PersonalIdentificationNumber);
        
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
            
        return TypedResults.Ok(insurances);
    }
}
