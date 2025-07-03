using System;
using AutoMapper;
using Vehicle.Application.DTOs;
using Vehicle.Application.Interfaces;
using Vehicle.Domain.Repositories;
using Vehicle.Domain.ValueObjects;

namespace Vehicle.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;

    public VehicleService(IVehicleRepository vehicleRepository, IMapper mapper)
    {
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<VehicleDto?> GetVehicleByRegistrationNumberAsync(string registrationNumber)
    {
        try
        {
            var regNumber = new RegistrationNumber(registrationNumber);
            var vehicle = await _vehicleRepository.GetByRegistrationNumberAsync(regNumber);

            return vehicle == null ? null : _mapper.Map<VehicleDto>(vehicle);
        }
        catch (ArgumentException)
        {
            // Invalid registration number format
            return null;
        }
    }
}
