using FluentValidation;
using Insurance.Core.Common;
using Insurance.Core.Repositories;
using Insurance.Core.ValueObjects;
using Insurance.Core.Interfaces;
using Insurance.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace Insurance.Core.Queries.GetPersonInsurances;

public class GetPersonInsurancesQueryHandler : IGetPersonInsurancesQueryHandler
{
    private readonly IInsuranceRepository _insuranceRepository;
    private readonly IFeatureManager _featureManager;
    private readonly IVehicleService _vehicleService;
    private readonly IValidator<GetPersonInsurancesQuery> _validator;
    private readonly ILogger<GetPersonInsurancesQueryHandler> _logger;

    public const string EnableDetailedVehicleInfo = "EnableDetailedVehicleInfo";

    public GetPersonInsurancesQueryHandler(
        IInsuranceRepository insuranceRepository,
        IVehicleService vehicleService,
        IValidator<GetPersonInsurancesQuery> validator,
        ILogger<GetPersonInsurancesQueryHandler> logger,
        IFeatureManager featureManager)
    {
        _insuranceRepository = insuranceRepository ?? throw new ArgumentNullException(nameof(insuranceRepository));
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
    }

    public async Task<PersonInsurancesResult?> Handle(GetPersonInsurancesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the request
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for GetPersonInsurancesQuery: {Errors}", validationResult.Errors);
                return null;
            }

            var pin = new PersonalIdentificationNumber(request.PersonalIdentificationNumber);
            var insurances = await _insuranceRepository.GetByPersonalIdAsync(pin, cancellationToken);
            var insurancesList = insurances.ToList();
            if (!insurancesList.Any())
            {
                _logger.LogWarning("No insurances found for person: {PersonalIdentificationNumber}", request.PersonalIdentificationNumber);
                return new PersonInsurancesResult
                {
                    PersonalIdentificationNumber = request.PersonalIdentificationNumber,
                    Insurances = [],
                    TotalMonthlyCost = 0
                };
            }

            var personInsurancesResult = new PersonInsurancesResult
            {
                PersonalIdentificationNumber = request.PersonalIdentificationNumber,
                Insurances = new List<InsuranceResponse>()
            };


            foreach (var insurance in insurances)
            {
                var insuranceResponse = insurance.ToResponse();
                
                // If it's car insurance, fetch vehicle details
                if (insurance.Type == Insurance.Core.Enums.InsuranceType.Car && !string.IsNullOrEmpty(insurance.VehicleRegistrationNumber))
                {
                    if (await _featureManager.IsEnabledAsync(EnableDetailedVehicleInfo, cancellationToken))
                    {

                        try
                        {
                            var vehicleInfo = await _vehicleService.GetVehicleInfoAsync(insurance.VehicleRegistrationNumber, cancellationToken);
                            if (vehicleInfo != null)
                            {
                                insuranceResponse.VehicleInfo = vehicleInfo;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error fetching vehicle info for registration number: {VehicleRegistrationNumber}", insurance.VehicleRegistrationNumber);
                        }
                    }
                }
                
                personInsurancesResult.Insurances.Add(insuranceResponse);
            }

            personInsurancesResult.TotalMonthlyCost = personInsurancesResult.Insurances.Sum(i => i.MonthlyCost);
            return personInsurancesResult;
        }
        catch (Exception)
        {
            _logger.LogError("An error occurred while processing the GetPersonInsurancesQuery for {PersonalIdentificationNumber}", request.PersonalIdentificationNumber);
            return null;
        }
    }
}
