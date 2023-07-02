using DtoEntityProject.Configuration;
using EntityProject;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProject.TokenGenerator
{
    public sealed class TokenGenerator : ITokenGenerator
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApiUser> _userManager;

        public TokenGenerator(IOptions<JwtSettings> jwtConfiguration, UserManager<ApiUser> userManager)
        {
            _jwtSettings = jwtConfiguration.Value;
            _userManager = userManager;
        }

        public async Task<string> GenerateToken(string email, string id)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var role = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, email),
                    new Claim("id", id),
                    new Claim("roles", role[0])
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
