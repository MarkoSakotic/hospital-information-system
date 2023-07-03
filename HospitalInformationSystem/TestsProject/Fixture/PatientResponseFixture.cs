using Bogus;
using DtoEntityProject;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProject.Fixture
{
    public class PatientResponseFixture : Faker<PatientResponse>
    {
        public PatientResponseFixture()
        {
            RuleFor(p => p.FirstName, f => f.Person.FirstName);
            RuleFor(p => p.LastName, f => f.Person.LastName);
            RuleFor(p => p.Address, f => f.Address.StreetAddress());
            RuleFor(p => p.DateOfBirth, f => f.Person.DateOfBirth);
            RuleFor(p => p.Email, f => f.Person.Email);
            RuleFor(p => p.Phone, f => f.Person.Phone);
            RuleFor(p => p.SSN, f => f.Person.Random.ToString());
            RuleFor(p => p.Id, f => f.UniqueIndex.ToString());
        }
    }
}
