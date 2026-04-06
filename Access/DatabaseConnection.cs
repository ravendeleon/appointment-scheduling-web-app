using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace SchedulingApp.Access
{
    public static class DatabaseConnection
    {
        // stores the connection string from appsettings.json
        private static string _connectionString;

        // called once at startup to read the connection string
        public static void Initialize(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SchedulingApp");
        }

        // creates a new connection each time it's called
        // this is better for web apps than sharing one connection
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}