using System;
using System.Collections.Generic;
using System.Text;

namespace DtoEntityProject
{
    public class AppointmentResponse
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Boolean Completed { get; set; }
        public string DoctorId { get; set; }
        public DoctorResponse Doctor { get; set; }
        public string PatientId { get; set; }
        public PatientResponse Patient { get; set; }
    }
}
