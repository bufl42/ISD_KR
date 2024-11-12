
using Build_BuildersIS.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Build_BuildersIS.Entarence
{
    public class Authentication
    {

        public static bool AuthenticateUserByName(string username, string passwordHash, out string userRole)
        {
            userRole = string.Empty;

            if(EmailChecker.IsValidEmail(username))
            {
                string query = "SELECT role FROM Users WHERE email = @Email AND passwordhash = @PasswordHash";

                using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", username);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    connection.Open();
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        userRole = result.ToString();
                        return true;
                    }
                    return false;
                }
            }

            else
            {
                string query = "SELECT role FROM Users WHERE name = @Username AND passwordhash = @PasswordHash";

                using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    connection.Open();
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        userRole = result.ToString();
                        return true;
                    }
                    return false;
                }
            }
        }
    }
}
