using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace ServiceProject.Utility
{
    public class JwtParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtParser()
        {

        }

        public JwtParser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual string GetIdFromJWT()
        {
            var claims = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            string result;
            try
            {
                result = claims.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (result.Contains("@"))
                {
                    result = claims.FindFirst("id").Value;
                }
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public virtual string GetRoleFromJWT()
        {
            var claims = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            string result;
            try
            {
                result = claims.FindFirst(ClaimTypes.Role).Value;
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
