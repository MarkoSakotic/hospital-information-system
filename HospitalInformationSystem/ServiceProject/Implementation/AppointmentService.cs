using AutoMapper;
using DtoEntityProject;
using EntityProject;
using Microsoft.EntityFrameworkCore;
using RepositoryProject.Context;
using ServiceProject.Interface;
using ServiceProject.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProject.Implementation
{
    public class AppointmentService : IAppointmentService
    {
        private readonly HISContext _context;
        private readonly IMapper _mapper;
        private readonly JwtParser _jwtParser;

        public AppointmentService(HISContext context, IMapper mapper, JwtParser jwtParser)
        {
            _context = context;
            _mapper = mapper;
            _jwtParser = jwtParser;
        }
        public async Task<ApiResponse> CreateAppointmentsAsync(AppointmentRequest appointmentRequest)
        {
            var response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());

            var doctor = await _context.Doctors
                .Where(d => d.Id == appointmentRequest.DoctorId)
                .FirstOrDefaultAsync();

            if (doctor == null)
            {
                response.Errors.Add("Unable to create appointments because doctor doesn't exists.");
                return response;
            }

            var appointments = new List<Appointment>();
            var workingTime = appointmentRequest.HoursPerDay * 60;
            var dateTemp = appointmentRequest.StartDate.AddHours(8);
            var compareStart = appointmentRequest.StartDate;
            var existingAppointments = await _context.Appointments.Where(a => a.DoctorId == appointmentRequest.DoctorId).Include(a => a.Patient).ToListAsync();
            var i = 1;
            var coffeeBreak = 0;
            while (compareStart <= appointmentRequest.EndDate)
            {
                if (!(existingAppointments.Where(a => a.StartTime == dateTemp).Any()))
                {
                    var appointment = new Appointment();
                    appointment.StartTime = dateTemp;
                    appointment.EndTime = dateTemp.AddMinutes(appointmentRequest.Duration);
                    appointment.Date = DateTime.Now;
                    if (coffeeBreak == 2)
                    {
                        appointment.Note = "Coffee Break";
                        coffeeBreak = 0;
                    }
                    else
                    {
                        appointment.Note = "";
                        coffeeBreak++;
                    }
                    appointment.DoctorId = appointmentRequest.DoctorId;
                    await _context.AddAsync(appointment);
                    appointments.Add(appointment);
                }
                else
                {
                    var existingApp = existingAppointments.Where(a => a.StartTime == dateTemp).FirstOrDefault();
                    if (coffeeBreak == 2 && existingApp.PatientId == null)
                    {
                        existingApp.Note = "Coffee Break";
                        coffeeBreak = 0;
                        _context.Update(existingApp);
                    }
                    else
                    {
                        coffeeBreak++;
                    }
                    appointments.Add(existingApp);
                    response.Errors.Add($"Couldn't add appointment with id: {existingApp.Id} and startTime: {dateTemp} because another appointment is already created with same start time.");
                }
                workingTime -= appointmentRequest.Duration;
                dateTemp = dateTemp.AddMinutes(appointmentRequest.Duration);
                if (workingTime == 0)
                {
                    workingTime = appointmentRequest.HoursPerDay * 60;
                    compareStart = appointmentRequest.StartDate.AddDays(i);
                    dateTemp = compareStart.AddHours(8);
                    coffeeBreak = 0;
                    i++;
                }
            }
            if (appointments.Count == 0)
            {
                response.Errors.Add($"Unable to create appointments for doctor with id: {appointmentRequest.DoctorId}");
                return response;
            }

            await _context.SaveChangesAsync();

            response.Result = _mapper.Map<IEnumerable<AppointmentResponse>>(appointments);
            return response;

        }

        public async Task<ApiResponse> DeleteAppointmentAsync(int id)
        {
            var response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());
            Appointment a = await _context.Appointments.FindAsync(id);

            if (a == null)
            {
                response.Errors.Add("Unable to delete appointment, because it doesn't exists.");
                return response;
            }

            _context.Appointments.Remove(a);
            await _context.SaveChangesAsync();

            response.Result = $"Appointment with id: {id} is deleted successfully.";
            return response;
        }

        public async Task<ApiResponse> GetAllAppointmentsAsync()
        {
            var response = new ApiResponse();
            string role = _jwtParser.GetRoleFromJWT();
            string userId = _jwtParser.GetIdFromJWT();
            response.Roles.Add(role);
            IEnumerable<Appointment> appointments;
            if (role.Equals(Constants.Technician))
            {
                appointments = await _context.Appointments
                    .Where(a => a.DoctorId != null)
                    .Include(b => b.Doctor)
                    .Include(b => b.Patient)
                    .ToListAsync();
            }
            else if (role.Equals(Constants.Doctor))
            {
                appointments = await _context.Appointments
                    .Where(a => a.Doctor != null && a.DoctorId == userId && a.StartTime.Date.AddDays(1) >= DateTime.Now.Date)
                    .Include(b => b.Doctor)
                    .Include(b => b.Patient)
                    .ToListAsync();
            }
            else
            {
                appointments = await _context.Appointments
                   .Where(a => a.PatientId == userId && a.DoctorId != null)
                   .Include(b => b.Doctor)
                   .Include(b => b.Patient)
                   .ToListAsync();
            }

            if (appointments.Count() == 0)
            {
                response.Errors.Add("There is no appointments in database.");
                return response;
            }
            response.Result = _mapper.Map<IEnumerable<AppointmentResponse>>(appointments);
            return response;
        }

        public async Task<ApiResponse> GetAppointmentAsync(int id)
        {
            var response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());

            Appointment a = await _context.Appointments
                .Include(b => b.Doctor)
                .Include(b => b.Patient)
                .Where(b => b.Id == id)
                .FirstOrDefaultAsync();

            if (a == null)
            {
                response.Errors.Add("Unable to get appointment, because it doesn't exists.");
                return response;
            }

            response.Result = _mapper.Map<AppointmentResponse>(a);
            return response;

        }


        public Task<ApiResponse> UpdateAppointmentAsync(AppointmentUpdate appointmentUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
