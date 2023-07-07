using DtoEntityProject;
using EntityProject;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceProject.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApiUser> _signInManager;

        public AccountController(IPatientService patientService, UserManager<ApiUser> userManager, SignInManager<ApiUser> signInManager, IDoctorService doctorService)
        {

            _signInManager = signInManager;
        }

        public IActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {


            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
                if (result.Succeeded)
                {
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        Redirect(Request.Query["ReturnUrl"].First());
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ModelState.AddModelError("", "Failed to login");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {

            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
