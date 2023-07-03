using Bogus;
using DtoEntityProject;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProject.Fixture
{
    public class AppointmentFilterFixture : Faker<AppointmentFilter>
    {
        public AppointmentFilterFixture()
        {
            RuleFor(p => p.UserId, f => f.Random.String());
            RuleFor(p => p.StartDate, f => f.Date.Past());
            RuleFor(p => p.EndDate, f => f.Date.Future());
        }
    }
}
