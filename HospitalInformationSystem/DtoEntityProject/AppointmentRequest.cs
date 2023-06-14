using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DtoEntityProject
{
    public class AppointmentRequest
    {
        [Required]
        public string DoctorId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        [Required]
        [Range(30, 60)]
        public int Duration { get; set; }
        [Required]
        [Range(1, 10)]
        public int HoursPerDay { get; set; }
    }
}
