using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SchedulingApp.Models;

namespace SchedulingApp.Access
{
    public static class AppointmentRepository
    {
        // checks if there is an appointment starting within the next 15 minutes
        public static bool TryGetUpcomingAppointment(int userId, DateTime utcNow, DateTime utcIn15,
            out int appointmentId, out DateTime startUtc)
        {
            appointmentId = 0;
            startUtc = DateTime.MinValue;

            const string sql =
                @"SELECT appointmentId, start
                  FROM appointment
                  WHERE userId = @userId
                    AND start >= @utcNow
                    AND start <= @utcIn15
                  ORDER BY start
                  LIMIT 1;";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@utcNow", utcNow);
                    cmd.Parameters.AddWithValue("@utcIn15", utcIn15);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return false;
                        appointmentId = reader.GetInt32("appointmentId");
                        startUtc = reader.GetDateTime("start");
                        return true;
                    }
                }
            }
        }

        // gets all appointments and converts times from UTC to local
        public static List<Appointment> GetAllAppointments()
        {
            const string sql =
                @"SELECT a.appointmentId, a.customerId, c.customerName,
                    a.title, a.description, a.location, a.contact, a.type,
                    a.start, a.end, a.userId
                  FROM appointment a
                  JOIN customer c ON a.customerId = c.customerId
                  ORDER BY a.start;";

            var appts = new List<Appointment>();

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime startUtc = DateTime.SpecifyKind(reader.GetDateTime("start"), DateTimeKind.Utc);
                        DateTime endUtc = DateTime.SpecifyKind(reader.GetDateTime("end"), DateTimeKind.Utc);

                        appts.Add(new Appointment
                        {
                            AppointmentId = reader.GetInt32("appointmentId"),
                            CustomerId = reader.GetInt32("customerId"),
                            CustomerName = reader.GetString("customerName"),
                            Title = reader.GetString("title"),
                            Description = reader.GetString("description"),
                            Location = reader.GetString("location"),
                            Contact = reader.GetString("contact"),
                            Type = reader.GetString("type"),
                            StartLocal = startUtc.ToLocalTime(),
                            EndLocal = endUtc.ToLocalTime(),
                            UserId = reader.GetInt32("userId")
                        });
                    }
                }
            }

            return appts;
        }

        // reuses GetAllAppointments for the reports page
        public static List<Appointment> GetAllAppointmentsForReports()
        {
            return GetAllAppointments();
        }

        // gets a single appointment by its ID
        public static Appointment GetAppointmentById(int appointmentId)
        {
            const string sql =
                @"SELECT a.appointmentId, a.customerId, c.customerName,
                    a.title, a.description, a.location, a.contact, a.type,
                    a.start, a.end, a.userId
                  FROM appointment a
                  JOIN customer c ON a.customerId = c.customerId
                  WHERE a.appointmentId = @id LIMIT 1;";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", appointmentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;

                        DateTime startUtc = DateTime.SpecifyKind(reader.GetDateTime("start"), DateTimeKind.Utc);
                        DateTime endUtc = DateTime.SpecifyKind(reader.GetDateTime("end"), DateTimeKind.Utc);

                        return new Appointment
                        {
                            AppointmentId = reader.GetInt32("appointmentId"),
                            CustomerId = reader.GetInt32("customerId"),
                            CustomerName = reader.GetString("customerName"),
                            Title = reader.GetString("title"),
                            Description = reader.GetString("description"),
                            Location = reader.GetString("location"),
                            Contact = reader.GetString("contact"),
                            Type = reader.GetString("type"),
                            StartLocal = startUtc.ToLocalTime(),
                            EndLocal = endUtc.ToLocalTime(),
                            UserId = reader.GetInt32("userId")
                        };
                    }
                }
            }
        }

        // checks if the user already has an appointment at the same time
        public static bool HasOverlappingAppointment(int userId, DateTime newStartUtc,
            DateTime newEndUtc, int? ignoreId)
        {
            string sql =
                @"SELECT COUNT(*) FROM appointment
                  WHERE userId = @userId AND start < @newEnd AND end > @newStart";

            if (ignoreId.HasValue) sql += " AND appointmentId <> @ignoreId;";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@newStart", newStartUtc);
                    cmd.Parameters.AddWithValue("@newEnd", newEndUtc);
                    if (ignoreId.HasValue)
                        cmd.Parameters.AddWithValue("@ignoreId", ignoreId.Value);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // adds a new appointment to the database
        public static void AddAppointment(int customerId, int userId, string title,
            string description, string location, string contact, string type,
            DateTime startUtc, DateTime endUtc, string username)
        {
            const string sql =
                @"INSERT INTO appointment
                (customerId, userId, title, description, location, contact, type, url,
                 start, end, createDate, createdBy, lastUpdate, lastUpdateBy)
                VALUES
                (@customerId, @userId, @title, @description, @location, @contact, @type, '',
                 @start, @end, NOW(), @user, NOW(), @user);";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@customerId", customerId);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@location", location);
                    cmd.Parameters.AddWithValue("@contact", contact);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@start", startUtc);
                    cmd.Parameters.AddWithValue("@end", endUtc);
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // updates an existing appointment in the database
        public static void UpdateAppointment(int appointmentId, int customerId, int userId,
            string title, string description, string location, string contact, string type,
            DateTime startUtc, DateTime endUtc, string username)
        {
            const string sql =
                @"UPDATE appointment
                  SET customerId=@customerId, userId=@userId, title=@title,
                      description=@description, location=@location, contact=@contact,
                      type=@type, start=@start, end=@end,
                      lastUpdate=NOW(), lastUpdateBy=@user
                  WHERE appointmentId=@id;";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", appointmentId);
                    cmd.Parameters.AddWithValue("@customerId", customerId);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@location", location);
                    cmd.Parameters.AddWithValue("@contact", contact);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@start", startUtc);
                    cmd.Parameters.AddWithValue("@end", endUtc);
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // deletes an appointment from the database
        public static void DeleteAppointment(int appointmentId)
        {
            const string sql = "DELETE FROM appointment WHERE appointmentId = @id;";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", appointmentId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // gets appointments along with the username of who created them
        public static List<(string UserName, Appointment Appt)> GetAppointmentsWithUserNames()
        {
            const string sql =
                @"SELECT u.userName, a.appointmentId, a.customerId, c.customerName,
                    a.type, a.title, a.start, a.end, a.userId
                  FROM appointment a
                  JOIN customer c ON a.customerId = c.customerId
                  JOIN user u ON a.userId = u.userId
                  ORDER BY u.userName, a.start;";

            var list = new List<(string, Appointment)>();

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime startUtc = DateTime.SpecifyKind(
                            reader.GetDateTime("start"), DateTimeKind.Utc);
                        DateTime endUtc = DateTime.SpecifyKind(
                            reader.GetDateTime("end"), DateTimeKind.Utc);

                        var appt = new Appointment
                        {
                            AppointmentId = reader.GetInt32("appointmentId"),
                            CustomerId = reader.GetInt32("customerId"),
                            CustomerName = reader.GetString("customerName"),
                            Type = reader.GetString("type"),
                            Title = reader.GetString("title"),
                            StartLocal = startUtc.ToLocalTime(),
                            EndLocal = endUtc.ToLocalTime(),
                            UserId = reader.GetInt32("userId")
                        };
                        list.Add((reader.GetString("userName"), appt));
                    }
                }
            }
            return list;
        }

        // gets all appointments for a specific date for the calendar module
        public static List<Appointment> GetAppointmentsForDate(DateTime localDate)
        {
            DateTime localStart = localDate.Date;
            DateTime localEnd = localStart.AddDays(1);

            DateTime utcStart = DateTime.SpecifyKind(localStart, DateTimeKind.Local).ToUniversalTime();
            DateTime utcEnd = DateTime.SpecifyKind(localEnd, DateTimeKind.Local).ToUniversalTime();

            const string sql =
                @"SELECT a.appointmentId, a.customerId, c.customerName,
                    a.title, a.description, a.location, a.contact, a.type,
                    a.start, a.end, a.userId
                  FROM appointment a
                  JOIN customer c ON a.customerId = c.customerId
                  WHERE a.start >= @utcStart AND a.start < @utcEnd
                  ORDER BY a.start;";

            var appts = new List<Appointment>();

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@utcStart", utcStart);
                    cmd.Parameters.AddWithValue("@utcEnd", utcEnd);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime startUtc = DateTime.SpecifyKind(
                                reader.GetDateTime("start"), DateTimeKind.Utc);
                            DateTime endUtc = DateTime.SpecifyKind(
                                reader.GetDateTime("end"), DateTimeKind.Utc);

                            appts.Add(new Appointment
                            {
                                AppointmentId = reader.GetInt32("appointmentId"),
                                CustomerId = reader.GetInt32("customerId"),
                                CustomerName = reader.GetString("customerName"),
                                Title = reader.GetString("title"),
                                Description = reader.GetString("description"),
                                Location = reader.GetString("location"),
                                Contact = reader.GetString("contact"),
                                Type = reader.GetString("type"),
                                StartLocal = startUtc.ToLocalTime(),
                                EndLocal = endUtc.ToLocalTime(),
                                UserId = reader.GetInt32("userId")
                            });
                        }
                    }
                }
            }
            return appts;
        }
    }
}