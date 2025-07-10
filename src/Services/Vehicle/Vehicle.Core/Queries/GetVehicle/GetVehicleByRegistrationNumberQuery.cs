using MediatR;
using Vehicle.Core.Common;

namespace Vehicle.Core.Queries.GetVehicle;

public record GetVehicleByRegistrationNumberQuery(string RegistrationNumber) : IRequest<VehicleResult?>;
