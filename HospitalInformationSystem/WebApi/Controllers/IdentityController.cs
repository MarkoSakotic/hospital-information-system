using DtoEntityProject;
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
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;

        }

        /// <summary>
        /// Enables user to login into system
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     POST api/identity/login
        ///     {
        ///       "email": "mark@example.com",
        ///       "password": "Mark.123",
        ///     }
        /// </remarks>
        /// <param name="request"></param>   
        [HttpPost("login")]
        [SwaggerResponse(200, "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\":\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJqYW5lQHZoaXMuY29tIiwianRpIjoiYTZmNDMzODAtZjQwMy00NTAyLThmYTMtOGEzZWQ3MDMyYjVhIiwiZW1haWwiOiJqYW5lQHZoaXMuY29tIiwiaWQiOiI0Yjc0YTFiNi1hZTE0LTQwOTAtOTc5Yy1mYTU0NDNkNjRkM2EiLCJyb2xlcyI6IlRFQ0hOSUNJQU4iLCJuYmYiOjE2NjM5Mzg3MDMsImV4cCI6MTY2Mzk0MjMwMywiaWF0IjoxNjYzOTM4NzAzfQ.IO9dzJZCfqW_wv1cVl1yaxrKGGW21PQ2YByyI4w_qEw\",\r\n\r\n" +
            "\t\"result\":{\r\n\r\n" +
            "\t\t\"seniority\":\"Admin\",\r\n\r\n" +
            "\t\t\"id\": \"4b74a1b6-ae14-4090-979c-fa5443d64d3a\",\r\n\r\n" +
            "\t\t\"firstName\": \"Jane\",\r\n\r\n" +
            "\t\t\"lastName\": \"Doe\",\r\n\r\n" +
            "\t\t\"email\": \"jane@vhis.com\",\r\n\r\n" +
            "\t\t\"address\": \"350 Fifth Avenue, Manhattan\",\r\n\r\n" +
            "\t\t\"phone:\": \"123-456-789\",\r\n\r\n" +
            "\t\t\"dateOfBirth\": \"1992-09-21T11:20:13.763261\"}\r\n\r\n" +
            "\t\"roles\":\"TECHNICIAN\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "{\r\n\r\n\t\"errors\":{" +
            "\r\n\r\n\t\t\"Email\":[" +
            "\r\n\r\n\t\t\t\"The Email field is requiered.,\"\r\n\t\t\t" +
            "\r\n\r\n\t\t\t\"The Email field is not a valid e-mail address.\"\r\n\r\n\t\t\t" +
            "]\r\n\r\n\t" +
            "}," +
            "\r\n\r\n\t\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.1\"," +
            "\r\n\r\n\t\"title\":\"One or more validation errors occurred.\"," +
            "\r\n\r\n\t\"status\":400," +
            "\r\n\r\n\t\"traceId\":\"|e5d8e345-4d4f69de289f9ccd.\"}" +
            "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"User does not exists.\",\r\n\r\n" +
            "\t\"token\": null,\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"[]\"\r\n\r\n" +
            "}")]
        [Consumes("application/json")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            ApiResponse response = new ApiResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    response = await _identityService.LoginAsync(request);

                    if (response.Errors.Any())
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
                response.Errors.AddRange(ModelState.Values.SelectMany(e => e.Errors) as IEnumerable<string>);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Enables user to reset their password  // Password should be at least 8 characters long, and contain upper case, number and special character.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///    
        ///     POST api/identity/login
        ///     {
        ///       "email": "mark@example.com",
        ///       "oldPassword": "Mark.123",
        ///       "newPassword": "Mark.321",
        ///       "confirmNewPassword": "Mark.321",
        ///     }
        /// </remarks>
        /// <param name="request"></param>   
        [HttpPost("resetpassword")]
        [SwaggerResponse(200, "{\r\n\r\n\t\"errors\":[],\r\n\r\n" +
            "\t\"token\": null,\r\n\r\n" +
            "\t\"result\": \"Password has been reseted successfully.\",\r\n\r\n" +
            "\t\"roles\":\"PATIENT\"\r\n\r\n" +
            "}")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = null, Description = "{\r\n\r\n\t\"errors\":{" +
            "\r\n\r\n\t\t\"ConfirmNewPassword\":[" +
            "\r\n\r\n\t\t\t\"ConfirmNewPassword and 'NewPassword' do not match.\"\r\n\t\t\t" +
            "]\r\n\r\n\t" +
            "}," +
            "\r\n\r\n\t\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.1\"," +
            "\r\n\r\n\t\"title\":\"One or more validation errors occurred.\"," +
            "\r\n\r\n\t\"status\":400," +
            "\r\n\r\n\t\"traceId\":\"|e5d8e345-4d4f69de289f9ccd.\"}" +
            "\r\n\r\n}")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = null, Description = "{\r\n\r\n\t\"errors\":\"Unable to change password for user with email: user13@vhis.com.\",\r\n\r\n" +
            "\t\"token\": null,\r\n\r\n" +
            "\t\"result\": null,\r\n\r\n" +
            "\t\"roles\":\"PATIENT\"\r\n\r\n" +
            "}")]
        [Consumes("application/json")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto request)
        {
            ApiResponse response = new ApiResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    response = await _identityService.ResetPasswordAsync(request);

                    if (response.Errors.Any())
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
                response.Errors.AddRange(ModelState.Values.SelectMany(e => e.Errors) as IEnumerable<string>);
                return BadRequest(ModelState);
            }
        }
    }
}
