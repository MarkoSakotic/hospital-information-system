using AutoMapper;
using DtoEntityProject;
using DtoEntityProject.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using ServiceProject.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebMVC.Models;

namespace WebMVC.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IMapper _mapper;


        public DoctorController(IDoctorService doctorService, IMapper mapper)
        {
            _doctorService = doctorService;
            _mapper = mapper;
        }

        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> GetAll(string searchString, string sortOrder, int page = 1, int pageSize = 10)
        {

            ViewData["FirstNameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "first_name_desc" : "";
            ViewData["LastNameSortParm"] = sortOrder == "LastName" ? "last_name_desc" : "LastName";
            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;
            TempData["create"] = null;

            IEnumerable<DoctorResponse> doctors = (await _doctorService.GetAllAsync()).Result as IEnumerable<DoctorResponse>;
            PagedList<DoctorResponse> model;
            if (doctors != null)
            {
                if (TempData["error"] != null)
                {
                    ViewBag.Errors = TempData["error"].ToString();
                }
                if (!String.IsNullOrEmpty(searchString))
                {
                    doctors = doctors.Where(d => d.FirstName.ToLower().StartsWith(searchString.ToLower())
                                           || d.LastName.ToLower().StartsWith(searchString.ToLower()));
                }
                if (doctors.Any())
                {
                    switch (sortOrder)
                    {
                        case "LastName":
                            doctors = doctors.OrderBy(s => s.LastName);
                            break;
                        case "last_name_desc":
                            doctors = doctors.OrderByDescending(s => s.LastName);
                            break;
                        case "first_name_desc":
                            doctors = doctors.OrderByDescending(s => s.FirstName);
                            break;
                        default:
                            doctors = doctors.OrderBy(s => s.FirstName);
                            break;
                    }
                    model = new PagedList<DoctorResponse>(doctors.AsQueryable(), page, pageSize);
                    return View(model);
                }
            }
            model = new PagedList<DoctorResponse>(null, 1, 1);
            return View(model);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpPost]
        public async Task<IActionResult> DeleteDoctor(string id)
        {
            await _doctorService.DeleteAsync(id);
            var doctors = (await _doctorService.GetAllAsync()).Result;
            return RedirectToAction("GetAll", doctors);
        }


        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var doctor = ((await _doctorService.GetAsync(id)).Result) as DoctorResponse;
            return View(doctor);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public IActionResult Add()
        {
            if (TempData["error"] != null)
            {
                ViewBag.Errors = TempData["error"].ToString();
            }

            DoctorRequest request = new DoctorRequest();
            request.DateOfBirth = DateTime.UtcNow.AddYears(-20).Date;
            return View(request);

        }

        [Authorize(Roles = Constants.Technician)]
        [HttpPost]
        public async Task<IActionResult> Add(DoctorRequest doctorRequest)
        {
            var response = await _doctorService.AddAsync(doctorRequest);

            if (response.Errors.Any())
            {
                TempData["error"] = response.Errors.FirstOrDefault();
                return RedirectToAction("Add");
            }

            var doctors = (await _doctorService.GetAllAsync()).Result;
            return RedirectToAction("GetAll", doctors);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {

            if (TempData["error"] != null)
            {
                ViewBag.Errors = TempData["error"].ToString();
            }
            DoctorUpdate doctor = _mapper.Map<DoctorUpdate>((await _doctorService.GetAsync(id)).Result);
            return View(doctor);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpPost]
        public async Task<IActionResult> Update(DoctorUpdate doctorUpdate)
        {
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                var response = await _doctorService.UpdateAsync(doctorUpdate);

                if (response.Errors.Any())
                {
                    TempData["error"] = response.Errors.FirstOrDefault();

                    return RedirectToAction("Update", "Doctor", doctorUpdate.Id);
                }

                return RedirectToAction("GetAll", (await _doctorService.GetAllAsync()).Result);
            }

            return BadRequest();
        }

        [Authorize(Roles = Constants.Technician + ", " + Constants.Patient)]
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            DoctorResponse doctor = (await _doctorService.GetAsync(id)).Result as DoctorResponse;
            return View(doctor);
        }

        [Authorize(Roles = Constants.Technician + ", " + Constants.Patient)]
        [HttpPost]
        public IActionResult Details()
        {
            return RedirectToAction("GetAll");
        }

        [Authorize(Roles = Constants.Patient)]
        [HttpGet]
        public async Task<IActionResult> GetDoctorsByPatient(string searchString, string sortOrder, int page = 1, int pageSize = 10)
        {
            ViewData["FirstNameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "first_name_desc" : "";
            ViewData["LastNameSortParm"] = sortOrder == "LastName" ? "last_name_desc" : "LastName";
            ViewBag.SearchString = searchString;
            ViewBag.Page = page;
            ViewBag.SortOrder = sortOrder;

            IEnumerable<DoctorResponse> doctors = (await _doctorService.GetDoctorsByPatientAsync()).Result as IEnumerable<DoctorResponse>;
            PagedList<DoctorResponse> model;

            if (doctors != null)
            {
                if (!String.IsNullOrEmpty(searchString))
                {
                    doctors = doctors.Where(d => d.FirstName.ToLower().StartsWith(searchString.ToLower())
                                           || d.LastName.ToLower().StartsWith(searchString.ToLower()));
                }
                if (doctors.Any())
                {
                    switch (sortOrder)
                    {
                        case "LastName":
                            doctors = doctors.OrderBy(s => s.LastName);
                            break;
                        case "last_name_desc":
                            doctors = doctors.OrderByDescending(s => s.LastName);
                            break;
                        case "first_name_desc":
                            doctors = doctors.OrderByDescending(s => s.FirstName);
                            break;
                        default:
                            doctors = doctors.OrderBy(s => s.FirstName);
                            break;
                    }
                    model = new PagedList<DoctorResponse>(doctors.AsQueryable(), page, pageSize);
                    return View(model);
                }
            }
            model = new PagedList<DoctorResponse>(null, 1, 1);
            return View(model);
        }
    }
}
