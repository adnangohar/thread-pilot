using System;
using Vehicle.Domain.ValueObjects;

namespace Vehicle.Domain.Entities;

 public class Vehicle
    {
        public RegistrationNumber RegistrationNumber { get; private set; }
        public string Make { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }
        public string Color { get; private set; }
        
        public Vehicle(string registrationNumber, string make, string model, int year, string color)
        {
            RegistrationNumber = new RegistrationNumber(registrationNumber);
            Make = make ?? throw new ArgumentNullException(nameof(make));
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Color = color ?? throw new ArgumentNullException(nameof(color));
            
            if (year < 1900 || year > DateTime.Now.Year + 1)
                throw new ArgumentException($"Invalid year: {year}", nameof(year));
                
            Year = year;
        }
    }