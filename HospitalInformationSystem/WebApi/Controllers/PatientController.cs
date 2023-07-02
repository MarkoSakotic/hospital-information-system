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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Constants.Technician + "," + Constants.Doctor)]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }


        [HttpPost("create")]
        [Consumes("application/json")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> AddAsync([FromBody] PatientRequest request)
        {
            ApiResponse response = new ApiResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    response = await _patientService.AddAsync(request);

                    if (response.Errors.Count() != 0)
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
        public async Task<IActionResult> UpdateAsync([FromBody] PatientUpdate patientUpdate)
        {
            ApiResponse response = new ApiResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    response = await _patientService.UpdateAsync(patientUpdate);

                    if (response.Errors.Count() != 0)
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

        [HttpGet("getall")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> GetAllAsync()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _patientService.GetAllAsync();

                if (response.Errors.Count() != 0)
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
        [Authorize(Roles = Constants.Technician + "," + Constants.Doctor)]
        public async Task<IActionResult> GetAsync(string id)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _patientService.GetAsync(id);

                if (response.Errors.Count() != 0)
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

        [HttpGet("getpatientsbydoctor")]
        [Authorize(Roles = Constants.Doctor)]
        public async Task<IActionResult> GetPatientsByDoctor()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _patientService.GetPatientsByDoctorAsync();

                if (response.Errors.Count() != 0)
                {
                    return NotFound(response);
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
                return BadRequest(response.Errors);
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _patientService.DeleteAsync(id);

                if (response.Errors.Count() != 0)
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
