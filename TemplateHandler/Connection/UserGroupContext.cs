using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TemplateHandler.Models;

namespace TemplateHandler.Connection {
    public class UserGroupContext {
        private string connectionString { get; set; }

        public UserGroupContext(string connectionString) {
            this.connectionString = connectionString;
        }

        private MySqlConnection getConnection() {
            return new MySqlConnection(connectionString);
        }

        public UserGroupModel[] getAllUserGroups(out string error) {
            try {
                List<UserGroupModel> list = new List<UserGroupModel>();
                MySqlConnection conn = getConnection();
                string sql = "Select ug.id, ug.name, ug.description, ug.real_group, COUNT(uug.user_id) member_number From user_groups ug Left Join users_user_groups uug ON ug.id=uug.group_id WHERE ug.real_group = '1' GROUP BY ug.id";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    list.Add(new UserGroupModel {
                        id = Convert.ToInt32(reader["id"]),
                        name = reader["name"].ToString(),
                        description = reader["description"].ToString(),
                        realGroup = Convert.ToInt32(reader["real_group"]) == 1,
                        memberNumber = Convert.ToInt32(reader["member_number"]),
                    });
                }
                conn.Close();
                error = null;
                return list.ToArray();
            }catch(Exception ex) {
                error = "[UserGroupContext/getAllUserGroups] " + ex.Message;
                return null;
            }
        }

