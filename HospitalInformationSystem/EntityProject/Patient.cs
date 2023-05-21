using System;
using System.Collections.Generic;
using System.Text;

namespace EntityProject
{
    public class Patient : ApiUser
    {
        public string SSN { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
