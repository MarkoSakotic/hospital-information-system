using DtoEntityProject;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProject.Interface
{
    public interface IDoctorService
    {
        Task<ApiResponse> AddAsync(DoctorRequest request);
        Task<ApiResponse> GetAllAsync();
        Task<ApiResponse> GetAsync(string id);
        Task<ApiResponse> GetDoctorsByPatientAsync();
        Task<ApiResponse> DeleteAsync(string id);
        Task<ApiResponse> UpdateAsync(DoctorUpdate doctorUpdate);
    }
}
