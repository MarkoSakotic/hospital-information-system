using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DtoEntityProject
{
    public class DoctorUpdate
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string Specialization { get; set; }
        [Required]
        [Range(1, 10)]
        public int HoursPerDay { get; set; }
    }
}
