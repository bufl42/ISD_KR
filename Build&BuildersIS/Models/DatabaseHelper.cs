using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Build_BuildersIS.Models;

namespace Build_BuildersIS.DataBase
{
    public class DatabaseHelper
    {

        // Выполнить SQL-запрос и вернуть результат в виде DataTable
        public static DataTable ExecuteQuery(string query, Dictionary<string, object> parameters = null)
        {
            DataTable result = new DataTable();

            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(result);
                    }
                }
            }

            return result;
        }

        public static object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    return command.ExecuteScalar();
                }
            }
        }

        // Выполнить SQL-запрос без возврата результата (INSERT, UPDATE, DELETE)
        public static int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }
    }
}
