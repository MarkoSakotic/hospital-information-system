using AutoMapper;
using DtoEntityProject;
using DtoEntityProject.Constants;
using EntityProject;
using Microsoft.AspNetCore.Identity;
using ServiceProject.Interface;
using ServiceProject.Utility;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceProject.Implementation
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApiUser> _userManager;
        private readonly JwtParser _jwtParser;
        private IMapper _mapper;

        public IdentityService(UserManager<ApiUser> userManager,
            JwtParser jwtParser, IMapper mapper)
        {
            _userManager = userManager;
            _jwtParser = jwtParser;
            _mapper = mapper;
        }

        public async Task<ApiResponse> RegisterUserAsync(ApiUser user)
        {
            var existingUser = await _userManager.FindByEmailAsync(user.Email);

            if (existingUser != null)
            {
                return new ApiResponse
                {
                    Errors = new System.Collections.Generic.List<string> { "User alredy exists" }
                };
            }

            var newUser = await _userManager.CreateAsync(user, user.Password);

            if (newUser != null)
            {
                return new ApiResponse
                {
                    Errors = newUser.Errors.Select(s => s.Description).ToList()
                };
            }

            return null;
        }

        public async Task<ApiResponse> LoginAsync(LoginRequest request)
        {
            var response = new ApiResponse();
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                response.Errors.Add("User does not exists");
                return response;
            }

            var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!validPassword)
            {
                response.Errors.Add("Password is wrong");
                return response;
            }
            var role = await _userManager.GetRolesAsync(user);
            response.Roles.Add(role[0]);
            if (role[0] == Constants.Technician)
                response.Result = _mapper.Map<TechnicianResponse>(user);
            if (role[0] == Constants.Patient)
                response.Result = _mapper.Map<PatientResponse>(user);
            if (role[0] == Constants.Doctor)
                response.Result = _mapper.Map<DoctorResponse>(user);
            return response;
        }

        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var id = _jwtParser.GetIdFromJWT();
            var role = _jwtParser.GetRoleFromJWT();
            var response = new ApiResponse();
            response.Roles.Add(role);
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null || user.Id != id)
            {
                response.Errors.Add($"Unable to change password for user with email: {resetPasswordDto.Email}");
                return response;
            }
            if (user.Password == resetPasswordDto.OldPassword)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                try
                {
                    if ((await _userManager.ResetPasswordAsync(user, code, resetPasswordDto.NewPassword)).Succeeded)
                    {
                        user.Password = resetPasswordDto.NewPassword;
                        await _userManager.UpdateAsync(user);
                        response.Result = $"Password has been reseted successfully.";
                        return response;
                    }
                }
                catch (Exception e)
                {
                    response.Errors.Add(e.Message);
                    return response;
                }
            }
            response.Errors.Add("You have entered wrong old password.");
            return response;
        }

    }
}
