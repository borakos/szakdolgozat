using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TemplateHandler.Models;

namespace TemplateHandler.Connection {
    public class TemplateFileContext {
        private string connectionString { get; set; }

        public TemplateFileContext(string connectionString) {
            this.connectionString = connectionString;
        }

        private MySqlConnection getConnection() {
            return new MySqlConnection(connectionString);
        }

        public List<TemplateFile> getAllTemplate() {
            List<TemplateFile> list = new List<TemplateFile>();
            MySqlConnection conn = getConnection();
            string sql = "Select tf.*, tg.name group_name, u.user_name owner_name from `template_files` tf Left Join `users` u on tf.owner_id=u.id LEFT JOIN template_groups tg on tf.group_id = tg.id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                Debug.Write(Convert.ToInt32(reader["id"]));
                Debug.Write(", "+reader["name"].ToString());
                Debug.Write(", " + reader["path"].ToString());
                Debug.Write(", " + reader["local_name"].ToString());
                Debug.Write(", " + ConnectionContext.parseEnum<TemplateFile.Type>(reader["type"].ToString()).ToString());
                if (reader["owner_id"] == DBNull.Value) {
                    Debug.Write(", Null");
                    Debug.WriteLine(", No owner");
                } else {
                    Debug.Write(", " + Convert.ToInt32(reader["owner_id"]));
                    Debug.Write(", " + reader["owner_name"].ToString());
                }
                if (reader["group_id"] == DBNull.Value) {
                    Debug.Write(", Null");
                    Debug.WriteLine(", No group");
                } else {
                    Debug.Write(", " + Convert.ToInt32(reader["group_id"]));
                    Debug.WriteLine(", " + reader["group_name"].ToString());
                }
                list.Add(new TemplateFile {
                    id = Convert.ToInt32(reader["id"]),
                    name = reader["name"].ToString(),
                    path = reader["path"].ToString(),
                    localName = reader["local_name"].ToString(),
                    type = ConnectionContext.parseEnum<TemplateFile.Type>(reader["type"].ToString()),
                    ownerId = reader["owner_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["owner_id"]),
                    groupId = reader["group_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["group_id"]),
                    ownerName = reader["owner_name"] == DBNull.Value ? "None" : reader["owner_name"].ToString(),
                    groupName = reader["group_name"] == DBNull.Value ? "None" : reader["group_name"].ToString(),
                });
            }
            conn.Close();
            return list;
        }
        public UserModel getUserById(int id) {
            UserModel user = new UserModel();
            Boolean hasUser = false;
            MySqlConnection conn = getConnection();
            string sql = "Select * from `users` where `id`=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
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
            string sql = "Select * from `users` where `user_name`=@user_name";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@user_name", userName);
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
            string sql = "Select * from `users` where `user_name`=@user_name And `password`=@password";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@user_name", userName);
            cmd.Parameters.AddWithValue("@password", password);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                valid = true;
            }
            conn.Close();
            return valid;
        }

        public void updateUser(UserModel newUser, int id, int cid) {
            UserModel user = getUserById(id);
            if (user != null) {
                UserModel controlUser = getUserById(cid);
                MySqlConnection conn = getConnection();
                string sql = "Update `users` Set `user_name`=@user_name, `email`=@email, `native_name`=@native_name ";
                if (newUser.password != "") {
                    sql += ",`password`=@password ";
                }
                if (controlUser.role == UserModel.Role.admin) {
                    sql += ", `role`=@role ";
                }
                sql += "Where `id`=@id";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@user_name", newUser.userName);
                cmd.Parameters.AddWithValue("@email", newUser.email);
                cmd.Parameters.AddWithValue("@native_name", newUser.nativeName);
                cmd.Parameters.AddWithValue("@id", id);
                if (controlUser.role == UserModel.Role.admin) {
                    cmd.Parameters.AddWithValue("@role", newUser.role.ToString());
                }
                if (newUser.password != "") {
                    cmd.Parameters.AddWithValue("@password", newUser.password);
                }
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
            string sql = "Delete from `users` Where `id`=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
