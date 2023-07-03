using Bogus;
using EntityProject;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProject.Fixture
{
    public class PatientFixture : Faker<Patient>
    {
        public PatientFixture()
        {
            RuleFor(p => p.FirstName, f => f.Person.FirstName);
            RuleFor(p => p.LastName, f => f.Person.LastName);
            RuleFor(p => p.Address, f => f.Address.StreetAddress());
            RuleFor(p => p.DateOfBirth, f => f.Person.DateOfBirth);
            RuleFor(p => p.Email, f => f.Person.Email);
            RuleFor(p => p.Phone, f => f.Person.Phone);
            RuleFor(p => p.SSN, f => f.Person.Random.ToString());
            RuleFor(p => p.Id, f => f.UniqueIndex.ToString());
            RuleFor(p => p.Password, f => "P@ssw0rd");
        }
    }
}
