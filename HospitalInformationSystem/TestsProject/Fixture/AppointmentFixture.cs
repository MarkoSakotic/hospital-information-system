using Bogus;
using EntityProject;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProject.Fixture
{
    public class AppointmentFixture : Faker<Appointment>
    {
        public AppointmentFixture()
        {
            RuleFor(p => p.Id, f => f.UniqueIndex);
            RuleFor(p => p.StartTime, f => f.Date.Soon());
            RuleFor(p => p.EndTime, f => f.Date.Soon());
            RuleFor(p => p.Note, f => f.Lorem.ToString());
            RuleFor(p => p.Date, f => DateTime.Now);
            RuleFor(p => p.PatientId, f => f.Random.String());
            RuleFor(p => p.Completed, f => f.Random.Bool());
            RuleFor(p => p.DoctorId, f => f.Random.String());
        }
    }
}
