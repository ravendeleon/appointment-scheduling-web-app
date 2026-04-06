using MySql.Data.MySqlClient;
using SchedulingApp.Models;
using System;
using System.Collections.Generic;

namespace SchedulingApp.Access
{
    public static class CustomerRepository
    {
        // gets all customers with their address and city info
        public static List<Customer> GetAllCustomers()
        {
            const string sql =
                @"SELECT cu.customerId, cu.customerName, a.address, a.phone,
                    a.postalCode, ci.cityId, ci.city, co.country
                  FROM customer cu
                  JOIN address a ON cu.addressId = a.addressId
                  JOIN city ci ON a.cityId = ci.cityId
                  JOIN country co ON ci.countryId = co.countryId
                  ORDER BY cu.customerId;";

            var customers = new List<Customer>();

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(new Customer
                        {
                            CustomerId = reader.GetInt32("customerId"),
                            CustomerName = reader.GetString("customerName"),
                            Address = reader.GetString("address"),
                            Phone = reader.GetString("phone"),
                            PostalCode = reader.GetString("postalCode"),
                            CityId = reader.GetInt32("cityId"),
                            City = reader.GetString("city"),
                            Country = reader.GetString("country")
                        });
                    }
                }
            }
            return customers;
        }

        // gets a single customer by ID for the edit form
        public static Customer GetCustomerById(int customerId)
        {
            const string sql =
                @"SELECT cu.customerId, cu.customerName, a.address, a.phone,
                    a.postalCode, ci.cityId, ci.city, co.country
                  FROM customer cu
                  JOIN address a ON cu.addressId = a.addressId
                  JOIN city ci ON a.cityId = ci.cityId
                  JOIN country co ON ci.countryId = co.countryId
                  WHERE cu.customerId = @id LIMIT 1;";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", customerId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        return new Customer
                        {
                            CustomerId = reader.GetInt32("customerId"),
                            CustomerName = reader.GetString("customerName"),
                            Address = reader.GetString("address"),
                            Phone = reader.GetString("phone"),
                            PostalCode = reader.GetString("postalCode"),
                            CityId = reader.GetInt32("cityId"),
                            City = reader.GetString("city"),
                            Country = reader.GetString("country")
                        };
                    }
                }
            }
        }

        // gets a simplified list of customers for dropdown menus
        public static List<CustomerLookup> GetCustomerLookup()
        {
            const string sql =
                "SELECT customerId, customerName FROM customer ORDER BY customerName;";
            var list = new List<CustomerLookup>();

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new CustomerLookup
                        {
                            CustomerId = reader.GetInt32("customerId"),
                            CustomerName = reader.GetString("customerName")
                        });
                    }
                }
            }
            return list;
        }

        // adds a new customer using a transaction
        public static void AddCustomer(string customerName, string address,
            string phone, string postalCode, int cityId, string username)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        const string insertAddress =
                            @"INSERT INTO address
                              (address, address2, cityId, postalCode, phone,
                               createDate, createdBy, lastUpdate, lastUpdateBy)
                              VALUES (@address, '', @cityId, @postalCode, @phone,
                               NOW(), @user, NOW(), @user);";

                        int newAddressId;
                        using (var cmd = new MySqlCommand(insertAddress, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@address", address);
                            cmd.Parameters.AddWithValue("@cityId", cityId);
                            cmd.Parameters.AddWithValue("@postalCode", postalCode);
                            cmd.Parameters.AddWithValue("@phone", phone);
                            cmd.Parameters.AddWithValue("@user", username);
                            cmd.ExecuteNonQuery();
                            newAddressId = Convert.ToInt32(cmd.LastInsertedId);
                        }

                        const string insertCustomer =
                            @"INSERT INTO customer
                              (customerName, addressId, active, createDate,
                               createdBy, lastUpdate, lastUpdateBy)
                              VALUES (@name, @addressId, 1, NOW(), @user, NOW(), @user);";

                        using (var cmd = new MySqlCommand(insertCustomer, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@name", customerName);
                            cmd.Parameters.AddWithValue("@addressId", newAddressId);
                            cmd.Parameters.AddWithValue("@user", username);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // updates an existing customer and their address
        public static void UpdateCustomer(int customerId, string customerName,
            string address, string phone, string postalCode, int cityId, string username)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        const string updateAddress =
                            @"UPDATE address a
                              JOIN customer c ON a.addressId = c.addressId
                              SET a.address=@address, a.phone=@phone,
                                  a.postalCode=@postal, a.cityId=@cityId,
                                  a.lastUpdate=NOW(), a.lastUpdateBy=@user
                              WHERE c.customerId=@customerId;";

                        using (var cmd = new MySqlCommand(updateAddress, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@address", address);
                            cmd.Parameters.AddWithValue("@phone", phone);
                            cmd.Parameters.AddWithValue("@postal", postalCode);
                            cmd.Parameters.AddWithValue("@cityId", cityId);
                            cmd.Parameters.AddWithValue("@user", username);
                            cmd.Parameters.AddWithValue("@customerId", customerId);
                            cmd.ExecuteNonQuery();
                        }

                        const string updateCustomer =
                            @"UPDATE customer
                              SET customerName=@name, lastUpdate=NOW(), lastUpdateBy=@user
                              WHERE customerId=@customerId;";

                        using (var cmd = new MySqlCommand(updateCustomer, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@name", customerName);
                            cmd.Parameters.AddWithValue("@user", username);
                            cmd.Parameters.AddWithValue("@customerId", customerId);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // deletes a customer from the database
        public static void DeleteCustomer(int customerId)
        {
            const string sql = "DELETE FROM customer WHERE customerId = @id;";
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", customerId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // checks if a customer has appointments before allowing delete
        public static bool CustomerHasAppointments(int customerId)
        {
            const string sql =
                "SELECT COUNT(*) FROM appointment WHERE customerId = @id;";
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", customerId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }
    }
}