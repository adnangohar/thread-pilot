namespace Vehicle.Application.Common;

public record VehicleResult(
    string RegistrationNumber = "",
    string Make = "",
    string Model = "",
    int Year = 0,
    string Color = ""
);
