using DtoEntityProject.Constants;
using EntityProject;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceProject.Seeder
{
    public class TechnicianSeeder
    {
        public async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            RoleManager<IdentityRole> roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
            UserManager<ApiUser> _userManager = serviceProvider.GetService<UserManager<ApiUser>>();

            string[] allRoles = new string[] { Constants.Technician, Constants.Doctor, Constants.Patient };

            foreach (string role in allRoles)
            {
                if (!roleManager.Roles.Any(r => r.Name == role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var user = new Technician
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@vhis.com",
                Password = "P@ssw0rd",
                UserName = "jane@vhis.com",
                Phone = "123-456-789",
                Seniority = "Admin",
                Address = "350 Fifth Avenue, Manhattan",
                DateOfBirth = DateTime.Now.AddYears(-30),
                EmailConfirmed = true
            };

            if (!_userManager.Users.Any(u => u.UserName == user.UserName))
            {
                var password = new PasswordHasher<ApiUser>();
                var hashed = password.HashPassword(user, "P@ssw0rd");
                user.PasswordHash = hashed;

                await _userManager.CreateAsync(user);
            }

            await AssignRoles(serviceProvider, _userManager, user.Email, Constants.Technician);
        }

        private static async Task<IdentityResult> AssignRoles(IServiceProvider services, UserManager<ApiUser> userManager, string email, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            return await userManager.AddToRoleAsync(user, role);
        }
    }
}
