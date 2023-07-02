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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Constants.Technician + "," + Constants.Patient)]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }


        [HttpPost("create")]
        [Consumes("application/json")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> Add([FromBody] DoctorRequest request)
        {
            ApiResponse response = new ApiResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    response = await _doctorService.AddAsync(request);
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
            else
            {
                response.Errors.AddRange((IEnumerable<string>)ModelState.Values.SelectMany(e => e.Errors));
                return BadRequest(ModelState);
            }
        }

        [HttpPut("update")]
        [Consumes("application/json")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> UpdateAsync(DoctorUpdate doctorUpdate)
        {
            ApiResponse response = new ApiResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    response = await _doctorService.UpdateAsync(doctorUpdate);
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
            else
            {
                response.Errors.AddRange((IEnumerable<string>)ModelState.Values.SelectMany(e => e.Errors));
                return BadRequest(ModelState);
            }
        }

        [HttpGet("getall")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> GetAllAsync()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _doctorService.GetAllAsync();
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

        [HttpGet("get/{id}")]
        [Authorize(Roles = Constants.Technician + "," + Constants.Patient)]
        public async Task<IActionResult> GetAsync(string id)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _doctorService.GetAsync(id);
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

        [HttpGet("getdoctorsbypatient")]
        [Authorize(Roles = Constants.Patient)]
        public async Task<IActionResult> GetDoctorsByPatientAsync()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _doctorService.GetDoctorsByPatientAsync();
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

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _doctorService.DeleteAsync(id);
                if (response.Errors.Count() != 0)
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
    }
}