        public UserGroupModel getUserGroupById(int id, out string error) {
            try {
                UserGroupModel group = new UserGroupModel();
                bool hasGroup = false;
                MySqlConnection conn = getConnection();
                string sql = "Select ug.id, ug.name, ug.description, ug.real_group, COUNT(uug.user_id) member_number From user_groups ug Left Join users_user_groups uug ON ug.id=uug.group_id WHERE ug.real_group = '1' AND ug.id = @id GROUP BY ug.id";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    hasGroup = true;
                    group.id = Convert.ToInt32(reader["id"]);
                    group.name = reader["name"].ToString();
                    group.description = reader["description"].ToString();
                    group.realGroup = Convert.ToInt32(reader["real_group"]) == 1;
                    group.memberNumber = Convert.ToInt32(reader["member_number"]);
                }
                conn.Close();
                if (hasGroup) {
                    error = null;
                    return group;
                } else {
                    error = null;
                    return null;
                }
            }catch(Exception ex) {
                error = "[UserGroupContext/getUserGroupById] " + ex.Message;
                return null;
            }
        }

        public MemberModel[] getUserGroupMember(int id, out string error) {
            try {
                List<MemberModel> list = new List<MemberModel>();
                MySqlConnection conn = getConnection();
                string sql = "Select uug.id id, u.id user_id, user_name, native_name, rights From users u JOIN users_user_groups uug on u.id = uug.user_id WHERE group_id = @id";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    list.Add(new MemberModel {
                        id = Convert.ToInt32(reader["id"]),
                        userId = Convert.ToInt32(reader["user_id"]),
                        userName = reader["user_name"].ToString(),
                        nativeName = reader["native_name"].ToString(),
                        rights = Convert.ToInt32(reader["rights"]),
                    });
                }
                conn.Close();
                error = null;
                return list.ToArray();
            }catch(Exception ex) {
                error = "[UserGroupContext/getUserGroupMember] " + ex.Message;
                return null;
            }
        }

        public UserGroupModel getUserGroupByGroupName(string groupName, out string error) {
            try {
                UserGroupModel group = new UserGroupModel();
                Boolean hasGroup = false;
                MySqlConnection conn = getConnection();
                string sql = "Select ug.id, ug.name, ug.description, ug.real_group, COUNT(uug.user_id) member_number From user_groups ug Left Join users_user_groups uug ON ug.id=uug.group_id WHERE ug.real_group = '1' AND ug.name = @groupName GROUP BY ug.id";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@groupName", groupName);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    hasGroup = true;
                    group.id = Convert.ToInt32(reader["id"]);
                    group.name = reader["name"].ToString();
                    group.description = reader["description"].ToString();
                    group.realGroup = Convert.ToInt32(reader["real_group"]) == 1;
                    group.memberNumber = Convert.ToInt32(reader["member_number"]);
                }
                conn.Close();
                if (hasGroup) {
                    error = null;
                    return group;
                } else {
                    error = null;
                    return null;
                }
            }catch(Exception ex) {
                error = "[UserGroupContext/getUserGroupByGroupName] " + ex.Message;
                return null;
            }
        }

        public bool editGroup(int groupId, string description, string groupName, out string error) {
            try {
                MySqlConnection conn = getConnection();
                string sql = "Update `user_groups` Set `name`=@name, `description`=@description Where `id`=@id";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", groupId);
                cmd.Parameters.AddWithValue("@name", groupName);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.ExecuteNonQuery();
                conn.Close();
                error = null;
                return true;
            }catch(Exception ex) {
                error = "[UserGroupContext/editGroup] " + ex.Message;
                return false;
            }
        }

        public bool createMember(int groupId, int userId, int rights, out string error) {
            try {
                MySqlConnection conn = getConnection();
                string sql = "Insert into `users_user_groups` (`group_id`,`user_id`,`rights`) Values(@groupId,@userId,@rights)";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@groupId", groupId);
                cmd.Parameters.AddWithValue("@rights", rights);
                cmd.ExecuteNonQuery();
                conn.Close();
                error = null;
                return true;
            }catch(Exception ex) {
                error = "[UserGroupContext/createMember] " + ex.Message;
                return false;
            }
        }

        public bool editMember(int userGroupId, int rights, out string error) {
            try {
                MySqlConnection conn = getConnection();
                string sql = "Update `users_user_groups` Set `rights`=@rights Where `id`=@userGroupId";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userGroupId", userGroupId);
                cmd.Parameters.AddWithValue("@rights", rights);
                cmd.ExecuteNonQuery();
                conn.Close();
                error = null;
                return true;
            } catch (Exception ex) {
                error = "[UserGroupContext/editMember] " + ex.Message;
                return false;
            }
        }

        public bool createGroup(UserGroupModel group, out string error) {
            try {
                MySqlConnection conn = getConnection();
                string sql = "Insert into `user_groups`(`name`,`description`,`real_group`) Values(@groupName,@description,@real)";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@groupName", group.name);
                cmd.Parameters.AddWithValue("@description", group.description);
                cmd.Parameters.AddWithValue("@real", group.realGroup ? 1 : 0);
                cmd.ExecuteNonQuery();
                conn.Close();
                error = null;
                return true;
            }catch(Exception ex) {
                error = "[UserGroupContext/createGroup] " + ex.Message;
                return false;
            }
        }

        public bool removeUser(int id, out string error) {
            try {
                MySqlConnection conn = getConnection();
                string sql = "Delete from `users_user_groups` Where `id`=@id";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                conn.Close();
                error = null;
                return true;
            }catch(Exception ex) {
                error = "[UserGroupContext/removeUser] " + ex.Message;
                return false;
            }
        }

        public bool deleteUserGroup(int id, out string error) {
            try {
                MySqlConnection conn = getConnection();
                string sql = "Select real_group from `user_groups` Where `id`=@id";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataReader reader = cmd.ExecuteReader();
                bool real = false, found = false;
                if (reader.Read()) {
                    found = true;
                    real = Convert.ToInt32(reader["real_group"]) == 1;
                }
                conn.Close();
                if (found) {
                    if (real) {
                        string directory = Path.Combine(Directory.GetCurrentDirectory(), "Resources\\Templates\\" + id);
                        if (Directory.Exists(directory)) {
                            Directory.Delete(directory, true);
                        }
                        conn = getConnection();
                        sql = "Delete from `user_groups` Where `id`=@id";
                        conn.Open();
                        cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        error = null;
                        return true;
                    } else {
                        error = "[UserGroupContext/deleteUserGroup] The found group is personal, it is unremovable from here.";
                        return false;
                    }
                } else {
                    error = "[UserGroupContext/deleteUserGroup] Cannot find user group";
                    return false;
                }
            }catch(Exception ex) {
                error = "[UserGroupContext/deleteUserGroup] " + ex.Message;
                return false;
            }
        }
    }
}
