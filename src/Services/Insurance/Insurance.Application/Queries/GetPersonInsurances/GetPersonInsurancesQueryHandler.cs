using MediatR;
using AutoMapper;
using Insurance.Application.Common;
using Insurance.Domain.Repositories;
using Insurance.Domain.ValueObjects;
using Insurance.Contracts;
using Insurance.Domain.Entities;
using Insurance.Application.Interfaces;

namespace Insurance.Application.Queries.GetPersonInsurances;

public class GetPersonInsurancesQueryHandler : IRequestHandler<GetPersonInsurancesQuery, PersonInsurancesResult?>
{
    private readonly IInsuranceRepository _insuranceRepository;
    private readonly IVehicleService _vehicleService;
    private readonly IMapper _mapper;

    public GetPersonInsurancesQueryHandler(
        IInsuranceRepository insuranceRepository,
        IVehicleService vehicleService,
        IMapper mapper)
    {
        _insuranceRepository = insuranceRepository ?? throw new ArgumentNullException(nameof(insuranceRepository));
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PersonInsurancesResult?> Handle(GetPersonInsurancesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pin = new PersonalIdentificationNumber(request.PersonalIdentificationNumber);
            var insurances = await _insuranceRepository.GetByOwnerAsync(pin);
            var insurancesList = insurances.ToList();
            if (!insurancesList.Any())
            {
                return null;
            }

            var result = new PersonInsurancesResult
            {
                PersonalIdentificationNumber = request.PersonalIdentificationNumber,
                Insurances = new List<InsuranceResponse>()
            };

            foreach (var insurance in insurancesList)
            {
                var contract = await MapInsuranceToContract(insurance, cancellationToken);
                if (contract != null)
                {
                    result.Insurances.Add(contract);
                }
            }

            result.TotalMonthlyCost = result.Insurances.Sum(i => i.MonthlyCost);
            return result;
        }
        catch (ArgumentException)
        {
            // Optionally log warning
            return null;
        }
    }

    private async Task<InsuranceResponse?> MapInsuranceToContract(Insurance.Domain.Entities.Insurance insurance, CancellationToken cancellationToken)
    {
        switch (insurance)
        {
            case CarInsurance carInsurance:
                var carContract = _mapper.Map<CarInsuranceResponse>(carInsurance);
                try
                {
                    carContract.Vehicle = await _vehicleService.GetVehicleInfoAsync(carInsurance.VehicleRegistrationNumber, cancellationToken);
                }
                catch (Exception)
                {
                    // Optionally log error
                }
                return carContract;
            case PetInsurance petInsurance:
                return _mapper.Map<PetInsuranceResponse>(petInsurance);
            case PersonalHealthInsurance healthInsurance:
                return _mapper.Map<PersonalHealthInsuranceResponse>(healthInsurance);
            default:
                // Optionally log warning
                return null;
        }
    }
}
