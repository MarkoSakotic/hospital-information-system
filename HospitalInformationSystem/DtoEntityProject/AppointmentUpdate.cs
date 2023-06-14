using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DtoEntityProject
{
    public class AppointmentUpdate
    {
        [Required]
        public int Id { get; set; }
        public string Note { get; set; }
        public string PatientId { get; set; }
    }
}
