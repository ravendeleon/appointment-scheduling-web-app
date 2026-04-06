using MySql.Data.MySqlClient;
using System;

namespace SchedulingApp.Access
{
    public static class UserRepository
    {
        // checks if the username and password match a record in the database
        public static bool ValidateLogin(string username, string password)
        {
            const string sql =
                @"SELECT COUNT(*) FROM user
                  WHERE userName = @userName AND password = @password AND active = 1;";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userName", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // gets the user's ID number by their username
        public static int GetUserId(string username)
        {
            const string sql =
                @"SELECT userId FROM user WHERE userName = @userName LIMIT 1;";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userName", username);
                    var result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
        }

        // gets the user's role (manager or employee)
        public static string GetUserRole(string username)
        {
            const string sql =
                @"SELECT role FROM user WHERE userName = @userName LIMIT 1;";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userName", username);
                    var result = cmd.ExecuteScalar();
                    return result == null ? "employee" : result.ToString();
                }
            }
        }
    }
}