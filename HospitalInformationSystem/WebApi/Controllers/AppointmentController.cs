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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Constants.Technician + "," + Constants.Patient + "," + Constants.Doctor)]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _iappointmentService;

        public AppointmentController(IAppointmentService iappointmentService)
        {
            _iappointmentService = iappointmentService;
        }

        /// <summary>
        /// Creates the list of Appointments for Doctor for given time period - [Technician] // Duration should be 30 (mins), startDate hours should be 09:00:00, endDate hours are not important
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     POST api/appointment/createfordoctor
        ///     {
        ///        "doctorId": "5ba08d2a-0a02-433f-a7d9-6f80880a5f01",
        ///        "startDate": "2022-09-20",
        ///        "endDate": "2022-09-22",
        ///        "duration": 30,
        ///        "hoursPerDay": 6
        ///     }
        /// </remarks>
        /// <param name="request"></param>   
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
        "\t\"token\":null,\r\n\r\n" +
        "\t\"result\":[\r\n\r\n\t{\r\n\r\n" +
        "\t\t\"id\":1,\r\n\r\n" +
        "\t\t\"note\":\"Note1\",\r\n\r\n" +
        "\t\t\"date\":\"2002-09-14T08:43:43.638\",\r\n\r\n" +
        "\t\t\"startTime\":\"2022-09-25T08:43:43.638\",\r\n\r\n" +
        "\t\t\"endTime\":\"2022-09-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"completed\": false,\r\n\r\n" +
        "\t\t\"doctorId\":\"58kydc-4947-4597-a9a9-7ed2766f6020\",\r\n\r\n" +
        "\t\t\"patientId\": null,\r\n\r\n\t\t},\r\n\r\n" +
        "\t\t{\r\n\r\n" +
        "\t\t\"id\":2,\r\n\r\n" +
        "\t\t\"note\":\"Note1\",\r\n\r\n" +
        "\t\t\"date\":\"2002-09-14T08:43:43.638\",\r\n\r\n" +
        "\t\t\"startTime\":\"2022-10-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"endTime\":\"2022-10-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"completed\": false,\r\n\r\n" +
        "\t\t\"doctorId\":\"58kydc-4947-4597-a9a9-7ed2766f6020\",\r\n\r\n" +
        "\t\t\"patientId\": null,\r\n\r\n\t\t}\r\n\r\n\t]" +
        "\r\n\r\n\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
        "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "{\r\n\r\n\t\"errors\":{" +
        "\r\n\r\n\t\t\"completed\":[" +
        "\r\n\r\n\t\t\t\"Unexpected character encountered while parsing value: ,.Path 'completed', line 6, position 14.\"\r\n\r\n\t\t\t]," +
        "\r\n\r\n\t\t\"note\":[\r\n\r\n\t\t\t\"Unexpected character encountered while parsing value: ,.Path 'note', line 8, position 18.\"\r\n\r\n\t\t\t" +
        "]\r\n\r\n\t" +
        "}," +
        "\r\n\r\n\t\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.1\"," +
        "\r\n\r\n\t\"title\":\"One or more validation errors occurred.\"," +
        "\r\n\r\n\t\"status\":400," +
        "\r\n\r\n\t\"traceId\":\"|e5d8e345-4d4f69de289f9ccd.\"}" +
        "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Unable to create appointments for doctor with id: hrerh25842 - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n" +
        "\t\"token\":\"\",\r\n\r\n" +
        "\t\"result\": null,\r\n\r\n" +
        "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
        "}")]
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

        /// <summary>
        /// Changes the appointment Note or Patient - [Technician]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     PUT api/appointment/update
        ///     {
        ///        "appointmentId": 1,
        ///        "note": "some note",
        ///        "patientId": "5ba08d2a-0a02-433f-a7d9-6f80880a5f01"
        ///     }
        /// </remarks>
        /// <param name="appointmentUpdate"></param>  
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
        "\t\"token\":null,\r\n\r\n" +
        "\t\"result\":{\r\n\r\n" +
        "\t\t\"id\":1,\r\n\r\n" +
        "\t\t\"note\":\"Note1\",\r\n\r\n" +
        "\t\t\"date\":\"2002-09-14T08:43:43.638\",\r\n\r\n" +
        "\t\t\"startTime\":\"2022-09-25T08:43:43.638\",\r\n\r\n" +
        "\t\t\"endTime\":\"2022-09-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"completed\": false,\r\n\r\n" +
        "\t\t\"doctorId\":\"58kydc - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n" +
        "\t\t\"patientId\":\"jdtj155 - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n\t\t}\r\n\r\n" +
        "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
        "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "{\r\n\r\n\t\"errors\":{" +
            "\r\n\r\n\t\t\"completed\":[" +
            "\r\n\r\n\t\t\t\"Unexpected character encountered while parsing value: ,.Path 'completed', line 6, position 14.\"\r\n\r\n\t\t\t]," +
            "\r\n\r\n\t\t\"patientId\":[\r\n\r\n\t\t\t\"Unexpected character encountered while parsing value: ,.Path 'patientId', line 8, position 18.\"\r\n\r\n\t\t\t" +
            "]\r\n\r\n\t" +
            "}," +
            "\r\n\r\n\t\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.1\"," +
            "\r\n\r\n\t\"title\":\"One or more validation errors occurred.\"," +
            "\r\n\r\n\t\"status\":400," +
            "\r\n\r\n\t\"traceId\":\"|e5d8e345-4d4f69de289f9ccd.\"}" +
            "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Unable to update appointment because it doesn't exists.\",\r\n\r\n" +
        "\t\"token\":\"\",\r\n\r\n" +
        "\t\"result\": null,\r\n\r\n" +
        "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
        "}")]
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

        /// <summary>
        /// Complete appointment - [Doctor] // Could only be done by a doctor who attended appointment
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     PUT api/appointment/complete
        ///     {
        ///        "appointmentId": 1
        ///        "note": Patient has no serious injuries.
        ///     }
        /// </remarks>
        /// <param name="appointmentComplete"></param>  
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
        "\t\"token\":null,\r\n\r\n" +
        "\t\"result\":{\r\n\r\n" +
        "\t\t\"id\": 1,\r\n\r\n" +
        "\t\t\"note\":\"Patient has no serious injuries.\",\r\n\r\n" +
        "\t\t\"date\":\"2002-09-14T08:43:43.638\",\r\n\r\n" +
        "\t\t\"startTime\":\"2022-09-25T08:43:43.638\",\r\n\r\n" +
        "\t\t\"endTime\":\"2022-09-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"completed\": true,\r\n\r\n" +
        "\t\t\"doctorId\":\"58kydc-4947-597-a9a9-7ed2766f6020\",\r\n\r\n" +
        "\t\t\"patientId\":\"jdtj155-4947-4597-a9a9-7ed2766f6020\",\r\n\r\n\t\t}\r\n\r\n" +
        "\t\"roles\":\"DOCTOR\"\r\n\r\n" +
        "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "{\r\n\r\n\t\"errors\":{" +
            "\r\n\r\n\t\t\"appointmentId\":[\r\n\r\n\t\t\t\"Unexpected character encountered while parsing value: ,.Path 'appointmentId', line 8, position 18.\"\r\n\r\n\t\t\t" +
            "]\r\n\r\n\t" +
            "}," +
            "\r\n\r\n\t\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.1\"," +
            "\r\n\r\n\t\"title\":\"One or more validation errors occurred.\"," +
            "\r\n\r\n\t\"status\":400," +
            "\r\n\r\n\t\"traceId\":\"|e5d8e345-4d4f69de289f9ccd.\"}" +
            "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Database does not contain appointment with that id.\",\r\n\r\n" +
        "\t\"token\":\"\",\r\n\r\n" +
        "\t\"result\": null,\r\n\r\n" +
        "\t\"roles\":\"DOCTOR\"\r\n\r\n" +
        "}")]
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

        /// <summary>
        /// Schedule appointment by assigning Patient to it - [Technician]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     PUT api/appointment/complete
        ///     {
        ///        "appointmentId": 1,
        ///        "patientId": "5ba08d2a-0a02-433f-a7d9-6f80880a5f01",
        ///        "note": "Some note"
        ///     }
        /// </remarks>
        /// <param name="appointmentSchedule"></param>  
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
        "\t\"token\":null,\r\n\r\n" +
        "\t\"result\":{\r\n\r\n" +
        "\t\t\"id\":1,\r\n\r\n" +
        "\t\t\"note\":\"Note1\",\r\n\r\n" +
        "\t\t\"date\":\"2002-09-14T08:43:43.638\",\r\n\r\n" +
        "\t\t\"startTime\":\"2022-09-25T08:43:43.638\",\r\n\r\n" +
        "\t\t\"endTime\":\"2022-09-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"completed\": false,\r\n\r\n" +
        "\t\t\"doctorId\":\"58kydc-4947-4597-a9a9-7ed2766f6020\",\r\n\r\n" +
        "\t\t\"patientId\":\"jdtj155-4947-4597-a9a9-7ed2766f6020\",\r\n\r\n\t\t}\r\n\r\n" +
        "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
        "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "{\r\n\r\n\t\"errors\":{" +
        "\r\n\r\n\t\t\"appointmentId\":[\r\n\r\n\t\t\t\"Unexpected character encountered while parsing value: ,.Path 'appointmentId', line 8, position 18.\"\r\n\r\n\t\t\t" +
        "]\r\n\r\n\t" +
        "}," +
        "\r\n\r\n\t\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.1\"," +
        "\r\n\r\n\t\"title\":\"One or more validation errors occurred.\"," +
        "\r\n\r\n\t\"status\":400," +
        "\r\n\r\n\t\"traceId\":\"|e5d8e345-4d4f69de289f9ccd.\"}" +
        "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"You cannot schedule this appointment because doctor is on cofee break as usual.\",\r\n\r\n" +
        "\t\"token\":\"\",\r\n\r\n" +
        "\t\"result\": null,\r\n\r\n" +
        "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
        "}")]
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

        /// <summary>
        /// Gets a list of all appointments for given userId and time period - [Doctor, Patient]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     POST api/appointment/getallbyuser
        ///     {
        ///        "userId": "5ba08d2a-0a02-433f-a7d9-6f80880a5f01"
        ///        "startDate": "2022-09-20",
        ///        "endDate": "2022-09-22",
        ///     }
        /// </remarks>
        /// <param name="appointmentFilter"></param>  
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
        "\t\"token\":null,\r\n\r\n" +
        "\t\"result\":[\r\n\r\n\t{\r\n\r\n" +
        "\t\t\"id\":2,\r\n\r\n" +
        "\t\t\"note\":\"Note1\",\r\n\r\n" +
        "\t\t\"date\":\"2002-09-14T08:43:43.638\",\r\n\r\n" +
        "\t\t\"startTime\":\"2022-09-25T08:43:43.638\",\r\n\r\n" +
        "\t\t\"endTime\":\"2022-09-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"completed\": false,\r\n\r\n" +
        "\t\t\"doctorId\":\"rywh5648hwr - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n" +
        "\t\t\"patientId\":\"jdtj155 - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n\t\t},\r\n\r\n" +
        "\t\t{\r\n\r\n" +
        "\t\t\"id\":3,\r\n\r\n" +
        "\t\t\"note\":\"Note1\",\r\n\r\n" +
        "\t\t\"date\":\"2002-09-14T08:43:43.638\",\r\n\r\n" +
        "\t\t\"startTime\":\"2022-10-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"endTime\":\"2022-10-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"completed\": true,\r\n\r\n" +
        "\t\t\"doctorId\":\"rywh5648hwr - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n" +
        "\t\t\"patientId\":\"58kydc - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n\t\t}\r\n\r\n\t]" +
        "\r\n\r\n\t\"roles\":\"PATIENT\"\r\n\r\n" +
        "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"There is no appointments in databse.\",\r\n\r\n" +
        "\t\"token\":\"\",\r\n\r\n" +
        "\t\"result\": null,\r\n\r\n" +
        "\t\"roles\":\"PATIENT\"\r\n\r\n" +
        "}")]
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

        /// <summary>
        /// Gets the list of all Appointments - [Technician]
        /// </summary>
        /// <returns>The list of Appointments.</returns>
        // GET: api/appointment/getall
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
        "\t\"token\":null,\r\n\r\n" +
        "\t\"result\":[\r\n\r\n\t{\r\n\r\n" +
        "\t\t\"id\": 1,\r\n\r\n" +
        "\t\t\"note\":\"Note1\",\r\n\r\n" +
        "\t\t\"date\":\"2002-09-14T08:43:43.638\",\r\n\r\n" +
        "\t\t\"startTime\":\"2022-09-25T08:43:43.638\",\r\n\r\n" +
        "\t\t\"endTime\":\"2022-09-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"completed\": true,\r\n\r\n" +
        "\t\t\"doctorId\":\"58kydc - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n" +
        "\t\t\"patientId\":\"jdtj155 - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n\t\t},\r\n\r\n" +
        "\t\t{\r\n\r\n" +
        "\t\t\"id\":2,\r\n\r\n" +
        "\t\t\"note\":\"Note1\",\r\n\r\n" +
        "\t\t\"date\":\"2002-09-14T08:43:43.638\",\r\n\r\n" +
        "\t\t\"startTime\":\"2022-10-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"endTime\":\"2022-10-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"completed\": false,\r\n\r\n" +
        "\t\t\"doctorId\":\"rywh5648hwr - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n" +
        "\t\t\"patientId\":\"58kydc - 4947 - 4597 - a9a9 - 7ed2766f6020\",\r\n\r\n\t\t}\r\n\r\n\t]" +
        "\r\n\r\n\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
        "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"There is no appointments in databse.\",\r\n\r\n" +
        "\t\"token\":\"\",\r\n\r\n" +
        "\t\"result\": null,\r\n\r\n" +
        "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
        "}")]
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

        /// <summary>
        /// Get an Appointment with specific Id - [Technician]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     GET api/appointment/get/1
        /// </remarks>
        /// <param name="id"></param>   
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
        "\t\"token\":null,\r\n\r\n" +
        "\t\"result\":{\r\n\r\n" +
        "\t\t\"id\": 4,\r\n\r\n" +
        "\t\t\"note\":\"Note1\",\r\n\r\n" +
        "\t\t\"date\":\"2002-09-14T08:43:43.638\",\r\n\r\n" +
        "\t\t\"startTime\":\"2022-09-25T08:43:43.638\",\r\n\r\n" +
        "\t\t\"endTime\":\"2022-09-30T08:43:43.638\",\r\n\r\n" +
        "\t\t\"completed\":false,\r\n\r\n" +
        "\t\t\"doctorId\":\"58kydc-4947-4597-a9a9-7ed2766f6020\",\r\n\r\n" +
        "\t\t\"patientId\":\"jdtj155-4947-4597-a9a-7ed2766f6020\",\r\n\r\n\t\t}\r\n\r\n" +
        "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
        "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Unable to get Doctor because it doesn't exists.\",\r\n\r\n" +
        "\t\"token\":\"\",\r\n\r\n" +
        "\t\"result\": null,\r\n\r\n" +
        "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
        "}")]
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

        /// <summary>
        /// Delete an Appointment with specific Id - [Technician]
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     DELETE api/appointment/delete/1
        /// </remarks>
        /// <param name="id"></param>   
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = Constants.Technician)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = null, Description = "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": \"Appointment with id: 11 is deleted successfully.\",\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null)]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Unable to delete appointment, because it doesn't exists.\",\r\n\r\n" +
            "\t\"token\":\"\",\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
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
