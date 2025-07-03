// using System;
// using Insurance.Domain.Entities;
// using Insurance.Domain.Repositories;
// using Insurance.Domain.ValueObjects;

// namespace Insurance.Infrastructure.Repositories;

// public class InMemoryInsuranceRepository : IInsuranceRepository
// {
//     private readonly List<Domain.Entities.Insurance> _insurances;
//     public InMemoryInsuranceRepository()
//     {
//         _insurances = new List<Domain.Entities.Insurance>();
//         // Seed data
//         var person1 = new PersonalIdentificationNumber("123456789");
//         var person2 = new PersonalIdentificationNumber("987654321");
//         var person3 = new PersonalIdentificationNumber("555555555");
        
//         // Person 1 has all types of insurance
//         _insurances.Add(new CarInsurance(person1, "ABC123"));
//         _insurances.Add(new PetInsurance(person1, "Fluffy", "Cat"));
//         _insurances.Add(new PersonalHealthInsurance(person1, "Premium"));
        
//         // Person 2 has only pet and health insurance
//         _insurances.Add(new PetInsurance(person2, "Rex", "Dog"));
//         _insurances.Add(new PersonalHealthInsurance(person2, "Basic"));
        
//         // Person 3 has only car insurance
//         _insurances.Add(new CarInsurance(person3, "XYZ789"));
//     }
//     public async Task AddAsync(Domain.Entities.Insurance insurance,CancellationToken cancellationToken)
//     {
//        _insurances.Add(insurance);
//         await Task.CompletedTask;
//     }

//     public async Task<IEnumerable<Domain.Entities.Insurance>> GetByPersonalIdAsync(PersonalIdentificationNumber owner, CancellationToken cancellationToken)
//     {
//         var insurances = _insurances.Where(i => i.Owner.Value == owner.Value);
//         return await Task.FromResult(insurances);
//     }
// }
