using System;
using System.Collections.Generic;
using System.Text;

namespace EntityProject
{
    public class Appointment
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Boolean Completed { get; set; }
        public string DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        public string PatientId { get; set; }
        public Patient Patient { get; set; }
    }
}
