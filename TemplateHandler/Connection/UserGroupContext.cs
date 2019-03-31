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

        public List<UserGroupModel> getAllUserGroups() {
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
            return list;
        }

        public UserGroupModel getUserGroupById(int id) {
            UserGroupModel group = new UserGroupModel();
            Boolean hasGroup = false;
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
                return group;
            } else {
                return null;
            }
        }

        public List<MemberModel> getUserGroupMember(int id) {
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
            return list;
        }

        public UserGroupModel getUserGroupByGroupName(string groupName) {
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
                return group;
            } else {
                return null;
            }
        }

        public void editGroup(int groupId, string description, string groupName) {
            MySqlConnection conn = getConnection();
            string sql = "Update `user_groups` Set `name`=@name, `description`=@description Where `id`=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", groupId);
            cmd.Parameters.AddWithValue("@name", groupName);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void createMember(int groupId, int userId, int rights) {
            MySqlConnection conn = getConnection();
            string sql = "Insert into `users_user_groups` (`group_id`,`user_id`,`rights`) Values(@groupId,@userId,@rights)";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@groupId", groupId);
            cmd.Parameters.AddWithValue("@rights", rights);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public Boolean createGroup(UserGroupModel group) {
            MySqlConnection conn = getConnection();
            string sql = "Insert into `user_groups`(`name`,`description`,`real_group`) Values(@groupName,@description,@real)";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@groupName", group.name);
            cmd.Parameters.AddWithValue("@description", group.description);
            cmd.Parameters.AddWithValue("@real", group.realGroup ? 1 : 0);
            cmd.ExecuteNonQuery();
            conn.Close();
            return true;
        }

        public Boolean removeUser(int id) {
            MySqlConnection conn = getConnection();
            string sql = "Delete from `users_user_groups` Where `id`=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            conn.Close();
            return true;
        }

        public Boolean deleteUserGroup(int id) {
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
                }
                return true;
            } else {
                return false;
            }
        }
    }
}
