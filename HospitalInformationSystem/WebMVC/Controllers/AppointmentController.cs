using AutoMapper;
using DtoEntityProject;
using DtoEntityProject.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PagedList.Core;
using ServiceProject.Interface;
using ServiceProject.Utility;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMVC.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;
        private readonly JwtParser _jwtParser;

        public AppointmentController(IAppointmentService appointmentService, IPatientService patientService, IDoctorService doctorService, JwtParser jwtParser)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
            _doctorService = doctorService;
            _jwtParser = jwtParser;
        }

        [Authorize(Roles = Constants.Technician + "," + Constants.Patient + "," + Constants.Doctor)]
        public async Task<IActionResult> GetAll(string buttonFilter, string searchString, string sortOrder, string filter, string startDate, string endDate, int page = 1, int pageSize = 10)
        {
            var role = _jwtParser.GetRoleFromJWT();
            var userId = _jwtParser.GetIdFromJWT();

            switch (buttonFilter)
            {
                case "clearFilter":
                    filter = "NoFilter";
                    break;
                default:
                    break;
            }

            IEnumerable<AppointmentResponse> appointments = (await _appointmentService.GetAllAppointmentsAsync()).Result as IEnumerable<AppointmentResponse>;

            var noFilterList = appointments;

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "doctor_name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CompletedSortParm"] = sortOrder == "Completed" ? "completed_desc" : "Completed";
            ViewData["PatientSortParm"] = sortOrder == "Patient" ? "patient_desc" : "Patient";
            ViewData["CreatedOnSortParm"] = sortOrder == "CreatedOn" ? "createdOn_desc" : "CreatedOn";
            ViewData["NoteSortParm"] = sortOrder == "Note" ? "note_desc" : "Note";
            ViewData["filter"] = filter;
            ViewData["searchString"] = searchString;
            if (startDate == null && endDate == null && appointments != null)
            {
                startDate = appointments.Min(a => a.StartTime).ToString("yyyy-MM-dd");
                endDate = appointments.Max(a => a.StartTime).ToString("yyyy-MM-dd");
            }

            ViewBag.UserId = userId;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;
            ViewBag.Filter = filter;
            ViewBag.SearchString = searchString;

            PagedList<AppointmentResponse> model;
            List<SelectListItem> selectList = new List<SelectListItem>();

            var all = new SelectListItem()
            {
                Value = "All",
                Text = "All"
            };
            var completed = new SelectListItem()
            {
                Value = "Completed",
                Text = "Completed"
            };
            var notCompleted = new SelectListItem()
            {
                Value = "NotCompleted",
                Text = "Not Completed"
            };
            var noFilter = new SelectListItem()
            {
                Value = "NoFilter",
                Text = "No filter"
            };

            selectList.Insert(0, all);
            selectList.Insert(1, completed);
            selectList.Insert(2, notCompleted);
            selectList.Insert(3, noFilter);

            if (role != Constants.Patient)
            {
                var scheduled = new SelectListItem()
                {
                    Value = "Scheduled",
                    Text = "Scheduled"
                };
                var coffee = new SelectListItem()
                {
                    Value = "Coffee",
                    Text = "Coffee Break"
                };
                var free = new SelectListItem()
                {
                    Value = "Free",
                    Text = "Free"
                };

                selectList.Insert(4, scheduled);
                selectList.Insert(5, coffee);
                selectList.Insert(6, free);
            }

            ViewBag.Filters = selectList;



            if (TempData["error"] != null)
            {
                ViewBag.Errors = TempData["error"].ToString();

            }
            if (appointments != null)
            {
                if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
                {
                    ApiResponse result;

                    if (role == Constants.Technician)
                    {
                        result = await _appointmentService.GetAllAppointmentsForUserWithinGivenPeriodAsync(new AppointmentFilter() { StartDate = DateTime.Parse(startDate), EndDate = DateTime.Parse(endDate), UserId = null });

                    }
                    else
                    {
                        result = await _appointmentService.GetAllAppointmentsForUserWithinGivenPeriodAsync(new AppointmentFilter() { StartDate = DateTime.Parse(startDate), EndDate = DateTime.Parse(endDate), UserId = userId });
                    }

                    if (result.Result == null)
                    {
                        ViewBag.Errors = result.Errors.FirstOrDefault();
                    }
                    else
                    {
                        appointments = (result.Result) as IEnumerable<AppointmentResponse>;
                    }

                }
                if (!String.IsNullOrEmpty(searchString))
                {
                    var temp = appointments;

                    if (role != Constants.Doctor)
                    {
                        appointments = appointments.Where(s => s.Doctor.FirstName.ToLower().StartsWith(searchString.ToLower())
                                               || s.Doctor.LastName.ToLower().StartsWith(searchString.ToLower()));
                    }
                    else
                    {
                        appointments = appointments.Where(s => s.PatientId != null && s.Patient != null);
                        appointments = appointments.Where(s => s.Patient.FirstName.ToLower().StartsWith(searchString.ToLower())
                                               || s.Patient.LastName.ToLower().StartsWith(searchString.ToLower()));
                    }
                    if (!appointments.Any())
                    {
                        ViewBag.Errors = "There is no appointments for given filter.";
                        ViewBag.SearchString = null;
                        appointments = temp;
                    }
                }
                var apps = appointments;

                switch (filter)
                {
                    case "Scheduled":
                        appointments = appointments.Where(a => a.PatientId != null);
                        break;
                    case "Coffee":
                        appointments = appointments.Where(a => a.Note == "Coffee Break");
                        break;
                    case "Free":
                        appointments = appointments.Where(a => a.PatientId == null && !a.Completed);
                        break;
                    case "Completed":
                        appointments = appointments.Where(a => a.Completed);
                        break;
                    case "NotCompleted":
                        appointments = appointments.Where(a => !a.Completed && a.PatientId != null);
                        break;
                    case "NoFilter":
                        appointments = noFilterList;
                        ViewData["searchString"] = null;
                        break;
                    default:
                        break;
                }



                if (!appointments.Any())
                {
                    ViewBag.Errors = "There is no appointments for given filter.";
                    appointments = apps;
                }

                switch (sortOrder)
                {
                    case "Date":
                        appointments = appointments.OrderBy(s => s.StartTime);
                        break;
                    case "date_desc":
                        appointments = appointments.OrderByDescending(s => s.StartTime);
                        break;
                    case "Completed":
                        appointments = appointments.OrderBy(s => s.Completed.ToString());
                        break;
                    case "completed_desc":
                        appointments = appointments.OrderByDescending(s => s.Completed.ToString());
                        break;
                    case "doctor_name_desc":
                        appointments = appointments.OrderByDescending(s => s.Doctor.FirstName);
                        break;
                    case "patient_desc":
                        var sortDesc = appointments.Where(a => a.Patient != null).OrderByDescending(a => a.Patient.FirstName).ToList();
                        sortDesc.AddRange(appointments.Where(a => a.Patient == null).ToList());
                        appointments = sortDesc;
                        break;
                    case "Patient":
                        var sortAsc = appointments.Where(a => a.Patient != null).OrderBy(a => a.Patient.FirstName).ToList();
                        sortAsc.AddRange(appointments.Where(a => a.Patient == null));
                        appointments = sortAsc;
                        break;
                    case "createdOn_desc":
                        appointments = appointments.OrderByDescending(s => s.Date);
                        break;
                    case "CreatedOn":
                        appointments = appointments.OrderBy(s => s.Date);
                        break;
                    case "note_desc":
                        var sortNoteDesc = appointments.Where(a => a.Note != "" || a.Note != " ").OrderByDescending(a => a.Note).ToList();
                        sortNoteDesc.AddRange(appointments.Where(a => a.Note == "" || a.Note == null).ToList());
                        appointments = sortNoteDesc;
                        break;
                    case "Note":
                        var sortNoteAsc = appointments.Where(a => a.Note != "").OrderBy(a => a.Note).ToList();
                        sortNoteAsc.AddRange(appointments.Where(a => a.Note == "" || a.Note == null));
                        appointments = sortNoteAsc;
                        break;
                    default:
                        appointments = appointments.OrderBy(s => s.Doctor.FirstName);
                        break;
                }

                startDate = appointments.Min(a => a.StartTime).ToString("yyyy-MM-dd");
                endDate = appointments.Max(a => a.StartTime).ToString("yyyy-MM-dd");
                ViewData["startDate"] = startDate;
                ViewData["endDate"] = endDate;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;

                if (appointments.Any())
                {
                    model = new PagedList<AppointmentResponse>(appointments.AsQueryable(), page, pageSize);
                    return View(model);
                }
            }
            model = new PagedList<AppointmentResponse>(null, 1, 1);
            return View(model);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public async Task<IActionResult> Create(string doctorId)
        {
            var doctor = (await _doctorService.GetAsync(doctorId)).Result as DoctorResponse;
            ViewBag.DoctorName = doctor.FirstName + " " + doctor.LastName;

            if (TempData["error"] != null)
            {
                ViewBag.Errors = TempData["error"].ToString();
            }

            AppointmentRequest request = new AppointmentRequest();
            request.DoctorId = doctorId;
            request.Duration = 30;
            request.StartDate = DateTime.UtcNow;
            request.EndDate = DateTime.UtcNow.AddDays(7);
            request.HoursPerDay = doctor.HoursPerDay;

            return View(request);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> Create(AppointmentRequest request)
        {
            ApiResponse response = await _appointmentService.CreateAppointmentsAsync(request);
            var appointments = response.Result as IEnumerable<AppointmentResponse>;
            TempData["startTime"] = appointments.First().StartTime;
            TempData["endTime"] = appointments.Last().StartTime;

            TempData["create"] = true;
            TempData["startDate"] = request.StartDate;
            TempData["endDate"] = request.EndDate;
            TempData["doctorId"] = request.DoctorId;

            var doctorId = (response.Result as IEnumerable<AppointmentResponse>).FirstOrDefault().DoctorId;

            if (response.Errors.Any())
            {
                if (response.Result == null)
                {
                    TempData["error"] = response.Errors.FirstOrDefault();
                    return RedirectToAction("Create", "Appointment", new { doctorId = request.DoctorId });
                }
                TempData["error"] = "Some of appointments are not added because they already exist.";
                return RedirectToAction("GetAllForDoctor", "Appointment", new { doctorId = doctorId });
            }
            return RedirectToAction("GetAllForDoctor", "Appointment", new { doctorId = doctorId });
        }

        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> ScheduledForPatient(string patientId, string searchString, string sortOrder, string filter, string startDate, string endDate, int page = 1, int pageSize = 10)
        {
            IEnumerable<AppointmentResponse> appointments = ((await _appointmentService.GetAllAppointmentsAsync()).Result as IEnumerable<AppointmentResponse>)
                .Where(a => a.PatientId == patientId);
            var noFilterList = appointments;

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "doctor_name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CreatedOnSortParm"] = sortOrder == "CreatedOn" ? "createdOn_desc" : "CreatedOn";
            ViewData["filter"] = filter;
            ViewData["searchString"] = searchString;
            if (startDate == null && endDate == null)
            {
                startDate = appointments.Min(a => a.StartTime).ToString("yyyy-MM-dd");
                endDate = appointments.Max(a => a.StartTime).ToString("yyyy-MM-dd");
            }

            ViewBag.patientId = patientId;
            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;
            ViewBag.Filter = filter;

            List<SelectListItem> selectList = new List<SelectListItem>();
            var all = new SelectListItem()
            {
                Value = "All",
                Text = "All"
            };
            var completed = new SelectListItem()
            {
                Value = "Completed",
                Text = "Completed"
            };
            var notCompleted = new SelectListItem()
            {
                Value = "NotCompleted",
                Text = "Not Completed"
            };
            var noFilter = new SelectListItem()
            {
                Value = "NoFilter",
                Text = "No filter"
            };
            selectList.Insert(0, all);
            selectList.Insert(1, completed);
            selectList.Insert(2, notCompleted);
            selectList.Insert(3, noFilter);
            ViewBag.Filters = selectList;

            var patient = (await _patientService.GetAsync(patientId)).Result as PatientResponse;
            PagedList<AppointmentResponse> model;
            if (appointments != null)
            {
                if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
                {

                    var result = await _appointmentService.GetAllAppointmentsForUserWithinGivenPeriodAsync(new AppointmentFilter() { StartDate = DateTime.Parse(startDate), EndDate = DateTime.Parse(endDate), UserId = null });
                    if (result.Result == null)
                    {
                        ViewBag.Errors = result.Errors.FirstOrDefault();
                    }
                    else
                    {
                        appointments = ((result.Result) as IEnumerable<AppointmentResponse>).Where(s => s.PatientId == patientId);
                    }
                }
                if (!String.IsNullOrEmpty(searchString))
                {
                    var apps = appointments;

                    appointments = appointments.Where(s => s.Doctor.FirstName.ToLower().StartsWith(searchString.ToLower())
                                                    || s.Doctor.LastName.ToLower().StartsWith(searchString.ToLower()));
                    if (!appointments.Any())
                    {
                        ViewBag.Errors = "There is no appointments for given filter.";
                        ViewBag.SearchString = null;
                        appointments = apps;
                    }

                }
                if (appointments.Any())
                {
                    ViewBag.PatientName = patient.FirstName + " " + patient.LastName;
                    ViewBag.PatientId = patient.Id;
                    var apps = appointments;

                    switch (filter)
                    {
                        case "Completed":
                            appointments = appointments.Where(a => a.Completed);
                            break;
                        case "NotCompleted":
                            appointments = appointments.Where(a => !a.Completed);
                            break;
                        case "NoFilter":
                            appointments = noFilterList;
                            ViewData["searchString"] = null;
                            break;
                        default:
                            break;
                    }

                    if (!appointments.Any())
                    {
                        ViewBag.Errors = "There is no appointments for given filter.";
                        appointments = apps;
                    }

                    switch (sortOrder)
                    {
                        case "Date":
                            appointments = appointments.OrderBy(s => s.StartTime);
                            break;
                        case "date_desc":
                            appointments = appointments.OrderByDescending(s => s.StartTime);
                            break;
                        case "doctor_name_desc":
                            appointments = appointments.OrderByDescending(s => s.Doctor.FirstName);
                            break;
                        case "createdOn_desc":
                            appointments = appointments.OrderByDescending(s => s.Date);
                            break;
                        case "CreatedOn":
                            appointments = appointments.OrderBy(s => s.Date);
                            break;
                        default:
                            appointments = appointments.OrderBy(s => s.Doctor.FirstName);
                            break;
                    }


                    startDate = appointments.Min(a => a.StartTime).ToString("yyyy-MM-dd");
                    endDate = appointments.Max(a => a.StartTime).ToString("yyyy-MM-dd");
                    ViewData["startDate"] = startDate;
                    ViewData["endDate"] = endDate;
                    ViewBag.StartDate = startDate;
                    ViewBag.EndDate = endDate;
                    model = new PagedList<AppointmentResponse>(appointments.AsQueryable(), page, pageSize);
                    return View(model);
                }
            }
            ViewBag.PatientName = patient.FirstName + " " + patient.LastName;
            ViewBag.PatientId = patient.Id;
            model = new PagedList<AppointmentResponse>(null, 1, 1);
            return RedirectToAction("GetAll", "Patient", model);

        }

        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public async Task<IActionResult> Schedule(int id)
        {
            AppointmentResponse app = (await _appointmentService.GetAppointmentAsync(id)).Result as AppointmentResponse;
            AppointmentSchedule request = new AppointmentSchedule();
            request.AppointmentId = id;
            request.Note = app.Note;
            TempData["schedule"] = false;
            List<PatientResponse> patients = (await _patientService.GetAllAsync()).Result as List<PatientResponse>;


            if (TempData["error"] != null)
            {
                ViewBag.Error = TempData["error"];
            }
            if (patients == null)
            {
                TempData["error"] = "There is no patients.";
                return RedirectToAction("GetAllForDoctor", "Appointment");
            }
            var selectListItem = new SelectListItem()
            {
                Value = null,
                Text = "Select..."
            };
            var selectList = new SelectList((from p in patients
                                             select new
                                             {
                                                 Id = p.Id,
                                                 FullName = p.FirstName + " " + p.LastName
                                             }),
                                                "Id",
                                                "FullName",
                                                null).ToList();
            selectList.Insert(0, selectListItem);
            ViewBag.Patients = selectList;
            ViewBag.Appointment = app;
            return View(request);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpPost]
        public async Task<IActionResult> Schedule(AppointmentSchedule request)
        {
            var response = await _appointmentService.ScheduleAppointmentAsync(request);
            if (response.Result == null)
            {
                TempData["error"] = response.Errors.FirstOrDefault();
                if (TempData["schedule"] != null)
                {
                    bool? res = TempData["schedule"] as bool?;
                    if ((bool)res)
                    {
                        return RedirectToAction("ScheduleFromPatients", new { id = request.AppointmentId, patientId = request.PatientId });
                    }
                    else
                    {
                        return RedirectToAction("Schedule", new { id = request.AppointmentId });
                    }
                }
            }

            return RedirectToAction("ScheduledForPatient", "Appointment", new { patientId = request.PatientId });
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            AppointmentResponse app = (await _appointmentService.GetAppointmentAsync(id)).Result as AppointmentResponse;

            return View(app);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpPost]
        public async Task<IActionResult> Details()
        {
            return RedirectToAction("GetAll", (await _appointmentService.GetAllAppointmentsAsync()).Result);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public async Task<IActionResult> Update(int id, string patientId)
        {
            AppointmentResponse app = (await _appointmentService.GetAppointmentAsync(id)).Result as AppointmentResponse;
            List<PatientResponse> patients = (await _patientService.GetAllAsync()).Result as List<PatientResponse>;

            if (TempData["error"] != null)
            {
                ViewBag.Error = TempData["error"];
            }

            var selectListItem = new SelectListItem()
            {
                Value = "",
                Text = "Select..."
            };
            var selectList = new SelectList((from p in patients
                                             select new
                                             {
                                                 Id = p.Id,
                                                 FullName = p.FirstName + " " + p.LastName
                                             }),
                                                "Id",
                                                "FullName",
                                                null).ToList();
            selectList.Insert(0, selectListItem);
            ViewBag.Patients = selectList;
            return View(app);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpPost]
        public async Task<IActionResult> Update(AppointmentResponse app)
        {
            AppointmentUpdate upd = new AppointmentUpdate()
            {
                Id = app.Id,
                Note = app.Note,
                PatientId = app.PatientId
            };

            if (ModelState.IsValid)
            {
                if (app.Note == "Coffee Break" && app.PatientId != null)
                {
                    TempData["error"] = "Unable to update appointment because doctor is on coffee break.";
                    return RedirectToAction("Update", app.Id);
                }

                await _appointmentService.UpdateAppointmentAsync(upd);
                return RedirectToAction("GetAll", (await _appointmentService.GetAllAppointmentsAsync()).Result);
            }
            return BadRequest();
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpPost]
        public async Task<IActionResult> DeleteApp(int id)
        {
            await _appointmentService.DeleteAppointmentAsync(id);

            return RedirectToAction("GetAll");
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            AppointmentResponse appointment = ((await _appointmentService.GetAppointmentAsync(id)).Result) as AppointmentResponse;

            return View(appointment);
        }

        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> SchedulePatient(int id, string patientId)
        {

            AppointmentSchedule app = new AppointmentSchedule()
            {
                PatientId = patientId,
                AppointmentId = id
            };


            var response = await _appointmentService.ScheduleAppointmentAsync(app);
            if (response.Result == null)
            {
                return RedirectToAction("GetAll", (await _appointmentService.GetAllAppointmentsAsync()).Result);
            }
            return RedirectToAction("GetAll", (await _appointmentService.GetAllAppointmentsAsync()).Result);
        }

        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> GetAllForDoctor(string buttonFilter, string doctorId, string sortOrder, string filter, string startDate, string endDate, int page = 1, int pageSize = 10)
        {
            IEnumerable<AppointmentResponse> appointments = (await _appointmentService.GetAllAppointmentsAsync()).Result as IEnumerable<AppointmentResponse>;
            if (TempData["create"] == null)
            {
                appointments = appointments.Where(a => a.StartTime.AddHours(-1) >= DateTime.Now).ToList();
            }
            var noFilterList = appointments;

            switch (buttonFilter)
            {
                case "clearFilter":
                    filter = "NoFilter";
                    break;
                default:
                    break;
            }

            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["PatientSortParm"] = sortOrder == "Patient" ? "patient_desc" : "Patient";
            ViewData["CreatedOnSortParm"] = sortOrder == "CreatedOn" ? "createdOn_desc" : "CreatedOn";
            ViewData["NoteSortParm"] = sortOrder == "Note" ? "note_desc" : "Note";
            ViewData["filter"] = filter;
            if (startDate == null && endDate == null)
            {
                startDate = appointments.Min(a => a.StartTime).ToString("yyyy-MM-dd");
                endDate = appointments.Max(a => a.StartTime).ToString("yyyy-MM-dd");
            }
            var all = new SelectListItem()
            {
                Value = "All",
                Text = "All"
            };
            var scheduled = new SelectListItem()
            {
                Value = "Scheduled",
                Text = "Scheduled"
            };
            var coffee = new SelectListItem()
            {
                Value = "Coffee",
                Text = "Coffee Break"
            };
            var free = new SelectListItem()
            {
                Value = "Free",
                Text = "Free"
            };

            var noFilter = new SelectListItem()
            {
                Value = "NoFilter",
                Text = "No filter"
            };
            List<SelectListItem> selectList = new List<SelectListItem>();
            selectList.Insert(0, all);
            selectList.Insert(1, scheduled);
            selectList.Insert(2, coffee);
            selectList.Insert(3, free);
            selectList.Insert(4, noFilter);
            ViewBag.Filters = selectList;



            appointments = appointments.Where(a => a.DoctorId == doctorId).ToList();
            PagedList<AppointmentResponse> model;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;
            ViewBag.Filter = filter;

            if (TempData["error"] != null)
            {
                ViewBag.Errors = TempData["error"].ToString();
            }

            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                if (TempData["create"] != null)
                {
                    var result = (await _appointmentService.GetAllAppointmentsForUserWithinGivenPeriodAsync(new AppointmentFilter() { StartDate = DateTime.Parse(startDate), EndDate = DateTime.Parse(endDate), UserId = null })).Result as IEnumerable<AppointmentResponse>;
                }
                else
                {
                    var result = await _appointmentService.GetAllAppointmentsForUserWithinGivenPeriodAsync(new AppointmentFilter() { StartDate = DateTime.Parse(startDate), EndDate = DateTime.Parse(endDate), UserId = null });
                    if (result.Result == null)
                    {
                        ViewBag.Errors = result.Errors.FirstOrDefault();
                    }
                    else
                    {
                        var temp = appointments;
                        appointments = (result.Result) as IEnumerable<AppointmentResponse>;
                        appointments = appointments.Where(a => a.StartTime.AddHours(-1) >= DateTime.Now);
                        if (!appointments.Any())
                        {
                            ViewBag.Errors = "Appointments in past cannot be scheduled.";
                            appointments = temp;
                        }
                    }
                }
            }
            if (appointments != null && appointments.Any())
            {
                ViewBag.DoctorName = appointments.First().Doctor.FirstName + " " + appointments.First().Doctor.LastName;
                ViewBag.DoctorId = appointments.First().DoctorId;

                if (TempData["create"] != null)
                {
                    DateTime? startTime = TempData["startTime"] as DateTime?;
                    DateTime? endTime = TempData["endTime"] as DateTime?;
                    appointments = appointments.Where(a => a.StartTime >= startTime.Value && a.StartTime <= endTime.Value);
                    TempData["create"] = true;
                    TempData["startTime"] = startTime;
                    TempData["endTime"] = endTime;
                }
                else
                {
                    appointments = appointments.Where(a => !a.Completed).ToList();
                }
                var apps = appointments;
                switch (filter)
                {
                    case "Scheduled":
                        appointments = appointments.Where(a => a.PatientId != null);
                        break;
                    case "Coffee":
                        appointments = appointments.Where(a => a.Note == "Coffee Break");
                        break;
                    case "Free":
                        appointments = appointments.Where(a => a.PatientId == null && !a.Completed);
                        break;
                    case "NoFilter":
                        appointments = noFilterList;
                        ViewData["searchString"] = null;
                        break;
                    default:
                        break;

                }
                if (!appointments.Any())
                {
                    ViewBag.Errors = "There is no appointments for given filter.";
                    appointments = apps;
                }

                switch (sortOrder)
                {
                    case "date_desc":
                        appointments = appointments.OrderByDescending(s => s.StartTime);
                        break;
                    case "patient_desc":
                        var sortDesc = appointments.Where(a => a.Patient != null).OrderByDescending(a => a.Patient.FirstName).ToList();
                        sortDesc.AddRange(appointments.Where(a => a.Patient == null).ToList());
                        appointments = sortDesc;
                        break;
                    case "Patient":
                        var sortAsc = appointments.Where(a => a.Patient != null).OrderBy(a => a.Patient.FirstName).ToList();
                        sortAsc.AddRange(appointments.Where(a => a.Patient == null));
                        appointments = sortAsc;
                        break;
                    case "createdOn_desc":
                        appointments = appointments.OrderByDescending(s => s.Date);
                        break;
                    case "CreatedOn":
                        appointments = appointments.OrderBy(s => s.Date);
                        break;
                    case "note_desc":
                        var sortNoteDesc = appointments.Where(a => a.Note != "").OrderByDescending(a => a.Note).ToList();
                        sortNoteDesc.AddRange(appointments.Where(a => a.Note == "" || a.Note == null).ToList());
                        appointments = sortNoteDesc;
                        break;
                    case "Note":
                        var sortNoteAsc = appointments.Where(a => a.Note != "").OrderBy(a => a.Note).ToList();
                        sortNoteAsc.AddRange(appointments.Where(a => a.Note == "" || a.Note == null));
                        appointments = sortNoteAsc;
                        break;
                    default:
                        appointments = appointments.OrderBy(s => s.StartTime);
                        break;
                }

                startDate = appointments.Min(a => a.StartTime).ToString("yyyy-MM-dd");
                endDate = appointments.Max(a => a.StartTime).ToString("yyyy-MM-dd");
                ViewData["startDate"] = startDate;
                ViewData["endDate"] = endDate;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;

                model = new PagedList<AppointmentResponse>(appointments.AsQueryable(), page, pageSize);
                return View(model);
            }
            var doctor = (await _doctorService.GetAsync(doctorId)).Result as DoctorResponse;
            ViewBag.DoctorName = doctor.FirstName + " " + doctor.LastName;
            ViewBag.DoctorId = doctor.Id;

            model = new PagedList<AppointmentResponse>(null, 1, 1);
            return View(model);
        }

        [Authorize(Roles = Constants.Technician)]
        public async Task<IActionResult> GetAllForPatient(string buttonFilter, string patientId, string searchString, string sortOrder, string filter, string startDate, string endDate, int page = 1, int pageSize = 10)
        {
            IEnumerable<AppointmentResponse> appointments = (await _appointmentService.GetAllAppointmentsAsync()).Result as IEnumerable<AppointmentResponse>;
            appointments = appointments.Where(a => a.StartTime.AddHours(-1) >= DateTime.Now);

            var noFilterList = appointments;

            switch (buttonFilter)
            {
                case "clearFilter":
                    filter = "NoFilter";
                    break;
                default:
                    break;
            }

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "doctor_name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["PatientSortParm"] = sortOrder == "Patient" ? "patient_desc" : "Patient";
            ViewData["CreatedOnSortParm"] = sortOrder == "CreatedOn" ? "createdOn_desc" : "CreatedOn";
            ViewData["NoteSortParm"] = sortOrder == "Note" ? "note_desc" : "Note";
            ViewData["filter"] = filter;
            ViewData["searchString"] = searchString;
            if (startDate == null && endDate == null && appointments.Any())
            {
                startDate = appointments.Min(a => a.StartTime).ToString("yyyy-MM-dd");
                endDate = appointments.Max(a => a.StartTime).ToString("yyyy-MM-dd");
            }
            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;

            var all = new SelectListItem()
            {
                Value = "All",
                Text = "All"
            };
            var scheduled = new SelectListItem()
            {
                Value = "Scheduled",
                Text = "Scheduled"
            };
            var coffee = new SelectListItem()
            {
                Value = "Coffee",
                Text = "Coffee Break"
            };
            var free = new SelectListItem()
            {
                Value = "Free",
                Text = "Free"
            };
            var noFilter = new SelectListItem()
            {
                Value = "NoFilter",
                Text = "No filter"
            };
            List<SelectListItem> selectList = new List<SelectListItem>();
            selectList.Insert(0, all);
            selectList.Insert(1, scheduled);
            selectList.Insert(2, coffee);
            selectList.Insert(3, free);
            selectList.Insert(4, noFilter);
            ViewBag.Filters = selectList;

            AppointmentSchedule appointment = new AppointmentSchedule()
            {
                PatientId = patientId
            };
            dynamic mymodel = new ExpandoObject();
            mymodel.AppSch = appointment;
            var patient = (await _patientService.GetAsync(patientId)).Result as PatientResponse;
            PagedList<AppointmentResponse> model;

            if (appointments != null)
            {
                if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
                {

                    var result = await _appointmentService.GetAllAppointmentsForUserWithinGivenPeriodAsync(new AppointmentFilter() { StartDate = DateTime.Parse(startDate), EndDate = DateTime.Parse(endDate), UserId = null });
                    if (result.Result == null)
                    {
                        ViewBag.Errors = result.Errors.FirstOrDefault();
                    }
                    else
                    {
                        var temp = appointments;
                        appointments = (result.Result) as IEnumerable<AppointmentResponse>;
                        appointments = appointments.Where(a => a.StartTime.AddHours(-1) >= DateTime.Now);
                        if (!appointments.Any())
                        {
                            ViewBag.Errors = "Appointments in past cannot be scheduled.";
                            appointments = temp;
                        }
                    }
                }

                if (!String.IsNullOrEmpty(searchString))
                {
                    var temp = appointments;
                    appointments = appointments.Where(s => s.Doctor.FirstName.ToLower().StartsWith(searchString.ToLower())
                                           || s.Doctor.LastName.ToLower().StartsWith(searchString.ToLower()));
                    if (!appointments.Any())
                    {
                        ViewBag.Errors = "There is no appointments for given filter.";
                        ViewBag.SearchString = null;
                        appointments = temp;
                    }
                }
                if (appointments.Any())
                {
                    ViewBag.PatientName = patient.FirstName + " " + patient.LastName;
                    ViewBag.PatientId = patient.Id;
                    var apps = appointments;
                    switch (filter)
                    {
                        case "Scheduled":
                            appointments = appointments.Where(a => a.PatientId != null);
                            break;
                        case "Coffee":
                            appointments = appointments.Where(a => a.Note == "Coffee Break");
                            break;
                        case "Free":
                            appointments = appointments.Where(a => a.PatientId == null && !a.Completed);
                            break;
                        case "NoFilter":
                            appointments = noFilterList;
                            ViewData["searchString"] = null;
                            break;
                        default:

                            break;
                    }

                    if (!appointments.Any())
                    {
                        ViewBag.Errors = "There is no appointments for given filter.";
                        appointments = apps;
                    }

                    switch (sortOrder)
                    {
                        case "Date":
                            appointments = appointments.OrderBy(s => s.StartTime);
                            break;
                        case "date_desc":
                            appointments = appointments.OrderByDescending(s => s.StartTime);
                            break;
                        case "doctor_name_desc":
                            appointments = appointments.OrderByDescending(s => s.Doctor.FirstName);
                            break;
                        case "patient_desc":
                            var sortDesc = appointments.Where(a => a.Patient != null).OrderByDescending(a => a.Patient.FirstName).ToList();
                            sortDesc.AddRange(appointments.Where(a => a.Patient == null).ToList());
                            appointments = sortDesc;
                            break;
                        case "Patient":
                            var sortAsc = appointments.Where(a => a.Patient != null).OrderBy(a => a.Patient.FirstName).ToList();
                            sortAsc.AddRange(appointments.Where(a => a.Patient == null));
                            appointments = sortAsc;
                            break;
                        case "createdOn_desc":
                            appointments = appointments.OrderByDescending(s => s.Date);
                            break;
                        case "CreatedOn":
                            appointments = appointments.OrderBy(s => s.Date);
                            break;
                        case "note_desc":
                            var sortNoteDesc = appointments.Where(a => a.Note != "").OrderByDescending(a => a.Note).ToList();
                            sortNoteDesc.AddRange(appointments.Where(a => a.Note == "" || a.Note == null).ToList());
                            appointments = sortNoteDesc;
                            break;
                        case "Note":
                            var sortNoteAsc = appointments.Where(a => a.Note != "").OrderBy(a => a.Note).ToList();
                            sortNoteAsc.AddRange(appointments.Where(a => a.Note == "" || a.Note == null));
                            appointments = sortNoteAsc;
                            break;
                        default:
                            appointments = appointments.OrderBy(s => s.Doctor.FirstName);
                            break;
                    }
                    startDate = appointments.Min(a => a.StartTime).ToString("yyyy-MM-dd");
                    endDate = appointments.Max(a => a.StartTime).ToString("yyyy-MM-dd");
                    ViewData["startDate"] = startDate;
                    ViewData["endDate"] = endDate;
                    ViewBag.StartDate = startDate;
                    ViewBag.EndDate = endDate;
                    model = new PagedList<AppointmentResponse>(appointments.AsQueryable(), page, pageSize);
                    mymodel.List = model;
                    return View(mymodel);
                }
            }
            ViewBag.PatientName = patient.FirstName + " " + patient.LastName;
            ViewBag.PatientId = patient.Id;
            mymodel.List = new PagedList<AppointmentResponse>(null, 1, 1);
            return View(mymodel);
        }

        [Authorize(Roles = Constants.Doctor)]
        [HttpPost]
        public async Task<IActionResult> Complete(AppointmentComplete appointment)
        {
            await _appointmentService.CompleteAppointmentAsync(appointment);
            return RedirectToAction("GetAll", (await _appointmentService.GetAllAppointmentsAsync()).Result);
        }

        [Authorize(Roles = Constants.Doctor)]
        [HttpGet]
        public async Task<IActionResult> Complete(int id)
        {
            AppointmentResponse app = (await _appointmentService.GetAppointmentAsync(id)).Result as AppointmentResponse;
            AppointmentComplete request = new AppointmentComplete();
            request.AppointmentId = id;
            request.Note = app.Note;
            _ = (await _patientService.GetAllAsync()).Result as List<PatientResponse>;
            ViewBag.Appointment = app;
            return View(request);
        }

        [HttpPost]
        public async Task<JsonResult> GetSearchValue(string search)
        {
            IEnumerable<PatientResponse> patients = ((await _patientService.GetAllAsync()).Result as IEnumerable<PatientResponse>);
            var response = (from p in patients
                            where p.FirstName.ToLower().Contains(search.ToLower()) || p.LastName.ToLower().Contains(search.ToLower())
                            select new
                            {
                                label = p.FirstName + " " + p.LastName,
                                val = p.Id
                            }).ToList();

            return Json(response);
        }

        [Authorize(Roles = Constants.Technician)]
        [HttpGet]
        public async Task<IActionResult> ScheduleFromPatients(int id, string patientId)
        {
            AppointmentResponse app = (await _appointmentService.GetAppointmentAsync(id)).Result as AppointmentResponse;
            AppointmentSchedule request = new AppointmentSchedule();
            TempData["schedule"] = true;
            if (TempData["error"] != null)
            {
                ViewBag.Error = TempData["error"];
            }
            request.AppointmentId = id;
            request.Note = app.Note;
            request.PatientId = patientId;
            ViewBag.Patient = ((await _patientService.GetAsync(patientId)).Result) as PatientResponse;


            ViewBag.Appointment = app;
            return View(request);
        }
    }
}
