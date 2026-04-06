using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchedulingApp.Access;
using SchedulingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchedulingApp.Controllers
{
    public class AppointmentController : Controller
    {
        // helper method to check if the user is logged in
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("Username") != null;
        }

        // shows the appointment list with optional search
        public IActionResult Index(string search)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var appointments = AppointmentRepository.GetAllAppointments();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string q = search.Trim().ToLower();
                appointments = appointments.Where(a =>
                    a.CustomerName.ToLower().Contains(q) ||
                    a.Type.ToLower().Contains(q) ||
                    a.Title.ToLower().Contains(q) ||
                    a.Location.ToLower().Contains(q)
                ).ToList();
            }

            ViewBag.Search = search;
            return View(appointments);
        }

        // shows the add appointment form
        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            LoadCustomerDropdown();
            LoadTypeDropdown();
            return View();
        }

        // handles the add appointment form submission
        [HttpPost]
        [ActionName("Create")]
        public IActionResult CreatePost(
            int customerId,
            string title,
            string description,
            string location,
            string contact,
            string appointmentType,
            string startDate,
            string startTime,
            string endTime)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            string username = HttpContext.Session.GetString("Username");
            int userId = UserRepository.GetUserId(username);

            if (customerId == 0 ||
                string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(location) ||
                string.IsNullOrWhiteSpace(contact) ||
                string.IsNullOrWhiteSpace(appointmentType) ||
                string.IsNullOrWhiteSpace(startDate) ||
                string.IsNullOrWhiteSpace(startTime) ||
                string.IsNullOrWhiteSpace(endTime))
            {
                ViewBag.Error = "All fields are required.";
                LoadCustomerDropdown(customerId);
                LoadTypeDropdown(appointmentType);
                ViewBag.Title = title;
                ViewBag.Description = description;
                ViewBag.Location = location;
                ViewBag.Contact = contact;
                ViewBag.StartDate = startDate;
                ViewBag.StartTime = startTime;
                ViewBag.EndTime = endTime;
                return View("Create");
            }

            if (!TryParseAppointmentTimes(startDate, startTime, endTime,
                out DateTime startLocal, out DateTime endLocal, out string timeError))
            {
                ViewBag.Error = timeError;
                LoadCustomerDropdown(customerId);
                LoadTypeDropdown(appointmentType);
                return View("Create");
            }

            if (endLocal <= startLocal)
            {
                ViewBag.Error = "End time must be after start time.";
                LoadCustomerDropdown(customerId);
                LoadTypeDropdown(appointmentType);
                return View("Create");
            }

            // check business hours using local time before converting to UTC
            if (!IsWithinBusinessHours(startLocal, endLocal))
            {
                ViewBag.Error = "Appointments must be Mon-Fri, 9:00 AM to 5:00 PM.";
                LoadCustomerDropdown(customerId);
                LoadTypeDropdown(appointmentType);
                return View("Create");
            }

            DateTime startUtc = DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime();
            DateTime endUtc = DateTime.SpecifyKind(endLocal, DateTimeKind.Local).ToUniversalTime();

            if (AppointmentRepository.HasOverlappingAppointment(userId, startUtc, endUtc, null))
            {
                ViewBag.Error = "This appointment overlaps with an existing appointment.";
                LoadCustomerDropdown(customerId);
                LoadTypeDropdown(appointmentType);
                return View("Create");
            }

            AppointmentRepository.AddAppointment(
                customerId, userId, title, description,
                location, contact, appointmentType,
                startUtc, endUtc, username);

            TempData["Success"] = "Appointment added successfully.";
            return RedirectToAction("Index");
        }

        // shows the edit appointment form
        public IActionResult Edit(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var appt = AppointmentRepository.GetAppointmentById(id);
            if (appt == null) return NotFound();

            ViewBag.StartDate = appt.StartLocal.ToString("yyyy-MM-dd");
            ViewBag.StartTime = appt.StartLocal.ToString("HH:mm");
            ViewBag.EndTime = appt.EndLocal.ToString("HH:mm");
            ViewBag.AppointmentId = appt.AppointmentId;

            LoadCustomerDropdown(appt.CustomerId);
            LoadTypeDropdown(appt.Type);
            return View(appt);
        }

        // handles the edit appointment form submission
        [HttpPost]
        public IActionResult Edit(
            int appointmentId,
            int customerId,
            string title,
            string description,
            string location,
            string contact,
            string appointmentType,
            string startDate,
            string startTime,
            string endTime)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            string username = HttpContext.Session.GetString("Username");
            int userId = UserRepository.GetUserId(username);

            if (customerId == 0 ||
                string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(location) ||
                string.IsNullOrWhiteSpace(contact) ||
                string.IsNullOrWhiteSpace(appointmentType) ||
                string.IsNullOrWhiteSpace(startDate) ||
                string.IsNullOrWhiteSpace(startTime) ||
                string.IsNullOrWhiteSpace(endTime))
            {
                ViewBag.Error = "All fields are required.";
                ViewBag.AppointmentId = appointmentId;
                ViewBag.StartDate = startDate;
                ViewBag.StartTime = startTime;
                ViewBag.EndTime = endTime;
                LoadCustomerDropdown(customerId);
                LoadTypeDropdown(appointmentType);
                return View();
            }

            if (!TryParseAppointmentTimes(startDate, startTime, endTime,
                out DateTime startLocal, out DateTime endLocal, out string timeError))
            {
                ViewBag.Error = timeError;
                ViewBag.AppointmentId = appointmentId;
                LoadCustomerDropdown(customerId);
                LoadTypeDropdown(appointmentType);
                return View();
            }

            if (endLocal <= startLocal)
            {
                ViewBag.Error = "End time must be after start time.";
                ViewBag.AppointmentId = appointmentId;
                LoadCustomerDropdown(customerId);
                LoadTypeDropdown(appointmentType);
                return View();
            }

            // check business hours using local time before converting to UTC
            if (!IsWithinBusinessHours(startLocal, endLocal))
            {
                ViewBag.Error = "Appointments must be Mon-Fri, 9:00 AM to 5:00 PM.";
                ViewBag.AppointmentId = appointmentId;
                LoadCustomerDropdown(customerId);
                LoadTypeDropdown(appointmentType);
                return View();
            }

            DateTime startUtc = DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime();
            DateTime endUtc = DateTime.SpecifyKind(endLocal, DateTimeKind.Local).ToUniversalTime();

            if (AppointmentRepository.HasOverlappingAppointment(
                userId, startUtc, endUtc, appointmentId))
            {
                ViewBag.Error = "This appointment overlaps with an existing appointment.";
                ViewBag.AppointmentId = appointmentId;
                LoadCustomerDropdown(customerId);
                LoadTypeDropdown(appointmentType);
                return View();
            }

            AppointmentRepository.UpdateAppointment(
                appointmentId, customerId, userId,
                title, description, location, contact,
                appointmentType, startUtc, endUtc, username);

            TempData["Success"] = "Appointment updated successfully.";
            return RedirectToAction("Index");
        }

        // shows the delete confirmation page
        public IActionResult Delete(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var appt = AppointmentRepository.GetAppointmentById(id);
            if (appt == null) return NotFound();
            return View(appt);
        }

        // handles the confirmed delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            AppointmentRepository.DeleteAppointment(id);
            TempData["Success"] = "Appointment deleted successfully.";
            return RedirectToAction("Index");
        }

        // loads the customer dropdown from the database
        private void LoadCustomerDropdown(int selectedId = 0)
        {
            var customers = CustomerRepository.GetCustomerLookup();
            ViewBag.Customers = new SelectList(customers, "CustomerId", "CustomerName", selectedId);
        }

        // loads the appointment type dropdown
        private void LoadTypeDropdown(string selected = "")
        {
            var types = new List<string>
            {
                "Consultation", "Planning", "Review", "Follow-up", "Other"
            };
            ViewBag.Types = new SelectList(types, selected);
        }

        // parses date and time strings into DateTime objects
        private bool TryParseAppointmentTimes(string date, string start, string end,
            out DateTime startLocal, out DateTime endLocal, out string error)
        {
            startLocal = DateTime.MinValue;
            endLocal = DateTime.MinValue;
            error = null;

            if (!DateTime.TryParse(date + " " + start, out startLocal))
            {
                error = "Invalid start date or time.";
                return false;
            }

            if (!DateTime.TryParse(date + " " + end, out endLocal))
            {
                error = "Invalid end time.";
                return false;
            }

            return true;
        }

        // checks appointment falls within business hours Mon-Fri 9am-5pm
        private bool IsWithinBusinessHours(DateTime startLocal, DateTime endLocal)
        {
            if (startLocal.DayOfWeek == DayOfWeek.Saturday ||
                startLocal.DayOfWeek == DayOfWeek.Sunday)
                return false;

            if (endLocal.Date != startLocal.Date)
                return false;

            TimeSpan open = new TimeSpan(9, 0, 0);
            TimeSpan close = new TimeSpan(17, 0, 0);

            return startLocal.TimeOfDay >= open && endLocal.TimeOfDay <= close;
        }
    }
}