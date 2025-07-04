using MediatR;
using Vehicle.Application.Common;

namespace Vehicle.Application.Queries.GetVehicle;

public record GetVehicleByRegistrationNumberQuery(string RegistrationNumber) : IRequest<VehicleResult?>;
