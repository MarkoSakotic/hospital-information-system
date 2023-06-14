using DtoEntityProject;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProject.Interface
{
    public interface IPatientService
    {
        Task<ApiResponse> AddAsync(PatientRequest request);
        Task<ApiResponse> GetAllAsync();
        Task<ApiResponse> GetAsync(string id);
        Task<ApiResponse> DeleteAsync(string id);
        Task<ApiResponse> UpdateAsync(PatientUpdate patientUpdate);
        Task<ApiResponse> GetPatientsByDoctorAsync();
    }
}
