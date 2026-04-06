using MySql.Data.MySqlClient;
using SchedulingApp.Models;
using System.Collections.Generic;

namespace SchedulingApp.Access
{
    public static class CityRepository
    {
        // gets all cities with their country for the dropdown menu
        // populating dropdowns from the database makes the app scalable
        public static List<CityInfo> GetAllCities()
        {
            const string sql =
                @"SELECT ci.cityId, ci.city, co.country
                  FROM city ci
                  JOIN country co ON ci.countryId = co.countryId
                  ORDER BY co.country, ci.city;";

            var cities = new List<CityInfo>();

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cities.Add(new CityInfo
                        {
                            CityId = reader.GetInt32("cityId"),
                            DisplayName = reader.GetString("city") +
                                " (" + reader.GetString("country") + ")"
                        });
                    }
                }
            }
            return cities;
        }
    }
}