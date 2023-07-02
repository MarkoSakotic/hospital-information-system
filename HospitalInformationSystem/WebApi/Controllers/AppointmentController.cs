using DtoEntityProject;
using DtoEntityProject.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceProject.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Constants.Technician + "," + Constants.Patient + "," + Constants.Doctor)]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _iappointmentService;

        public AppointmentController(IAppointmentService iappointmentService)
        {
            _iappointmentService = iappointmentService;
        }

        [HttpPost("createfordoctor")]
        [Consumes("application/json")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> CreateAppointmentsAsync([FromBody] AppointmentRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _iappointmentService.CreateAppointmentsAsync(request);
                if (response.Errors.Count != 0 && response.Result == null)
                {
                    return NotFound(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }

        }

        [HttpPut("update")]
        [Consumes("application/json")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> UpdateAppointmentAsync([FromBody] AppointmentUpdate appointmentUpdate)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _iappointmentService.UpdateAppointmentAsync(appointmentUpdate);
                if (response.Errors.Count != 0)
                {
                    return NotFound(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }

        [HttpPut("complete")]
        [Consumes("application/json")]
        [Authorize(Roles = Constants.Doctor)]
        public async Task<IActionResult> CompleteAppointmentAsync(AppointmentComplete appointmentComplete)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _iappointmentService.CompleteAppointmentAsync(appointmentComplete);
                if (response.Errors.Count != 0)
                {
                    return NotFound(response);
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
                return BadRequest(response);
            }
        }

        [HttpPut("schedule")]
        [Consumes("application/json")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> ScheduleAppointmentAsync([FromBody] AppointmentSchedule appointmentSchedule)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _iappointmentService.ScheduleAppointmentAsync(appointmentSchedule);
                if (response.Errors.Count != 0)
                {
                    return NotFound(response);
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
                return BadRequest(response);
            }
        } 

        [HttpPost("getallbyuser")]
        [Consumes("application/json")]
        [Authorize(Roles = Constants.Patient + "," + Constants.Doctor + "," + Constants.Technician)]
        public async Task<IActionResult> GetAllForUserWithinGivenPeriodAsync([FromBody] AppointmentFilter appointmentFilter)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _iappointmentService.GetAllAppointmentsForUserWithinGivenPeriodAsync(appointmentFilter);
                if (response.Errors.Count != 0)
                {
                    return NotFound(response);
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
                return BadRequest(response);
            }
        }

        [HttpGet("getall")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> GetAllAppointmentAsync()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _iappointmentService.GetAllAppointmentsAsync();
                if (response.Errors.Count != 0)
                {
                    return NotFound(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }
 
        [HttpGet("get/{id}")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> GetAppointmentAsync(int id)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _iappointmentService.GetAppointmentAsync(id);
                if (response.Errors.Count != 0)
                {
                    return NotFound(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }

        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> DeleteAppointmentAsync(int id)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _iappointmentService.DeleteAppointmentAsync(id);
                if (response.Errors.Count != 0)
                {
                    return NotFound(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }
    }
}
