using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RequestAndExceptionLogsCore3_1_WebAPI
{
    public sealed class DatabaseUtil
    {
        private string DatabaseConnection;

        public DatabaseUtil()
        {
            // Get default connection string from appsettings.json file.
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            DatabaseConnection = builder.Build().GetConnectionString("LogsConnection");
        }

        public bool SaveData(string query, IDictionary<string, object> parameters)
        {
            int isQueryExecuted = 0;

            using (var con = new SqlConnection(DatabaseConnection))
            {
                con.Open();
                using (var command = new SqlCommand(query, con))
                {
                    // Store query procedure or normal text.
                    command.CommandType = CommandType.Text;
                    // Query Parameters
                    if (parameters != null)
                    {
                        IDbDataParameter dbParam;
                        foreach (KeyValuePair<string, object> param in parameters)
                        {
                            dbParam = command.CreateParameter();
                            dbParam.ParameterName = param.Key;
                            dbParam.Value = param.Value ?? DBNull.Value;

                            command.Parameters.Add(dbParam);
                        }
                    }

                    // return int if inserted, updated or deleted.
                    isQueryExecuted = command.ExecuteNonQuery();

                }
                con.Close();

            }

            return isQueryExecuted > 0;

        }
    }
}