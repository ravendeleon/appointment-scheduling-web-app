using System;
using SchedulingApp.Access;

namespace SchedulingApp.Utilities
{
    public static class AppointmentAlerts
    {
        // returns an alert message if the user has an appointment in the next 15 minutes
        public static string GetUpcomingAlert(string username)
        {
            int userId = UserRepository.GetUserId(username);
            if (userId <= 0) return null;

            DateTime utcNow = DateTime.UtcNow;
            DateTime utcIn15 = utcNow.AddMinutes(15);

            bool found = AppointmentRepository.TryGetUpcomingAppointment(
                userId, utcNow, utcIn15,
                out int apptId, out DateTime startUtc);

            if (!found)
                return "No upcoming appointments within the next 15 minutes.";

            DateTime localStart = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc).ToLocalTime();
            return $"Upcoming appointment (ID: {apptId}) at {localStart:g}";
        }
    }
}