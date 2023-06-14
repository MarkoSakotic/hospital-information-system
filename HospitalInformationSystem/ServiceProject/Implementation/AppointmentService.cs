using DtoEntityProject;
using ServiceProject.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProject.Implementation
{
    public class AppointmentService : IAppointmentService
    {
        public Task<ApiResponse> CreateAppointmentsAsync(AppointmentRequest appointmentRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> DeleteAppointmentAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> GetAllAppointmentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> GetAppointmentAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> UpdateAppointmentAsync(AppointmentUpdate appointmentUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
