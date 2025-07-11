using Vehicle.Core.ValueObjects;

namespace Vehicle.Core.Entities;

 public class Vehicle
    {
        public Guid Id { get; private set; }
        public string RegistrationNumber { get; private set; }
        public string Make { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }
        public string Color { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }


    protected Vehicle() { } // EF Core
    public Vehicle(RegistrationNumber registrationNumber, string make, string model, int year, string color)
    {
        Id = Guid.NewGuid();
        RegistrationNumber = registrationNumber.Value ?? throw new ArgumentNullException(nameof(registrationNumber));
        Make = make ?? throw new ArgumentNullException(nameof(make));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Color = color ?? throw new ArgumentNullException(nameof(color));

        if (year < 1900 || year > DateTime.Now.Year + 1)
            throw new ArgumentException($"Invalid year: {year}", nameof(year));

        Year = year;
        CreatedAt = DateTime.UtcNow;
    }
}