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
    public class PatientService : IPatientService
    {
        private readonly UserManager<ApiUser> _userManager;
        private readonly HISContext _context;
        private readonly IMapper _mapper;
        private readonly JwtParser _jwtParser;

        public PatientService(UserManager<ApiUser> userManager, HISContext context, IMapper mapper, JwtParser jwtParser)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _jwtParser = jwtParser;
        }
        public Task<ApiResponse> AddAsync(PatientRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse> DeleteAsync(string id)
        {
            ApiResponse response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());

            var patient = await _context.Patients
                .Include(p => p.Appointments)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                response.Errors.Add("Unable to delete patient because it doesn't exists.");
                return response;
            }

            _context.Appointments.RemoveRange(_context.Appointments.Where(a => a.Completed == false && a.PatientId == id));
            _context.Users.Remove(patient);
            await _context.SaveChangesAsync();

            response.Result = $"Patient with id: [{id}] is deleted successfully.";
            return response;
        }

        public async Task<ApiResponse> GetAllAsync()
        {
            ApiResponse response = new ApiResponse();
            response.Roles.Add(_jwtParser.GetRoleFromJWT());
            var patients = await _context.Patients.ToListAsync();

            if (patients.Count() == 0)
            {
                response.Result = "There is no patients in database.";
                return response;
            }

            response.Result = _mapper.Map<IEnumerable<PatientResponse>>(patients);
            return response;
        }

        public async Task<ApiResponse> GetAsync(string id)
        {
            ApiResponse response = new ApiResponse();
            var role = _jwtParser.GetRoleFromJWT();
            response.Roles.Add(role);
            var exists = false;
            string doctorId = null;
            if (role == Constants.Doctor)
            {
                doctorId = _jwtParser.GetIdFromJWT();
                exists = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId && a.PatientId == id)
                    .AnyAsync();
                if (!exists)
                {
                    response.Errors.Add($"Unable to preview Patient's details because user with id {doctorId} does not treat this patient.");
                    return response;
                }
            }
            var patient = await _context.Patients
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                response.Errors.Add("Unable to get patient because it doesn't exists.");
                return response;
            }

            response.Result = _mapper.Map<PatientResponse>(patient);
            return response;
        }

        public Task<ApiResponse> GetPatientsByDoctorAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> UpdateAsync(PatientUpdate patientUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
