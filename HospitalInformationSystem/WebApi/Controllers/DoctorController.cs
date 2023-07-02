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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Constants.Technician + "," + Constants.Patient)]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        /// <summary>
        /// Creates a Doctor - [Technician] // Password should be at least 8 characters long, and contain upper case and number.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     POST api/doctor/create
        ///     {
        ///       "firstName": "Mark",
        ///       "lastName": "Morelius",
        ///       "email": "mark@example.com",
        ///       "password": "Mark.123",
        ///       "address": "M. Highus 21",
        ///       "phone": "+381546845",
        ///       "dateOfBirth": "1996-09-19T15:27:00.313Z",
        ///       "specialization": "Surger",
        ///       "hoursPerDay": 30
        ///     }
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost("create")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":null,\r\n\r\n" +
            "\t\"result\":{\r\n\r\n" +
            "\t\t\"specialization\":\"surgeon\" ,\r\n\r\n" +
            "\t\t\"hoursPerDay\":30,\r\n\r\n" +
            "\t\t\"id\":\"c4fef1cb - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n" +
            "\t\t\"firstName\":\"Jane\",\r\n\r\n" +
            "\t\t\"lastName\":\"Doe\",\r\n\r\n" +
            "\t\t\"email\":\"jane@vhis.com\",\r\n\r\n" +
            "\t\t\"address\":\"350 Fifth Avenue, Manhattan\",\r\n\r\n" +
            "\t\t\"phone\":\"123 - 456 - 789\",\r\n\r\n" +
            "\t\t\"dateOfBirth\":\"1992 - 09 - 20T11: 05:04.0270879\"\r\n\r\n\t}\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "{\r\n\r\n\t\"errors\":{" +
            "\r\n\r\n\t\t\"address\":[" +
            "\r\n\r\n\t\t\t\"Unexpected character encountered while parsing value: ,.Path 'address', line 6, position 14.\"\r\n\r\n\t\t\t]," +
            "\r\n\r\n\t\t\"dateOfBirth\":[\r\n\r\n\t\t\t\"Unexpected character encountered while parsing value: ,.Path 'dateOfBirth', line 8, position 18.\"\r\n\r\n\t\t\t" +
            "]\r\n\r\n\t" +
            "}," +
            "\r\n\r\n\t\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.1\"," +
            "\r\n\r\n\t\"title\":\"One or more validation errors occurred.\"," +
            "\r\n\r\n\t\"status\":400," +
            "\r\n\r\n\t\"traceId\":\"|e5d8e345-4d4f69de289f9ccd.\"}" +
            "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Doctor alredy exists.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
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

        /// <summary>
        /// Updates a Doctor - [Technician] // But it does not change password or id.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     PUT api/doctor/update
        ///     {
        ///       "id": "5ba08d2a-0a02-433f-a7d9-6f80880a5f01" 
        ///       "firstName": "Mark",
        ///       "lastName": "Morelius",
        ///       "email": "mark@example.com",
        ///       "address": "M. Highus 21",
        ///       "phone": "+381546845",
        ///       "dateOfBirth": "1996-09-19T15:27:00.313Z",
        ///       "specialization": "Surger",
        ///       "hoursPerDay": 30
        ///     }
        /// </remarks>
        /// <param name="doctorUpdate"></param>   
        [HttpPut("update")]
        [Consumes("application/json")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":null,\r\n\r\n" +
            "\t\"result\":{\r\n\r\n" +
            "\t\t\"specialization\":\"surgeon\",\r\n\r\n" +
            "\t\t\"hoursPerDay\":30,\r\n\r\n" +
            "\t\t\"id\":\"c4fef1cb - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n" +
            "\t\t\"firstName\":\"Jane\",\r\n\r\n" +
            "\t\t\"lastName\":\"Doe\",\r\n\r\n" +
            "\t\t\"email\":\"jane@vhis.com\",\r\n\r\n" +
            "\t\t\"address\":\"350 Fifth Avenue, Manhattan\",\r\n\r\n" +
            "\t\t\"phone\":\"123 - 456 - 789\",\r\n\r\n" +
            "\t\t\"dateOfBirth\":\"1992 - 09 - 20T11: 05:04.0270879\"\r\n\r\n\t}\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "{\r\n\r\n\t\"errors\":{" +
            "\r\n\r\n\t\t\"address\":[" +
            "\r\n\r\n\t\t\t\"Unexpected character encountered while parsing value: ,.Path 'address', line 6, position 14.\"\r\n\r\n\t\t\t]," +
            "\r\n\r\n\t\t\"dateOfBirth\":[\r\n\r\n\t\t\t\"Unexpected character encountered while parsing value: ,.Path 'dateOfBirth', line 8, position 18.\"\r\n\r\n\t\t\t" +
            "]\r\n\r\n\t" +
            "}," +
            "\r\n\r\n\t\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.1\"," +
            "\r\n\r\n\t\"title\":\"One or more validation errors occurred.\"," +
            "\r\n\r\n\t\"status\":400," +
            "\r\n\r\n\t\"traceId\":\"|e5d8e345-4d4f69de289f9ccd.\"}" +
            "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Unable to update Doctor because it doesn't exists.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
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

        /// <summary>
        /// Gets the list of all Doctors - [Technician]
        /// </summary>
        /// <returns>The list of Doctors.</returns>
        // GET: api/doctor/getall
        [HttpGet("getall")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":null,\r\n\r\n" +
            "\t\"result\":[\r\n\r\n\t{\r\n\r\n" +
            "\t\t\"specialization\":\"surgeon\",\r\n\r\n" +
            "\t\t\"hoursPerDay\":30,\r\n\r\n" +
            "\t\t\"id\":\"85468dssh - ss68 - 4747 - a97b - shseh8856\",\r\n\r\n" +
            "\t\t\"firstName\":\"John\",\r\n\r\n" +
            "\t\t\"lastName\":\"Doe\",\r\n\r\n" +
            "\t\t\"email\":\"john@gmail.com\",\r\n\r\n" +
            "\t\t\"address\":\"440 Fifth Avenue, Manhattan\",\r\n\r\n" +
            "\t\t\"phone\":\"+36522158965\",\r\n\r\n" +
            "\t\t\"dateOfBirth\":\"1956 - 09 - 13T13: 41:52.271\"\r\n\r\n" +
            "\t},\r\n\r\n" +
            "\t{\r\n\r\n" +
            "\t\t\"specialization\":\"surgeon\",\r\n\r\n" +
            "\t\t\"hoursPerDay\":40,\r\n\r\n" +
            "\t\t\"id\":\"f4c7c02c - 446f - 47b7 - abe4 - 26aef447ea62\",\r\n\r\n" +
            "\t\t\"firstName\":\"Jane\",\r\n\r\n" +
            "\t\t\"lastName\":\"Vhis\",\r\n\r\n" +
            "\t\t\"email\":\"jane@example.com\",\r\n\r\n" +
            "\t\t\"address\":\"365 Fifth Avenue, Manhattan\",\r\n\r\n" +
            "\t\t\"phone\":\"+69588228\",\r\n\r\n" +
            "\t\t\"dateOfBirth\":\"1385 - 09 - 13T14: 23:58.348\"\r\n\r\n" +
            "\t}\r\n\r\n\t]\r\n\r\n" +
            "\t\"roles\":\"PATIENT\"\r\n\r\n" +
            "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"There is no Doctors in databse.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":[\"TECHNICIAN\"]\r\n\r\n" +
            "}")]
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

        /// <summary>
        /// Get a Doctor with specific Id - [Technician, Patient] //This can also be used by a Patient who is treated by requested Doctor, so he can see Doctors details
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     GET api/doctor/get/5ba08d2a-0a02-433f-a7d9-6f80880a5f01
        /// </remarks>
        /// <param name="id"></param>  
        [HttpGet("get/{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":null,\r\n\r\n" +
            "\t\"result\":{\r\n\r\n" +
            "\t\t\"specialization\":\"surgeon\",\r\n\r\n" +
            "\t\t\"hoursPerDay\":30,\r\n\r\n" +
            "\t\t\"id\":\"c4fef1cb - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n" +
            "\t\t\"firstName\":\"Jane\",\r\n\r\n" +
            "\t\t\"lastName\":\"Doe\",\r\n\r\n" +
            "\t\t\"email\":\"jane@vhis.com\",\r\n\r\n" +
            "\t\t\"address\":\"350 Fifth Avenue, Manhattan\",\r\n\r\n" +
            "\t\t\"phone\":\"123 - 456 - 789\",\r\n\r\n" +
            "\t\t\"dateOfBirth\":\"1992 - 09 - 20T11: 05:04.0270879\"\r\n\r\n\t}\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Unable to get Doctor because it doesn't exists.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
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

        /// <summary>
        /// Get a List of Doctors for Patient who is treated by them - [Patient]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     GET api/doctor/getdoctorsbypatient
        /// </remarks>
        [HttpGet("getdoctorsbypatient")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":null,\r\n\r\n" +
            "\t\"result\":[\r\n\r\n\t{\r\n\r\n" +
            "\t\t\"specialization\":\"surgeon\",\r\n\r\n" +
            "\t\t\"hoursPerDay\":30,\r\n\r\n" +
            "\t\t\"id\":\"85468dssh - ss68 - 4747 - a97b - shseh8856\",\r\n\r\n" +
            "\t\t\"firstName\":\"John\",\r\n\r\n" +
            "\t\t\"lastName\":\"Doe\",\r\n\r\n" +
            "\t\t\"email\":\"john@gmail.com\",\r\n\r\n" +
            "\t\t\"address\":\"440 Fifth Avenue, Manhattan\",\r\n\r\n" +
            "\t\t\"phone\":\"+36522158965\",\r\n\r\n" +
            "\t\t\"dateOfBirth\":\"1956 - 09 - 13T13: 41:52.271\"\r\n\r\n" +
            "\t},\r\n\r\n" +
            "\t{\r\n\r\n" +
            "\t\t\"specialization\":\"surgeon\",\r\n\r\n" +
            "\t\t\"hoursPerDay\":40,\r\n\r\n" +
            "\t\t\"id\":\"f4c7c02c - 446f - 47b7 - abe4 - 26aef447ea62\",\r\n\r\n" +
            "\t\t\"firstName\":\"Jane\",\r\n\r\n" +
            "\t\t\"lastName\":\"Vhis\",\r\n\r\n" +
            "\t\t\"email\":\"jane@example.com\",\r\n\r\n" +
            "\t\t\"address\":\"365 Fifth Avenue, Manhattan\",\r\n\r\n" +
            "\t\t\"phone\":\"+69588228\",\r\n\r\n" +
            "\t\t\"dateOfBirth\":\"1385 - 09 - 13T14: 23:58.348\"\r\n\r\n" +
            "\t}\r\n\r\n\t]\r\n\r\n" +
            "\t\"roles\":\"PATIENT\"\r\n\r\n" +
            "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"There is no Doctors in database.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
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

        /// <summary>
        /// Delete a Doctor with specific Id - [Technician]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     DELETE api/doctor/delete/5ba08d2a-0a02-433f-a7d9-6f80880a5f01
        /// </remarks>
        /// <param name="id"></param>   
        [HttpDelete("delete/{id}")]
        //[Produces("application/json")]
        //[SwaggerResponse(StatusCodes.Status200OK,Type = typeof(ApiResponse))]
        //[SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        //[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": \"Doctor with id: [5ba08d2a - 0a02 - 433f - a7d9 - 6f80880a5f01] is deleted successfully.\",\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null)]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Unable to delete Doctor because it doesn't exists.\",\r\n\r\n" +
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
