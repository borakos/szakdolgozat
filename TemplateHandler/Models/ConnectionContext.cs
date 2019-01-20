using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace TemplateHandler.Models {
    public class ConnectionContext {
        public string connectionString { get; set; }

        public ConnectionContext(string connectionString) {
            this.connectionString = connectionString;
        }

        private MySqlConnection GetConnection() {
            return new MySqlConnection(connectionString);
        }

        public List<TemplateFile> getTemplateFiles() {
            List<TemplateFile> files = new List<TemplateFile>();
            MySqlConnection conn = GetConnection();
            Console.Out.WriteLine("Connection string:"+conn.ConnectionString);
            string sql = "Select * from files";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                files.Add(new TemplateFile {
                    id = Convert.ToInt32(reader["id"]),
                    name = reader["name"].ToString(),
                    path = reader["path"].ToString()
                });
            }
            conn.Close();
            return files;
        }
    }
}