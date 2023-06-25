using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DtoEntityProject
{
    public class AppointmentComplete
    {
        [Required]
        public int AppointmentId { get; set; }
        public string Note { get; set; }
    }
}
