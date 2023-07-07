using DtoEntityProject;
using DtoEntityProject.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceProject.Interface;
using Swashbuckle.AspNetCore.Annotations;
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

        /// <summary>
        /// Creates a Patient - [Technician] // Password should be at least 8 characters long, and contain upper case, number and special character.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     POST api/patient/create
        ///     {
        ///       "firstName": "Mark",
        ///       "lastName": "Morelius",
        ///       "email": "mark@example.com",
        ///       "password": "Mark.123",
        ///       "address": "M. Highus 21",
        ///       "phone": "+381546845",
        ///       "dateOfBirth": "1996-09-19T15:27:00.313Z",
        ///       "ssn": "15SDA52D521"
        ///     }
        /// </remarks>
        /// <param name="request"></param>   
        [HttpPost("create")]
        [Consumes("application/json")]
        [SwaggerResponse(200, "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":null,\r\n\r\n" +
            "\t\"result\":{\r\n\r\n" +
            "\t\t\"ssn\":\"15SDA52D521\",\r\n\r\n" +
            "\t\t\"id\": \"1ca2a14c-3e22-45a4-8c2e-28f982d33f5c\",\r\n\r\n" +
            "\t\t\"firstName\": \"Peter\",\r\n\r\n" +
            "\t\t\"lastName\": \"Smith\",\r\n\r\n" +
            "\t\t\"email\": \"petersmith@example.com\",\r\n\r\n" +
            "\t\t\"address\": \"Main squere 23, London\",\r\n\r\n" +
            "\t\t\"phone:\": \"+38166232555\",\r\n\r\n" +
            "\t\t\"dateOfBirth\": \"1995-09-22T12:30:22.885Z\r\n\r\n\t}\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "{\r\n\r\n\t\"errors\":{" +
            "\r\n\r\n\t\t\"FirstName\":[" +
            "\r\n\r\n\t\t\t\"The FirstName field is requierd.\"\r\n\r\n\t\t\t" +
            "]\r\n\r\n\t" +
            "}," +
            "\r\n\r\n\t\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.1\"," +
            "\r\n\r\n\t\"title\":\"One or more validation errors occurred.\"," +
            "\r\n\r\n\t\"status\":400," +
            "\r\n\r\n\t\"traceId\":\"|b9c6ee37-47909aa210fe39b7.\"}" +
            "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Patient alredy exists.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> AddAsync([FromBody] PatientRequest request)
        {
            ApiResponse response = new ApiResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    response = await _patientService.AddAsync(request);

                    if (response.Errors.Any())
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
                response.Errors.AddRange(ModelState.Values.SelectMany(e => e.Errors) as IEnumerable<string>);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates a Patient - [Technician] // But it does not change password or id.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     PUT api/patient/update
        ///     {
        ///       "id": "5ba08d2a-0a02-433f-a7d9-6f80880a5f01"  
        ///       "firstName": "Mark",
        ///       "lastName": "Morelius",
        ///       "email": "mark@example.com",
        ///       "address": "M. Highus 21",
        ///       "phone": "+381546845",
        ///       "dateOfBirth": "1996-09-19T15:27:00.313Z",
        ///       "ssn": "15SDA52D521"
        ///     }
        /// </remarks>
        /// <param name="patientUpdate"></param>   
        [HttpPut("update")]
        [Consumes("application/json")]
        [Authorize(Roles = Constants.Technician)]
        [SwaggerResponse(200, "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":null,\r\n\r\n" +
            "\t\"result\":{\r\n\r\n" +
            "\t\t\"ssn\":\"15SDA52D521\",\r\n\r\n" +
            "\t\t\"id\": \"1ca2a14c-3e22-45a4-8c2e-28f982d33f5c\",\r\n\r\n" +
            "\t\t\"firstName\": \"Peter\",\r\n\r\n" +
            "\t\t\"lastName\": \"Smith\",\r\n\r\n" +
            "\t\t\"email\": \"petersmith@example.com\",\r\n\r\n" +
            "\t\t\"address\": \"Main squere 23, London\",\r\n\r\n" +
            "\t\t\"phone:\": \"+38166232555\",\r\n\r\n" +
            "\t\t\"dateOfBirth\": \"1995-09-22T12:30:22.885Z\r\n\r\n\t}\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "{\r\n\r\n\t\"errors\":{" +
            "\r\n\r\n\t\t\"FirstName\":[" +
            "\r\n\r\n\t\t\t\"The FirstName field is requierd.\"\r\n\r\n\t\t\t" +
            "]\r\n\r\n\t" +
            "}," +
            "\r\n\r\n\t\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.1\"," +
            "\r\n\r\n\t\"title\":\"One or more validation errors occurred.\"," +
            "\r\n\r\n\t\"status\":400," +
            "\r\n\r\n\t\"traceId\":\"|b9c6ee37-47909aa210fe39b7.\"}" +
            "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Unable to update Patient because it doesn't exists.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        public async Task<IActionResult> UpdateAsync([FromBody] PatientUpdate patientUpdate)
        {
            ApiResponse response = new ApiResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    response = await _patientService.UpdateAsync(patientUpdate);

                    if (response.Errors.Any())
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
                response.Errors.AddRange(ModelState.Values.SelectMany(e => e.Errors) as IEnumerable<string>);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Gets the list of all Patients - [Technician]
        /// </summary>
        /// <returns>The list of Patients.</returns>
        // GET: api/patient/getall
        [HttpGet("getall")]
        [SwaggerResponse(200, "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":null,\r\n\r\n" +
            "\t\"result\":[\r\n\r\n" +
            "\t\"{\r\n\r\n" +
            "\t\t\"ssn\":\"15SDA52D521\",\r\n\r\n" +
            "\t\t\"id\": \"1ca2a14c-3e22-45a4-8c2e-28f982d33f5c\",\r\n\r\n" +
            "\t\t\"firstName\": \"Peter\",\r\n\r\n" +
            "\t\t\"lastName\": \"Smith\",\r\n\r\n" +
            "\t\t\"email\": \"petersmith@example.com\",\r\n\r\n" +
            "\t\t\"address\": \"Main squere 23, London\",\r\n\r\n" +
            "\t\t\"phone:\": \"+38166232555\",\r\n\r\n" +
            "\t\t\"dateOfBirth\": \"1995-09-22T12:30:22.885Z\"\r\n\r\n\t},\r\n\r\n" +
            "\t{,\r\n\r\n" +
            "\t\t\"ssn\":\"25BEA53D521\",\r\n\r\n" +
            "\t\t\"id\": \"2ca8b33a-7f55-85a9-3c7d-38g782e36f8d\",\r\n\r\n" +
            "\t\t\"firstName\": \"Emilia\",\r\n\r\n" +
            "\t\t\"lastName\": \"John\",\r\n\r\n" +
            "\t\t\"email\": \"emiliajohn@example.com\",\r\n\r\n" +
            "\t\t\"address\": \"Low street 11b, Edinburgh\",\r\n\r\n" +
            "\t\t\"phone:\": \"+3816295858\",\r\n\r\n" +
            "\t\t\"dateOfBirth\": \"1997-01-12T11:20:22.885Z\"\r\n\r\n\t}\r\n\r\n\t]\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"There is no Patient in databse.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> GetAllAsync()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _patientService.GetAllAsync();

                if (response.Errors.Any())
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

        /// <summary>
        /// Get a Patient with specific Id - [Technician, Doctor] // This can also be used by a Doctor who treats requested Patient, so he can see patient details
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     GET api/patient/get/5ba08d2a-0a02-433f-a7d9-6f80880a5f01
        /// </remarks>
        /// <param name="id"></param>   
        [HttpGet("get/{id}")]
        [Authorize(Roles = Constants.Technician)]
        [SwaggerResponse(200, "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":null,\r\n\r\n" +
            "\t\"result\":{\r\n\r\n" +
            "\t\t\"ssn\":\"15SDA52D521\",\r\n\r\n" +
            "\t\t\"id\": \"1ca2a14c-3e22-45a4-8c2e-28f982d33f5c\",\r\n\r\n" +
            "\t\t\"firstName\": \"Peter\",\r\n\r\n" +
            "\t\t\"lastName\": \"Smith\",\r\n\r\n" +
            "\t\t\"email\": \"petersmith@example.com\",\r\n\r\n" +
            "\t\t\"address\": \"Main squere 23, London\",\r\n\r\n" +
            "\t\t\"phone:\": \"+38166232555\",\r\n\r\n" +
            "\t\t\"dateOfBirth\": \"1995-09-22T12:30:22.885Z\"}\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Unable to get Patient because it doesn't exists.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [Authorize(Roles = Constants.Technician + "," + Constants.Doctor)]
        public async Task<IActionResult> GetAsync(string id)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _patientService.GetAsync(id);

                if (response.Errors.Any())
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

        /// <summary>
        /// Get a List of Patients by Doctor who treats them - [Doctor]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     GET api/patient/getpatientsbydoctor
        /// </remarks>
        [HttpGet("getpatientsbydoctor")]
        [Authorize(Roles = Constants.Doctor)]
        [SwaggerResponse(200, "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":null,\r\n\r\n" +
            "\t\"result\":[\r\n\r\n" +
            "\t\"{\r\n\r\n" +
            "\t\t\"ssn\":\"15SDA52D521\",\r\n\r\n" +
            "\t\t\"id\": \"1ca2a14c-3e22-45a4-8c2e-28f982d33f5c\",\r\n\r\n" +
            "\t\t\"firstName\": \"Peter\",\r\n\r\n" +
            "\t\t\"lastName\": \"Smith\",\r\n\r\n" +
            "\t\t\"email\": \"petersmith@example.com\",\r\n\r\n" +
            "\t\t\"address\": \"Main squere 23, London\",\r\n\r\n" +
            "\t\t\"phone:\": \"+38166232555\",\r\n\r\n" +
            "\t\t\"dateOfBirth\": \"1995-09-22T12:30:22.885Z\"\r\n\r\n\t},\r\n\r\n" +
            "\t{,\r\n\r\n" +
            "\t\t\"ssn\":\"25BEA53D521\",\r\n\r\n" +
            "\t\t\"id\": \"2ca8b33a-7f55-85a9-3c7d-38g782e36f8d\",\r\n\r\n" +
            "\t\t\"firstName\": \"Emilia\",\r\n\r\n" +
            "\t\t\"lastName\": \"John\",\r\n\r\n" +
            "\t\t\"email\": \"emiliajohn@example.com\",\r\n\r\n" +
            "\t\t\"address\": \"Low street 11b, Edinburgh\",\r\n\r\n" +
            "\t\t\"phone:\": \"+3816295858\",\r\n\r\n" +
            "\t\t\"dateOfBirth\": \"1997-01-12T11:20:22.885Z\"\r\n\r\n\t}\r\n\r\n\t]\r\n\r\n" +
            "\t\"roles\":\"DOCTOR\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"There is no Patients in database.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        public async Task<IActionResult> GetPatientsByDoctor()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _patientService.GetPatientsByDoctorAsync();

                if (response.Errors.Any())
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

        /// <summary>
        /// Delete a Patient with specific Id - [Technician]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     DELETE api/patient/delete/5ba08d2a-0a02-433f-a7d9-6f80880a5f01
        /// </remarks>
        /// <param name="id"></param>   
        [HttpDelete("delete/{id}")]
        [SwaggerResponse(200, "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": \"Patient with id: [1ca2a14c-3e22-45a4-8c2e-28f982d33f5c] is deleted successfully.\",\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null)]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Unable to delete Patient because it doesn't exists.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await _patientService.DeleteAsync(id);

                if (response.Errors.Any())
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
