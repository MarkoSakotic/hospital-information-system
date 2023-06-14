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
        public Task<ApiResponse> CreateAppointmentsAsync(AppointmentRequest appointmentRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> DeleteAppointmentAsync(int id)
        {
            throw new NotImplementedException();
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
