using Build_BuildersIS.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build_BuildersIS.Models
{
    public class Registration
    {

        public static bool IsUsernameUnique(string username)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE name = @Username";
            var parameters = new Dictionary<string, object>
            {
                { "@Username", username }
            };

            int userCount = (int)DatabaseHelper.ExecuteScalar(query, parameters);

            return userCount == 0;
        }

        public static bool IsEmailUnique(string email)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE email = @Email";
            var parameters = new Dictionary<string, object>
            {
                { "@Email", email }
            };

            int userCount = (int)DatabaseHelper.ExecuteScalar(query, parameters);

            return userCount == 0;
        }

        public static bool RegisterUser(string username, string email, string passwordHash)
        {
            string query = "INSERT INTO Users (name, passwordhash, role, email) VALUES (@Username, @PasswordHash, @UserRole, @Email)";
            var parameters = new Dictionary<string, object>
            {
                { "@Username", username },
                { "@PasswordHash", passwordHash },
                { "@UserRole", "WRK" },
                { "@Email", email }
            };

            try
            {
                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
