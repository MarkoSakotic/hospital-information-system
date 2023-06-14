using DtoEntityProject;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProject.Interface
{
    public interface IAppointmentService
    {
        Task<ApiResponse> CreateAppointmentsAsync(AppointmentRequest appointmentRequest);
        Task<ApiResponse> UpdateAppointmentAsync(AppointmentUpdate appointmentUpdate);
        Task<ApiResponse> GetAllAppointmentsAsync();
        Task<ApiResponse> GetAppointmentAsync(int id);
        Task<ApiResponse> DeleteAppointmentAsync(int id);
    }
}
