using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DtoEntityProject
{
    public class DoctorRequest : RequestBase
    {
        [Required]
        public string Specialization { get; set; }
        [Required]
        [Range(1, 10)]
        public int HoursPerDay { get; set; }
    }
}
