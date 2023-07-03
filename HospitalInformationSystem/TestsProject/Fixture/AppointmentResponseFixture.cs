using Bogus;
using DtoEntityProject;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProject.Fixture
{
    public class AppointmentResponseFixture : Faker<AppointmentResponse>
    {
        public AppointmentResponseFixture()
        {
            RuleFor(p => p.Id, f => f.UniqueIndex);
            RuleFor(p => p.StartTime, f => f.Date.Past());
            RuleFor(p => p.EndTime, f => f.Date.Future());
            RuleFor(p => p.Date, f => DateTime.Now);
            RuleFor(p => p.Note, f => f.Lorem.ToString());
            RuleFor(p => p.PatientId, f => f.Random.String());
            RuleFor(p => p.Completed, f => f.Random.Bool());
            RuleFor(p => p.DoctorId, f => f.Random.String());
        }
    }
}
