using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchedulingApp.Access;
using System;

namespace SchedulingApp.Controllers
{
    public class CalendarController : Controller
    {
        // helper method to check if the user is logged in
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("Username") != null;
        }

        // shows the calendar page
        public IActionResult Index(string selectedDate)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            // default to today if no date was selected
            DateTime date = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(selectedDate) &&
                DateTime.TryParse(selectedDate, out DateTime parsed))
            {
                date = parsed;
            }

            DateTime firstOfMonth = new DateTime(date.Year, date.Month, 1);
            DateTime lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

            var appointments = AppointmentRepository.GetAppointmentsForDate(date);

            ViewBag.SelectedDate = date;
            ViewBag.FirstOfMonth = firstOfMonth;
            ViewBag.LastOfMonth = lastOfMonth;
            ViewBag.CurrentMonth = date.ToString("MMMM yyyy");

            return View(appointments);
        }
    }
}