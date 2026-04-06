# 📅 Appointment Scheduling Web App

A full-stack web application for managing customers, appointments, and business scheduling — built with ASP.NET Core MVC and deployed live on Render.

🔗 **[View Live App](https://schedulingapp-f4sg.onrender.com)**  
> _Hosted on Render free tier — may take 30–60 seconds to spin up on first load._  
> **Test credentials:** `manager` / `test` or `employee` / `test`

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| Language | C# |
| Database | MySQL (Aiven managed cloud) |
| ORM | Dapper |
| Frontend | Razor Views, Bootstrap, JavaScript |
| Containerization | Docker |
| CI/CD | Docker Hub → Render.com |
| Testing | xUnit |

---

## ✨ Features

- **Role-based access control** — Manager and Employee roles with different permissions
- **Customer Management** — Full CRUD with real-time search
- **Appointment Scheduling** — Create and manage appointments with business hours validation (9 AM–5 PM) and overlap detection
- **Calendar View** — Monthly grid with date-click appointment display
- **Reports** — Three report types: Appointment Types by Month, Schedule by User, Appointments by Customer
- **Session-based authentication** — Secure login/logout flow
- **OOP Design** — Inheritance and polymorphism via `Person` base class with `Customer` and `Contact` subclasses

---

## 🏗️ Architecture

```
SchedulingApp/
├── Controllers/         # AccountController, CustomerController, AppointmentController, CalendarController, ReportController
├── Models/              # Person (base), Customer, Contact, Appointment, Report
├── Views/               # Razor views per controller
├── Utilities/           # Business logic helpers (time zone, business hours validation)
├── Properties/          # Launch settings
├── wwwroot/             # Static assets (CSS, JS)
├── Program.cs           # App entry point & DI configuration
├── appsettings.json     # App configuration (connection string placeholder)
├── Dockerfile           # Container definition
└── docker-compose.yml   # Local development setup
```

---

## 🚀 Running Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [MySQL](https://dev.mysql.com/downloads/) running locally
- [Docker](https://www.docker.com/products/docker-desktop/) (optional)

### Steps

1. **Clone the repo**
   ```bash
   git clone https://github.com/ravendeleon/appointment-scheduling-web-app.git
   cd appointment-scheduling-web-app
   ```

2. **Set up your database**  
   Import the schema into your local MySQL instance.

3. **Configure your connection string**  
   Create `appsettings.Development.json` in the project root (this file is gitignored):
   ```json
   {
     "ConnectionStrings": {
       "SchedulingApp": "Server=localhost;Database=client_schedule;Port=3306;User=root;Password=;AllowPublicKeyRetrieval=true;SslMode=Disabled;"
     }
   }
   ```

4. **Run the app**
   ```bash
   dotnet run
   ```

### Running with Docker
```bash
docker pull ravenpdeleon/schedulingapp:latest
docker run -p 8080:8080 ravenpdeleon/schedulingapp:latest
```

---

## 🧪 Tests

Unit tests are written with xUnit, covering customer and login validation logic.

```bash
cd SchedulingApp.Tests
dotnet test
```

---

## 📌 Notes

- Time zone handling converts all appointment times to UTC for storage and back to local (Central Time) for display
- Render free tier spins down after inactivity — first load may be slow
- The live app connects to a managed MySQL instance on Aiven

---

## 👩‍💻 Author

**Raven DeLeon**  
B.S. Software Engineering — Western Governors University  
[GitHub](https://github.com/ravendeleon) · [LinkedIn](https://www.linkedin.com/in/ravenpdeleon/)
