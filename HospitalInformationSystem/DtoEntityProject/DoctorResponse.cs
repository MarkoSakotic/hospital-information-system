using System;
using System.Collections.Generic;
using System.Text;

namespace DtoEntityProject
{
    public class DoctorResponse : ResponseBase
    {
        public string Specialization { get; set; }
        public int HoursPerDay { get; set; }
    }
}
