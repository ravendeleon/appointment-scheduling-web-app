using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchedulingApp.Access;
using SchedulingApp.Utilities;

namespace SchedulingApp.Controllers
{
    public class AccountController : Controller
    {
        // shows the login page
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToAction("Index", "Customer");

            return View();
        }

        // handles the login form submission
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            bool valid = UserRepository.ValidateLogin(username, password);
            LoginHistory.Append(username);

            if (!valid)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            // save the username and role in session
            HttpContext.Session.SetString("Username", username);
            HttpContext.Session.SetString("Role", UserRepository.GetUserRole(username));

            string alert = AppointmentAlerts.GetUpcomingAlert(username);
            if (alert != null)
                TempData["Alert"] = alert;

            return RedirectToAction("Index", "Customer");
        }

        // logs the user out and clears the session
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}