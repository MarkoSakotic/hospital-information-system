using AutoMapper;
using DtoEntityProject;
using DtoEntityProject.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagedList;
using PagedList.Core;
using ServiceProject.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMVC.Controllers
{
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly IMapper _mapper;

        public PatientController(IPatientService patientService, IMapper mapper)
        {
            _patientService = patientService;
            _mapper = mapper;
        }


        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public IActionResult Add()
        {
            if (TempData["error"] != null)
            {
                ViewBag.Errors = TempData["error"].ToString();
            }
            PatientRequest request = new PatientRequest();
            request.DateOfBirth = DateTime.UtcNow.AddYears(-20).Date;
            return View(request);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpPost]
        public async Task<IActionResult> Add(PatientRequest patientRequest)
        {
            var response = await _patientService.AddAsync(patientRequest);

            if (response.Errors.Any())
            {
                TempData["error"] = response.Errors.FirstOrDefault();
                return RedirectToAction("Add");
            }

            return RedirectToAction("GetAll", (await _patientService.GetAllAsync()).Result);
        }

        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> GetAll(string searchString, string sortOrder, int page = 1, int pageSize = 10)
        {
            ViewData["FirstNameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "first_name_desc" : "";
            ViewData["LastNameSortParm"] = sortOrder == "LastName" ? "last_name_desc" : "LastName";
            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;

            IEnumerable<PatientResponse> patients = (await _patientService.GetAllAsync()).Result as IEnumerable<PatientResponse>;
            PagedList<PatientResponse> model;
            if (patients != null)
            {
                if (!String.IsNullOrEmpty(searchString))
                {
                    patients = patients.Where(d => d.FirstName.ToLower().StartsWith(searchString.ToLower())
                                           || d.LastName.ToLower().StartsWith(searchString.ToLower()));
                }
                if (patients.Any())
                {
                    switch (sortOrder)
                    {
                        case "LastName":
                            patients = patients.OrderBy(s => s.LastName);
                            break;
                        case "last_name_desc":
                            patients = patients.OrderByDescending(s => s.LastName);
                            break;
                        case "first_name_desc":
                            patients = patients.OrderByDescending(s => s.FirstName);
                            break;
                        default:
                            patients = patients.OrderBy(s => s.FirstName);
                            break;
                    }
                    model = new PagedList<PatientResponse>(patients.AsQueryable(), page, pageSize);
                    return View(model);
                }
            }
            model = new PagedList<PatientResponse>(null, 1, 1);
            return View(model);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            if (TempData["error"] != null)
            {
                ViewBag.Errors = TempData["error"].ToString();
            }
            PatientUpdate patient = _mapper.Map<PatientUpdate>((await _patientService.GetAsync(id)).Result);
            return View(patient);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpPost]
        public async Task<IActionResult> Update(PatientUpdate patientUpdate)
        {
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                var response = await _patientService.UpdateAsync(patientUpdate);

                if (response.Errors.Any())
                {
                    TempData["error"] = response.Errors.FirstOrDefault();

                    return RedirectToAction("Update", "Patient", patientUpdate.Id);
                }
                return RedirectToAction("GetAll", (await _patientService.GetAllAsync()).Result);
            }
            return BadRequest();
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpPost]
        public async Task<IActionResult> DeletePatient(string id)
        {
            await _patientService.DeleteAsync(id);
            return RedirectToAction("GetAll", (await _patientService.GetAllAsync()).Result);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {

            var patient = ((await _patientService.GetAsync(id)).Result) as PatientResponse;
            return View(patient);
        }

        [Authorize(Roles = Constants.Technician + ", " + Constants.Doctor)]
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            PatientResponse patient = (await _patientService.GetAsync(id)).Result as PatientResponse;
            return View(patient);
        }

        [Authorize(Roles = Constants.Doctor)]
        [HttpGet]
        public async Task<IActionResult> GetPatientsByDoctor(string searchString, string sortOrder, int page = 1, int pageSize = 10)
        {

            ViewData["FirstNameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "first_name_desc" : "";
            ViewData["LastNameSortParm"] = sortOrder == "LastName" ? "last_name_desc" : "LastName";
            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;

            IEnumerable<PatientResponse> patients = (await _patientService.GetPatientsByDoctorAsync()).Result as IEnumerable<PatientResponse>;
            PagedList<PatientResponse> model;
            if (patients != null)
            {
                if (!String.IsNullOrEmpty(searchString))
                {
                    patients = patients.Where(d => d.FirstName.ToLower().StartsWith(searchString.ToLower())
                                           || d.LastName.ToLower().StartsWith(searchString.ToLower()));
                }
                if (patients.Any())
                {
                    switch (sortOrder)
                    {
                        case "LastName":
                            patients = patients.OrderBy(s => s.LastName);
                            break;
                        case "last_name_desc":
                            patients = patients.OrderByDescending(s => s.LastName);
                            break;
                        case "first_name_desc":
                            patients = patients.OrderByDescending(s => s.FirstName);
                            break;
                        default:
                            patients = patients.OrderBy(s => s.FirstName);
                            break;
                    }
                    model = new PagedList<PatientResponse>(patients.AsQueryable(), page, pageSize);
                    return View(model);
                }
            }
            model = new PagedList<PatientResponse>(null, 1, 1);
            return View(model);
        }
    }
}
