using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DtoEntityProject
{
    public class AppointmentSchedule
    {
        [Required]
        public int AppointmentId { get; set; }
        [Required]
        public string PatientId { get; set; }
        public string Note { get; set; }
    }
}
