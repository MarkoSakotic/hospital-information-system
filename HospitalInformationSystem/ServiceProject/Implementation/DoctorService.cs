using AutoMapper;
using DtoEntityProject;
using EntityProject;
using Microsoft.AspNetCore.Identity;
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
    public class DoctorService : IDoctorService
    {
        private readonly HISContext _context;
        private readonly UserManager<ApiUser> _userManager;
        private IMapper _mapper;
        private readonly JwtParser _jwtParser;

        public DoctorService(HISContext context, IMapper mapper, UserManager<ApiUser> userManager, JwtParser jwtParser)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _jwtParser = jwtParser;
        }

        public async Task<ApiResponse> AddAsync(DoctorRequest request)
        {
            ApiResponse response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());

            if (request.DateOfBirth > DateTime.Now)
            {
                response.Errors.Add("Date of birth cannot be in the future.");
                return response;
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                response.Errors.Add("Doctor alredy exists");
                return response;
            }

            Doctor doctor = _mapper.Map<Doctor>(request);
            doctor.UserName = request.Email;

            var newUser = await _userManager.CreateAsync(doctor, request.Password);
            if (!newUser.Succeeded)
            {
                response.Errors = newUser.Errors.Select(s => s.Description).ToList();
                return response;
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            var rolesResult = await _userManager.AddToRoleAsync(user, Constants.Doctor);

            if (!rolesResult.Succeeded)
            {
                response.Errors = newUser.Errors.Select(s => s.Description).ToList();
                return response;
            }

            response.Result = _mapper.Map<DoctorResponse>(user);
            return response;
        }

        public async Task<ApiResponse> DeleteAsync(string id)
        {
            ApiResponse response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());

            var doctor = await _context.Doctors
                .Include(p => p.Appointments)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (doctor == null)
            {
                response.Errors.Add("Unable to delete Doctor because it doesn't exists.");
                return response;
            }

            _context.Appointments.RemoveRange(_context.Appointments.Where(a => a.DoctorId == id && a.Completed == false));
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            response.Result = $"Doctor with id: [{id}] is deleted successfully.";
            return response;
        }

        public async Task<ApiResponse> GetAllAsync()
        {
            ApiResponse response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());

            var doctors = await _context.Doctors.ToListAsync();

            if (doctors.Count() == 0)
            {
                response.Errors.Add("There is no Doctors in databse.");
                return response;
            }

            response.Result = _mapper.Map<IEnumerable<DoctorResponse>>(doctors);
            return response;
        }

        public async Task<ApiResponse> GetAsync(string id)
        {
            ApiResponse response = new ApiResponse();
            var role = _jwtParser.GetRoleFromJWT();
            response.Roles.Add(role);
            var exists = false;
            string patientId = null;
            if (role == Constants.Patient)
            {
                patientId = _jwtParser.GetIdFromJWT();
                exists = await _context.Appointments
                    .Where(a => a.DoctorId == id && a.PatientId == patientId)
                    .AnyAsync();
                if (!exists)
                {
                    response.Errors.Add($"Unable to preview Doctor's details because user with id {patientId} is not Doctor's patient");
                    return response;
                }
            }

            Doctor doctor = await _context.Doctors
              .Where(d => d.Id == id)
              .FirstOrDefaultAsync();

            if (doctor == null)
            {
                response.Errors.Add("Unable to get Doctor because it doesn't exists.");
                return response;
            }

            DoctorResponse doctorResponse = _mapper.Map<DoctorResponse>(doctor);
            response.Result = doctorResponse;
            return response;
        }

        public async Task<ApiResponse> GetDoctorsByPatientAsync()
        {
            ApiResponse response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());

            var patientId = _jwtParser.GetIdFromJWT();
            var doctors = await _context.Appointments
                .Where(a => a.PatientId == patientId && a.DoctorId != null)
                .Include(a => a.Doctor)
                .Select(a => a.Doctor)
                .Distinct()
                .ToListAsync();

            if (doctors.Count == 0)
            {
                response.Errors.Add("There is no Doctors in database.");
                return response;
            }
            response.Result = _mapper.Map<IEnumerable<DoctorResponse>>(doctors);
            return response;

        }

        public async Task<ApiResponse> UpdateAsync(DoctorUpdate doctorUpdate)
        {
            ApiResponse response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());

            if (doctorUpdate.DateOfBirth > DateTime.Now)
            {
                response.Errors.Add("Date of birth cannot be in the future.");
                return response;
            }

            var doctor = await _userManager.FindByIdAsync(doctorUpdate.Id);
            if (doctor == null)
            {
                response.Errors.Add("Unable to update Doctor because it doesn't exists.");
                return response;
            }

            _mapper.Map(doctorUpdate, doctor);
            await _userManager.UpdateAsync(doctor);

            response.Result = _mapper.Map<DoctorResponse>(doctor);
            return response;
        }
    }
}
