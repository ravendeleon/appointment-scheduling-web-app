using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchedulingApp.Access;
using SchedulingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchedulingApp.Controllers
{
    public class ReportController : Controller
    {
        // helper method to check if the user is logged in
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("Username") != null;
        }

        // shows the reports page
        public IActionResult Index(string reportType, string search)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            // only managers can access reports
            if (HttpContext.Session.GetString("Role") != "manager")
            {
                TempData["Error"] = "Access denied. Reports are available to managers only.";
                return RedirectToAction("Index", "Customer");
            }

            ViewBag.ReportType = reportType;
            ViewBag.Search = search;
            ViewBag.ReportTime = DateTime.Now.ToString("M/d/yyyy h:mm tt");

            if (reportType == "TypesByMonth")
            {
                var appts = AppointmentRepository.GetAllAppointmentsForReports();

                var report = appts
                    .GroupBy(a => new { Month = a.StartLocal.ToString("yyyy-MM"), a.Type })
                    .Select(g => new TypeByMonthRow
                    {
                        Month = g.Key.Month,
                        Type = g.Key.Type,
                        Count = g.Count()
                    })
                    .OrderBy(r => r.Month)
                    .ThenBy(r => r.Type)
                    .ToList();

                ViewBag.ReportTitle = "Appointment Types by Month";
                ViewBag.TypesByMonth = report;
            }
            else if (reportType == "ScheduleByUser")
            {
                var data = AppointmentRepository.GetAppointmentsWithUserNames();

                var report = data
                    .Select(x => new UserScheduleRow
                    {
                        UserName = x.UserName,
                        AppointmentId = x.Appt.AppointmentId,
                        CustomerName = x.Appt.CustomerName,
                        Type = x.Appt.Type,
                        StartLocal = x.Appt.StartLocal,
                        EndLocal = x.Appt.EndLocal
                    })
                    .OrderBy(r => r.UserName)
                    .ThenBy(r => r.StartLocal)
                    .ToList();

                ViewBag.ReportTitle = "Schedule by User";
                ViewBag.ScheduleByUser = report;
            }
            else if (reportType == "ByCustomer")
            {
                var appts = AppointmentRepository.GetAllAppointmentsForReports();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string q = search.Trim().ToLower();
                    appts = appts.Where(a =>
                        a.CustomerName.ToLower().Contains(q)).ToList();
                }

                var report = appts
                    .GroupBy(a => a.CustomerName)
                    .Select(g => new CustomerAppointmentCountRow
                    {
                        CustomerName = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(r => r.Count)
                    .ThenBy(r => r.CustomerName)
                    .ToList();

                ViewBag.ReportTitle = "Appointments by Customer";
                ViewBag.ByCustomer = report;
            }

            return View();
        }
    }
}