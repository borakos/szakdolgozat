using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public UserModel[] getAllUsers(out string error) {
            try {
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
                        nativeName = reader["native_name"] == DBNull.Value ? "" : reader["native_name"].ToString(),
                        role = ConnectionContext.parseEnum<UserModel.Role>(reader["role"].ToString()),
                        password = reader["password"].ToString(),
                        email = reader["email"] == DBNull.Value ? "" : reader["email"].ToString(),
                    });
                }
                conn.Close();
                error = null;
                return list.ToArray();
            }catch(Exception ex) {
                error = "[UserContext/getAllUsers] "+ex.Message;
                return null;
            }
        }
        public UserModel[] getFilteredUsers(string filter, out string error) {
            try {
                List<UserModel> list = new List<UserModel>();
                MySqlConnection conn = getConnection();
                string sql = "Select * from `users` WHERE user_name LIKE '%" + filter + "%' OR native_name LIKE '%" + filter + "%';";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    list.Add(new UserModel {
                        id = Convert.ToInt32(reader["id"]),
                        userName = reader["user_name"].ToString(),
                        nativeName = reader["native_name"] == DBNull.Value ? "" : reader["native_name"].ToString(),
                        role = ConnectionContext.parseEnum<UserModel.Role>(reader["role"].ToString()),
                        password = reader["password"].ToString(),
                        email = reader["email"] == DBNull.Value ? "" : reader["email"].ToString(),
                    });
                }
                conn.Close();
                error = null;
                return list.ToArray();
            }catch(Exception ex) {
                error = "[UserContext/getFilteredUsers] " + ex.Message;
                return null;
            }
        }
        public UserModel getUserById(int id, out string error) {
            try {
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
                    user.nativeName = reader["native_name"] == DBNull.Value ? "" : reader["native_name"].ToString();
                    user.role = ConnectionContext.parseEnum<UserModel.Role>(reader["role"].ToString());
                    user.password = reader["password"].ToString();
                    user.email = reader["email"] == DBNull.Value ? "" : reader["email"].ToString();
                }
                conn.Close();
                if (hasUser) {
                    error = null;
                    return user;
                } else {
                    error = null;
                    return null;
                }
            }catch(Exception ex) {
                error = "[UserContext/getUserById] " + ex.Message;
                return null;
            }
        }

        public UserModel getUserByUserName(string userName, out string error) {
            try {
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
                    user.nativeName = reader["native_name"] == DBNull.Value ? "" : reader["native_name"].ToString();
                    user.role = ConnectionContext.parseEnum<UserModel.Role>(reader["role"].ToString());
                    user.password = reader["password"].ToString();
                    user.email = reader["email"] == DBNull.Value ? "" : reader["email"].ToString();
                }
                conn.Close();
                if (hasUser) {
                    error = null;
                    return user;
                } else {
                    error = null;
                    return null;
                }
            }catch(Exception ex) {
                error = "[UserContext/getUserByUserName] " + ex.Message;
                return null;
            }
        }

        public bool validateUser(string userName, string password, out string error) {
            try {
                bool valid = false;
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
                error = null;
                return valid;
            }catch (Exception ex) {
                error = "[UserContext/validateUser] " + ex.Message;
                return false;
            }
        }

        public bool updateUser(UserModel newUser,int id, int cid, out string error) {
            try {
                UserModel user = getUserById(id, out error);
                if (user != null) {
                    UserModel controlUser = getUserById(cid, out error);
                    if (controlUser != null) {
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
                        if (newUser.nativeName == "") {
                            cmd.Parameters.AddWithValue("@native_name", DBNull.Value);
                        } else {
                            cmd.Parameters.AddWithValue("@native_name", newUser.nativeName);
                        }
                        Debug.Write("\n\n" + user.email + "\n\n");
                        if (newUser.email == "") {
                            cmd.Parameters.AddWithValue("@email", DBNull.Value);
                        } else {
                            cmd.Parameters.AddWithValue("@email", newUser.email);
                        }
                        cmd.Parameters.AddWithValue("@id", id);
                        if (controlUser.role == UserModel.Role.admin) {
                            cmd.Parameters.AddWithValue("@role", newUser.role.ToString());
                        }
                        if (newUser.password != "") {
                            cmd.Parameters.AddWithValue("@password", newUser.password);
                        }
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        error = null;
                        return true;
                    } else if (error == null) {
                        error = "[UserContext/updateUser] Cannot find control user.";
                        return false;
                    } else {
                        error = "[UserContext/updateUser] " + error;
                        return false;
                    }
                } else if (error == null) {
                    error = "[UserContext/updateUser] Cannot find user.";
                    return false;
                } else {
                    error = "[UserContext/updateUser] " + error;
                    return false;
                }
            }catch(Exception ex) {
                error = "[UserContext/updateUser] " + ex.Message;
                return false;
            }
        }

        public bool createUser(UserModel user, out string error) {
            try {
                MySqlConnection conn = getConnection();
                string sql = "Insert into `users`(`user_name`,`role`,`email`,`native_name`,`password`) Values(@user_name,@role,@email,@native_name,@password)";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@user_name", user.userName);
                cmd.Parameters.AddWithValue("@role", user.role.ToString());
                cmd.Parameters.AddWithValue("@password", user.password);
                if (user.nativeName == "") {
                    cmd.Parameters.AddWithValue("@native_name", DBNull.Value);
                } else {
                    cmd.Parameters.AddWithValue("@native_name", user.nativeName);
                }
                if (user.email == "") {
                    cmd.Parameters.AddWithValue("@email", DBNull.Value);
                } else {
                    cmd.Parameters.AddWithValue("@email", user.email);
                }
                cmd.ExecuteNonQuery();
                conn.Close();
                UserModel insertedUser = getUserByUserName(user.userName, out error);
                if (insertedUser != null) {
                    int groupId = createPersonalGroup(insertedUser.id, out error);
                    if (groupId != -1) {
                        if (createPersonalAssociation(insertedUser.id, groupId, out error)) {
                            error = null;
                            return true;
                        } else {
                            error = "[UserContext/createUser] " + error;
                            return false;
                        }
                    } else if (error == null) {
                        error = "[UserContext/createUser] Personal user group creation failed";
                        return false;
                    } else {
                        error = "[UserContext/createUser] " + error;
                        return false;
                    }
                } else if (error == null){
                    error = "[UserContext/createUser] Cannot find created user, so cannot create personal group";
                    return false;
                } else {
                    error = "[UserContext/createUser] " + error;
                    return false;
                }
            }catch(Exception ex) {
                error = ex.Message;
                return false;
            }
        }

        private bool createPersonalAssociation(int userId, int groupId, out string error) {
            try {
                MySqlConnection conn = getConnection();
                string sql = "Insert into `users_user_groups`(`user_id`,`group_id`,`rights`) Values(@userId,@groupId,'7')";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@groupId", groupId);
                cmd.ExecuteNonQuery();
                conn.Close();
                error = null;
                return true;
            }catch(Exception ex) {
                error = "[UserContext/createPersonalAssociation] " + ex.Message;
                return false;
            }
        }

        private int createPersonalGroup(int id, out string error) {
            try {
                int groupId = -1;
                MySqlConnection conn = getConnection();
                string sql = "Insert into `user_groups`(`name`,`description`,`real_group`) Values('Personal',@id,'0')";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                conn.Close();
                sql = "Select id from `user_groups` where `description`=@id And `real_group`='0'";
                conn.Open();
                cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    groupId = Convert.ToInt32(reader["id"]);
                }
                conn.Close();
                error = null;
                return groupId;
            }catch(Exception ex) {
                error = "[UserContext/createPersonalGroup] " + ex.Message;
                return -1;
            }
        }

        private int getPersonalGroupId(int userId, out string error) {
            try {
                int groupId = -1;
                string sql = "Select ug.id FROM users_user_groups uug JOIN user_groups ug ON uug.group_id=ug.id WHERE user_id=@id AND real_group='0'";
                MySqlConnection conn = getConnection();
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    groupId = Convert.ToInt32(reader["id"]);
                }
                error = null;
                return groupId;
            } catch (Exception ex){
                error = "[UserContext/getPersonalGroupId] " + ex.Message;
                return -1;
            }
        }

        public bool deleteUser(int id, out string error) {
            try {
                int groupId = getPersonalGroupId(id, out error);
                if (groupId != -1) {
                    string directory = Path.Combine(Directory.GetCurrentDirectory(), "Resources\\Templates\\" + groupId);
                    if (Directory.Exists(directory)) {
                        Directory.Delete(directory, true);
                    }
                    MySqlConnection conn = getConnection();
                    string sql = "Delete from `users` Where `id`=@id";
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    sql = "Delete from `user_groups` Where `id`=@id";
                    conn.Open();
                    cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", groupId);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    error = null;
                    return true;
                } else if (error == null){
                    error = "[UserContext/deleteUser] Cannot find user personal group.";
                    return false;
                } else {
                    error = "[UserContext/deleteUser] " + error;
                    return false;
                }
            }catch(Exception ex) {
                error = "[UserContext/deleteUser] " + ex.Message;
                return false;
            }
        }
    }
}
