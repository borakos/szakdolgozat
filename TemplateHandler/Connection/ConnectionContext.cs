using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using TemplateHandler.Models;

namespace TemplateHandler.Models {
    public class ConnectionContext {

        public static T parseEnum<T>(string value) {
            return (T)Enum.Parse(typeof(T), value, true);
        }

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
        public List<UserModel> getAllUsers() {
            List<UserModel> list = new List<UserModel>();
            MySqlConnection conn = GetConnection();
            string sql = "Select * from users";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                list.Add(new UserModel {
                    id = Convert.ToInt32(reader["id"]),
                    userName = reader["user_name"].ToString(),
                    nativeName = reader["native_name"].ToString(),
                    role = parseEnum<UserModel.Role>(reader["role"].ToString()),
                    password = reader["password"].ToString(),
                    email = reader["email"].ToString(),
                    salt = reader["salt"].ToString(),
                });
            }
            conn.Close();
            return list;
        }
        public UserModel getUserById(int id) {
            UserModel user= new UserModel();
            Boolean hasUser = false;
            MySqlConnection conn = GetConnection();
            string sql = "Select * from users where id='"+id+"'";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                hasUser = true;
                user.id = Convert.ToInt32(reader["id"]);
                user.userName = reader["user_name"].ToString();
                user.nativeName = reader["native_name"].ToString();
                user.role = parseEnum<UserModel.Role>(reader["role"].ToString());
                user.password = reader["password"].ToString();
                user.email = reader["email"].ToString();
                user.salt = reader["salt"].ToString();
            }
            conn.Close();
            if (hasUser) {
                return user;
            } else {
                return null;
            }
        }

        public UserModel getUserByUserName(string userName) {
            UserModel user = new UserModel();
            Boolean hasUser = false;
            MySqlConnection conn = GetConnection();
            string sql = "Select * from users where user_name='" + userName + "'";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                hasUser = true;
                user.id = Convert.ToInt32(reader["id"]);
                user.userName = reader["user_name"].ToString();
                user.nativeName = reader["native_name"].ToString();
                user.role = parseEnum<UserModel.Role>(reader["role"].ToString());
                user.password = reader["password"].ToString();
                user.email = reader["email"].ToString();
                user.salt = reader["salt"].ToString();
            }
            conn.Close();
            if (hasUser) {
                return user;
            } else {
                return null;
            }
        }

        public Boolean validateUser(string userName, string password) {
            Boolean valid = false;
            MySqlConnection conn = GetConnection();
            string sql = "Select * from users where user_name='" + userName + "' And password='"+password+"'";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                valid = true;
            }
            conn.Close();
            return valid;
        }
    }
}