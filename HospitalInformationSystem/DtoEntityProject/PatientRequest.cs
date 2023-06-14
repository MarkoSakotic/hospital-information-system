using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DtoEntityProject
{
    public class PatientRequest : RequestBase
    {
        [Required]
        public string SSN { get; set; }
    }
}
