using DtoEntityProject;
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
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("login")]
        [Consumes("application/json")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            ApiResponse response = new ApiResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    response = await _identityService.LoginAsync(request);

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
            else
            {
                response.Errors.AddRange((IEnumerable<string>)ModelState.Values.SelectMany(e => e.Errors));
                return BadRequest(ModelState);
            }
        }

        [HttpPost("resetpassword")]
        [Consumes("application/json")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto request)
        {
            ApiResponse response = new ApiResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    response = await _identityService.ResetPasswordAsync(request);

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
            else
            {
                response.Errors.AddRange((IEnumerable<string>)ModelState.Values.SelectMany(e => e.Errors));
                return BadRequest(ModelState);
            }
        }
    }
}
