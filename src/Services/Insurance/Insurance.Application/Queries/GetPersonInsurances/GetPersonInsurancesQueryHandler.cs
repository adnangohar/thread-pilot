using MediatR;
using AutoMapper;
using Insurance.Application.Common;
using Insurance.Domain.Repositories;
using Insurance.Domain.ValueObjects;
using Insurance.Contracts;
using Insurance.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace Insurance.Application.Queries.GetPersonInsurances;

public class GetPersonInsurancesQueryHandler : IRequestHandler<GetPersonInsurancesQuery, PersonInsurancesResult?>
{
    private readonly IInsuranceRepository _insuranceRepository;
    private readonly IFeatureManager _featureManager;

    private readonly IVehicleService _vehicleService;
    private readonly IMapper _mapper;
    private ILogger<GetPersonInsurancesQueryHandler> _logger;

    public const string EnableDetailedVehicleInfo = "EnableDetailedVehicleInfo";

    public GetPersonInsurancesQueryHandler(
        IInsuranceRepository insuranceRepository,
        IVehicleService vehicleService,
        IMapper mapper,
        ILogger<GetPersonInsurancesQueryHandler> logger,
        IFeatureManager featureManager)
    {
        _insuranceRepository = insuranceRepository ?? throw new ArgumentNullException(nameof(insuranceRepository));
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
        _featureManager = featureManager;
    }

    public async Task<PersonInsurancesResult?> Handle(GetPersonInsurancesQuery request, CancellationToken cancellationToken)
    {
        try
        {
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
                var insuranceResponse = _mapper.Map<InsuranceResponse>(insurance);
                
                // If it's car insurance, fetch vehicle details
                if (insurance.Type == Domain.Enums.InsuranceType.Car && !string.IsNullOrEmpty(insurance.VehicleRegistrationNumber))
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
