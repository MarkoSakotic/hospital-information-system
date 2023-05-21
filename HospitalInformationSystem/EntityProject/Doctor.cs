using System;
using System.Collections.Generic;
using System.Text;

namespace EntityProject
{
    public class Doctor : ApiUser
    {
        public string Specialization { get; set; }
        public int HoursPerDay { get; set; }
        public ICollection<Appointment> Appointments { get; set; }

    }
}
