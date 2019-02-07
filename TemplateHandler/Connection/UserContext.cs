using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TemplateHandler.Models;

namespace TemplateHandler.Connection {
    public class UserContext {
        private string connectionString { get; set; }

        public UserContext(string connectionString) {
            this.connectionString = connectionString;
        }

        private MySqlConnection getConnection() {
            return new MySqlConnection(connectionString);
        }

        public List<UserModel> getAllUsers() {
            List<UserModel> list = new List<UserModel>();
            MySqlConnection conn = getConnection();
            string sql = "Select * from `users`";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                list.Add(new UserModel {
                    id = Convert.ToInt32(reader["id"]),
                    userName = reader["user_name"].ToString(),
                    nativeName = reader["native_name"].ToString(),
                    role = ConnectionContext.parseEnum<UserModel.Role>(reader["role"].ToString()),
                    password = reader["password"].ToString(),
                    email = reader["email"].ToString(),
                });
                Debug.WriteLine("\n\nRole:"+list[list.Count-1].role);
            }
            conn.Close();
            return list;
        }
        public UserModel getUserById(int id) {
            UserModel user = new UserModel();
            Boolean hasUser = false;
            MySqlConnection conn = getConnection();
            string sql = "Select * from `users` where `id`='" + id + "'";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                hasUser = true;
                user.id = Convert.ToInt32(reader["id"]);
                user.userName = reader["user_name"].ToString();
                user.nativeName = reader["native_name"].ToString();
                user.role = ConnectionContext.parseEnum<UserModel.Role>(reader["role"].ToString());
                user.password = reader["password"].ToString();
                user.email = reader["email"].ToString();
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
            MySqlConnection conn = getConnection();
            string sql = "Select * from `users` where `user_name`='" + userName + "'";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                hasUser = true;
                user.id = Convert.ToInt32(reader["id"]);
                user.userName = reader["user_name"].ToString();
                user.nativeName = reader["native_name"].ToString();
                user.role = ConnectionContext.parseEnum<UserModel.Role>(reader["role"].ToString());
                user.password = reader["password"].ToString();
                user.email = reader["email"].ToString();
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
            MySqlConnection conn = getConnection();
            string sql = "Select * from `users` where `user_name`='" + userName + "' And `password`='" + password + "'";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                valid = true;
            }
            conn.Close();
            return valid;
        }

        public void updateUserByUser(UserModel newUser,int id) {
            UserModel user = getUserById(id);
            if (user!= null) {
                MySqlConnection conn = getConnection();
                string sql = "Update `users` Set `user_name`="+newUser.userName+"', `email`='"+newUser.email+"', `native_name`='"+newUser.nativeName+"', `password`='"+newUser.password+"' Where `id`='"+id+"'";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public void createUser(UserModel user) {
            MySqlConnection conn = getConnection();
            string sql = "Insert into `users`(`user_name`,`role`,`email`,`native_name`,`password`) Values(@user_name,@role,@email,@native_name,@password)";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@user_name", user.userName);
            cmd.Parameters.AddWithValue("@role", user.role.ToString());
            cmd.Parameters.AddWithValue("@email", user.email);
            cmd.Parameters.AddWithValue("@native_name", user.nativeName);
            cmd.Parameters.AddWithValue("@password", user.password);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void deleteUser(int id) {
            MySqlConnection conn = getConnection();
            string sql = "Delete from `users` Where `id`='@id'";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
