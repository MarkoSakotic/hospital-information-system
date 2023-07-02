using DtoEntityProject;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProject.Interface
{
    public interface IIdentityService
    {
        Task<ApiResponse> LoginAsync(LoginRequest request);
        Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}
