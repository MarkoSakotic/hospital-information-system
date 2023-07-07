using AutoMapper;
using DtoEntityProject;
using DtoEntityProject.Constants;
using EntityProject;
using Microsoft.EntityFrameworkCore;
using RepositoryProject.Context;
using ServiceProject.Interface;
using ServiceProject.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServiceProject.Implementation
{
    public class AppointmentService : IAppointmentService
    {
        private readonly HisContext _context;
        private readonly IMapper _mapper;
        private readonly JwtParser _jwtParser;

        public AppointmentService(HisContext context, IMapper mapper, JwtParser jwtParser)
        {
            _context = context;
            _mapper = mapper;
            _jwtParser = jwtParser;
        }

        public async Task<ApiResponse> CreateAppointmentsAsync(AppointmentRequest appointmentRequest)
        {
            var response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());

            if (appointmentRequest.HoursPerDay <= 0)
            {
                response.Errors.Add("Invalid number of Hours Per Day.");
                return response;
            }
            if (appointmentRequest.StartDate < DateTime.UtcNow.Date)
            {
                response.Errors.Add("Start date cannot be in past.");
                return response;
            }
            if (appointmentRequest.EndDate < appointmentRequest.StartDate)
            {
                response.Errors.Add("End date cannot be before start date.");
                return response;
            }

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

            if (!appointments.Any())
            {
                response.Errors.Add("There is no appointments in database.");
                return response;
            }
            response.Result = _mapper.Map<IEnumerable<AppointmentResponse>>(appointments);
            return response;
        }


        public async Task<ApiResponse> UpdateAppointmentAsync(AppointmentUpdate appointmentUpdate)
        {
            var response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());
            var appointment = await _context.Appointments
                .Where(d => d.Id == appointmentUpdate.Id)
                .FirstOrDefaultAsync();
            if (appointmentUpdate.Note == null)
                appointmentUpdate.Note = "";

            if (appointment == null)
            {
                response.Errors.Add("Unable to update appointment because it doesn't exists.");
                return response;
            }

            if (appointmentUpdate.PatientId == null)
            {
                appointment.PatientId = null;
                appointment.Patient = null;
                appointment.Note = appointmentUpdate.Note;
            }
            else if (appointmentUpdate.PatientId == "" || Regex.Matches(appointmentUpdate.PatientId, @"^[a-zA-Z0-9_.-]*$").Count == 0)
            {
                appointment.Note = appointmentUpdate.Note;
            }
            else
            {
                var patient = await _context.Patients
                .Where(d => d.Id == appointmentUpdate.PatientId)
                .FirstOrDefaultAsync();
                if (patient == null)
                {
                    response.Errors.Add("Unable to update patient in appointment because patient doesn't exists.");
                    return response;
                }
                _mapper.Map(appointmentUpdate, appointment);
            }

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            response.Result = _mapper.Map<AppointmentResponse>(appointment);
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

        public async Task<ApiResponse> ScheduleAppointmentAsync(AppointmentSchedule appointmentSchedule)
        {
            var response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());

            var appointment = await _context.Appointments
                .Where(a => a.Id == appointmentSchedule.AppointmentId)
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                response.Errors.Add("Unable to schedule appointment because it doesn't exists.");
                return response;
            }
            if (appointment.Note == "Coffee Break" && appointmentSchedule.Note == "Coffee Break")
            {
                response.Errors.Add("You cannot schedule this appointment because doctor is on coffee break as usual.");
                return response;
            }
            if (appointmentSchedule.PatientId == "" || appointmentSchedule.PatientId == null)
            {
                response.Errors.Add("Unable to schedule appointment because patient is not selected.");
                return response;
            }
            else
            {
                var patient = await _context.Patients
                    .Where(d => d.Id == appointmentSchedule.PatientId)
                    .FirstOrDefaultAsync();

                if (appointmentSchedule.Note == null)
                    appointmentSchedule.Note = "";

                if (patient == null)
                {
                    response.Errors.Add("Unable to schedule appointment because patient does not exists.");
                    return response;
                }
                appointment.PatientId = appointmentSchedule.PatientId;
                appointment.Note = appointmentSchedule.Note;
                _context.Update(appointment);
                await _context.SaveChangesAsync();

                response.Result = _mapper.Map<AppointmentResponse>(appointment);
                return response;
            }
        }

        public async Task<ApiResponse> CompleteAppointmentAsync(AppointmentComplete appointmentComplete)
        {
            var response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());

            var doctorId = _jwtParser.GetIdFromJWT();
            Appointment appointment = await _context.Appointments
                .Where(a => a.Id == appointmentComplete.AppointmentId)
                .FirstOrDefaultAsync();

            if (appointmentComplete.Note == null)
                appointmentComplete.Note = "";

            if (appointment == null)
            {
                response.Errors.Add("Database does not contain appointment with that id.");
                return response;
            }
            else if (appointment.DoctorId != doctorId)
            {
                response.Errors.Add("You can't complete this appointment because it's not yours.");
                return response;
            }
            else
            {
                appointment.Completed = true;
                appointment.Note = appointmentComplete.Note;

                _context.Update(appointment);
                await _context.SaveChangesAsync();

                response.Result = _mapper.Map<Appointment, AppointmentResponse>(appointment);
                return response;
            }
        }

        public async Task<ApiResponse> GetAllAppointmentsForUserWithinGivenPeriodAsync(AppointmentFilter appointmentFilter)
        {
            var response = new ApiResponse();
            var role = _jwtParser.GetRoleFromJWT();
            if (role == "")
            {
                response.Errors.Add("There is no needed role for given user.");
                return response;
            }
            response.Roles.Add(role);
            if (appointmentFilter.StartDate > appointmentFilter.EndDate)
            {
                response.Errors.Add("Start date can't be after end date.");
                return response;
            }

            List<Appointment> appointments = new List<Appointment>();

            if (role == Constants.Doctor || role == Constants.Technician)
            {
                if (appointmentFilter.UserId != null)
                {
                    appointments = await _context.Appointments
                           .Where(a => a.DoctorId == appointmentFilter.UserId
                               && a.StartTime.CompareTo(appointmentFilter.StartDate) >= 0
                               && a.StartTime.Date.CompareTo(appointmentFilter.EndDate) <= 0)
                           .Include(a => a.Patient)
                           .Include(a => a.Doctor)
                           .ToListAsync();
                }
                else
                {
                    appointments = await _context.Appointments
                           .Where(a => a.StartTime.CompareTo(appointmentFilter.StartDate) >= 0
                               && a.StartTime.Date.CompareTo(appointmentFilter.EndDate.Date) <= 0)
                           .Include(a => a.Patient)
                           .Include(a => a.Doctor)
                           .ToListAsync();
                }
            }

            if (role == Constants.Patient)
            {
                appointments = await _context.Appointments
                       .Where(a => a.PatientId == appointmentFilter.UserId
                           && a.StartTime.CompareTo(appointmentFilter.StartDate) >= 0
                           && a.StartTime.Date.CompareTo(appointmentFilter.EndDate.Date) <= 0)
                       .Include(a => a.Patient)
                       .Include(a => a.Doctor)
                       .ToListAsync();
            }

            if (!appointments.Any())
            {
                response.Errors.Add("There is no appointments for given filters.");
                return response;
            }

            response.Result = _mapper.Map<IEnumerable<AppointmentResponse>>(appointments);
            return response;
        }
    }
}
