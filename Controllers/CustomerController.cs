using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchedulingApp.Access;
using SchedulingApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace SchedulingApp.Controllers
{
    public class CustomerController : Controller
    {
        // helper method to check if the user is logged in
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("Username") != null;
        }

        // shows the customer list
        public IActionResult Index(string search)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var customers = CustomerRepository.GetAllCustomers();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string q = search.Trim().ToLower();
                customers = customers.Where(c =>
                    c.CustomerName.ToLower().Contains(q) ||
                    c.City.ToLower().Contains(q) ||
                    c.Phone.ToLower().Contains(q) ||
                    c.Country.ToLower().Contains(q)
                ).ToList();
            }

            ViewBag.Search = search;
            return View(customers);
        }

        // shows the add customer form
        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            LoadCityDropdown();
            return View();
        }

        // handles the add customer form submission
        [HttpPost]
        public IActionResult Create(Customer customer)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(customer.CustomerName) ||
                string.IsNullOrWhiteSpace(customer.Address) ||
                string.IsNullOrWhiteSpace(customer.Phone) ||
                string.IsNullOrWhiteSpace(customer.PostalCode) ||
                customer.CityId == 0)
            {
                ViewBag.Error = "All fields are required.";
                LoadCityDropdown();
                return View(customer);
            }

            bool hasDigit = false;
            foreach (char c in customer.Phone)
            {
                if (!char.IsDigit(c) && c != '-')
                {
                    ViewBag.Error = "Phone number must contain only digits and dashes.";
                    LoadCityDropdown();
                    return View(customer);
                }
                if (char.IsDigit(c)) hasDigit = true;
            }

            if (!hasDigit)
            {
                ViewBag.Error = "Phone number must contain at least one digit.";
                LoadCityDropdown();
                return View(customer);
            }

            string username = HttpContext.Session.GetString("Username");
            CustomerRepository.AddCustomer(
                customer.CustomerName, customer.Address,
                customer.Phone, customer.PostalCode,
                customer.CityId, username);

            TempData["Success"] = "Customer added successfully.";
            return RedirectToAction("Index");
        }

        // shows the edit customer form
        public IActionResult Edit(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var customer = CustomerRepository.GetCustomerById(id);
            if (customer == null) return NotFound();
            LoadCityDropdown(customer.CityId);
            return View(customer);
        }

        // handles the edit customer form submission
        [HttpPost]
        public IActionResult Edit(Customer customer)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(customer.CustomerName) ||
                string.IsNullOrWhiteSpace(customer.Address) ||
                string.IsNullOrWhiteSpace(customer.Phone) ||
                string.IsNullOrWhiteSpace(customer.PostalCode) ||
                customer.CityId == 0)
            {
                ViewBag.Error = "All fields are required.";
                LoadCityDropdown(customer.CityId);
                return View(customer);
            }

            bool hasDigit = false;
            foreach (char c in customer.Phone)
            {
                if (!char.IsDigit(c) && c != '-')
                {
                    ViewBag.Error = "Phone number must contain only digits and dashes.";
                    LoadCityDropdown(customer.CityId);
                    return View(customer);
                }
                if (char.IsDigit(c)) hasDigit = true;
            }

            if (!hasDigit)
            {
                ViewBag.Error = "Phone number must contain at least one digit.";
                LoadCityDropdown(customer.CityId);
                return View(customer);
            }

            string username = HttpContext.Session.GetString("Username");
            CustomerRepository.UpdateCustomer(
                customer.CustomerId, customer.CustomerName,
                customer.Address, customer.Phone,
                customer.PostalCode, customer.CityId, username);

            TempData["Success"] = "Customer updated successfully.";
            return RedirectToAction("Index");
        }

        // shows the delete confirmation page
        public IActionResult Delete(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var customer = CustomerRepository.GetCustomerById(id);
            if (customer == null) return NotFound();

            if (CustomerRepository.CustomerHasAppointments(id))
            {
                TempData["Error"] = "This customer has appointments and cannot be deleted.";
                return RedirectToAction("Index");
            }

            return View(customer);
        }

        // handles the confirmed delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            CustomerRepository.DeleteCustomer(id);
            TempData["Success"] = "Customer deleted successfully.";
            return RedirectToAction("Index");
        }

        // loads the city dropdown from the database
        // populating from DB makes the app scalable
        private void LoadCityDropdown(int selectedCityId = 0)
        {
            var cities = CityRepository.GetAllCities();
            ViewBag.Cities = new SelectList(cities, "CityId", "DisplayName", selectedCityId);
        }
    }
}