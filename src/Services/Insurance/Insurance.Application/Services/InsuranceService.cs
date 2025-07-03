using AutoMapper;
using Insurance.Application.Interfaces;
using Insurance.Contracts;
using Insurance.Domain.Entities;
using Insurance.Domain.Repositories;
using Insurance.Domain.ValueObjects;

namespace Insurance.Application.Services;

public class InsuranceService : IInsuranceService
{
    private readonly IInsuranceRepository _insuranceRepository;
    private readonly IVehicleService _vehicleService;
    private readonly IMapper _mapper;

    public InsuranceService(
        IInsuranceRepository insuranceRepository,
        IVehicleService vehicleService,
        IMapper mapper)
    {
        _insuranceRepository = insuranceRepository ?? throw new ArgumentNullException(nameof(insuranceRepository));
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PersonInsurancesDto?> GetPersonInsurancesAsync(string personalIdentificationNumber)
    {
        try
        {
            var pin = new PersonalIdentificationNumber(personalIdentificationNumber);
            var insurances = await _insuranceRepository.GetByOwnerAsync(pin);

            var insurancesList = insurances.ToList();
            if (!insurancesList.Any())
            {
                return null;
            }

            var result = new PersonInsurancesDto
            {
                PersonalIdentificationNumber = personalIdentificationNumber,
                Insurances = new List<InsuranceDto>()
            };

            foreach (var insurance in insurancesList)
            {
                var dto = await MapInsuranceToDto(insurance);
                if (dto != null)
                {
                    result.Insurances.Add(dto);
                }
            }

            result.TotalMonthlyCost = result.Insurances.Sum(i => i.MonthlyCost);

            return result;
        }
        catch (ArgumentException ex)
        {
            // _logger.LogWarning("Invalid personal identification number: {Message}", ex.Message);
            return null;
        }
    }
    
    private async Task<InsuranceDto?> MapInsuranceToDto(Domain.Entities.Insurance insurance)
    {
        switch (insurance)
        {
            case CarInsurance carInsurance:
                var carDto = _mapper.Map<CarInsuranceDto>(carInsurance);
                try
                {
                    carDto.Vehicle = await _vehicleService.GetVehicleInfoAsync(carInsurance.VehicleRegistrationNumber);
                }
                catch (Exception ex)
                {
                    // _logger.LogError(ex, "Failed to fetch vehicle info for registration {RegistrationNumber}", 
                    //     carInsurance.VehicleRegistrationNumber);
                }
                return carDto;
                
            case PetInsurance petInsurance:
                return _mapper.Map<PetInsuranceDto>(petInsurance);
                
            case PersonalHealthInsurance healthInsurance:
                return _mapper.Map<PersonalHealthInsuranceDto>(healthInsurance);
                
            default:
                // _logger.LogWarning("Unknown insurance type: {Type}", insurance.GetType().Name);
                return null;
        }
    }
}
