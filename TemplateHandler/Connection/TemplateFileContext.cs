using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public List<TemplateFileModel> getAllTemplate() {
            List<TemplateFileModel> list = new List<TemplateFileModel>();
            MySqlConnection conn = getConnection();
            string sql = "Select tf.*, tg.name group_name, u.user_name owner_name from `template_files` tf Left Join `users` u on tf.owner_id=u.id LEFT JOIN template_groups tg on tf.group_id = tg.id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                list.Add(new TemplateFileModel {
                    id = Convert.ToInt32(reader["id"]),
                    name = reader["name"].ToString(),
                    path = reader["path"].ToString(),
                    localName = reader["local_name"].ToString(),
                    type = ConnectionContext.parseEnum<TemplateFileModel.Type>(reader["type"].ToString()),
                    ownerId = reader["owner_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["owner_id"]),
                    groupId = reader["group_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["group_id"]),
                    ownerName = reader["owner_name"] == DBNull.Value ? "None" : reader["owner_name"].ToString(),
                    groupName = reader["group_name"] == DBNull.Value ? "None" : reader["group_name"].ToString(),
                    version = Convert.ToInt32(reader["version"])
                });
            }
            conn.Close();
            return list;
        }

        public TemplateFileModel getTemplate(int id) {
            TemplateFileModel file = new TemplateFileModel();
            MySqlConnection conn = getConnection();
            string sql = "Select tf.*, tg.name group_name, u.user_name owner_name from `template_files` tf Left Join `users` u on tf.owner_id=u.id LEFT JOIN template_groups tg on tf.group_id = tg.id Where tf.id=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                file.id = Convert.ToInt32(reader["id"]);
                file.name = reader["name"].ToString();
                file.path = reader["path"].ToString();
                file.localName = reader["local_name"].ToString();
                file.type = ConnectionContext.parseEnum<TemplateFileModel.Type>(reader["type"].ToString());
                file.ownerId = reader["owner_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["owner_id"]);
                file.groupId = reader["group_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["group_id"]);
                file.ownerName = reader["owner_name"] == DBNull.Value ? "None" : reader["owner_name"].ToString();
                file.groupName = reader["group_name"] == DBNull.Value ? "None" : reader["group_name"].ToString();
                file.version = Convert.ToInt32(reader["version"]);
            } else {
                return null;
            }
            conn.Close();
            return file;
        }

        public List<GrouppedTemplatesModel> getGroupedTemplate(int id) {
            List<GrouppedTemplatesModel> list = new List<GrouppedTemplatesModel>();
            MySqlConnection conn = getConnection();
            string sql = "Select tg.id ,tg.name group_name, tg.description, tg.latest_version, tg.default_version, tg.owner_id, Count(tf.id) count from `template_groups` tg Left Join `template_files` tf on tf.group_id=tg.id Join `users` u on tg.owner_id=u.id Where u.id=@id Group by tg.id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                list.Add(new GrouppedTemplatesModel {
                    id = Convert.ToInt32(reader["id"]),
                    groupName = reader["group_name"].ToString(),
                    description = reader["description"].ToString(),
                    latestVersion = Convert.ToInt32(reader["latest_version"]),
                    fileNumber = Convert.ToInt32(reader["count"]),
                    defaultVersion = Convert.ToInt32(reader["default_version"]),
                    ownerId = Convert.ToInt32(reader["owner_id"]),
                });
            }
            conn.Close();
            return list;
        }

        public List<TemplateFileModel> getTemplatesInGroup(int id) {
            List<TemplateFileModel> list = new List<TemplateFileModel>();
            MySqlConnection conn = getConnection();
            string sql = "Select tf.* From template_files tf Join template_groups tg on tf.group_id = tg.id Where tg.id=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                list.Add(new TemplateFileModel {
                    id = Convert.ToInt32(reader["id"]),
                    name = reader["name"].ToString(),
                    path = reader["path"].ToString(),
                    localName = reader["local_name"].ToString(),
                    type = ConnectionContext.parseEnum<TemplateFileModel.Type>(reader["type"].ToString()),
                    ownerId = reader["owner_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["owner_id"]),
                    groupId = reader["group_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["group_id"]),
                    ownerName = "",
                    groupName = "",
                    version = Convert.ToInt32(reader["version"])
                });
            }
            conn.Close();
            return list;
        }

        public GrouppedTemplatesModel getGroup(int id) {
            List<GrouppedTemplatesModel> list = new List<GrouppedTemplatesModel>();
            MySqlConnection conn = getConnection();
            string sql = "Select * From template_groups Where id=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                list.Add(new GrouppedTemplatesModel {
                    id = Convert.ToInt32(reader["id"]),
                    groupName = reader["name"].ToString(),
                    description = reader["description"].ToString(),
                    latestVersion = Convert.ToInt32(reader["latest_version"]),
                    defaultVersion = Convert.ToInt32(reader["default_version"]),
                    ownerId= Convert.ToInt32(reader["owner_id"]),
                    fileNumber = 0
                });
            }
            conn.Close();
            return list[0];
        }

        public void editGroup(int groupId, string description, string groupName, int defaultVersion) {
            MySqlConnection conn = getConnection();
            string sql = "Update `template_groups` Set `name`=@name, `description`=@description,`default_version`=@defaultVersion Where `id`=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", groupId);
            cmd.Parameters.AddWithValue("@defaultVersion", defaultVersion);
            cmd.Parameters.AddWithValue("@name", groupName);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public bool setMaxVersionDefault(int id) {
            MySqlConnection conn = getConnection();
            string sql = "Select version from template_groups tg join template_files tf on tg.id=tf.group_id where tg.id=@id order by version desc limit 1";
            int version = -1;
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                version = Convert.ToInt32(reader["version"]);
            }
            conn.Close();
            if (version != -1) {
                int defaultVersion = -1;
                sql = "Select default_version from template_groups  Where id=@id";
                conn.Open();
                cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    defaultVersion = Convert.ToInt32(reader["default_version"]);
                }
                conn.Close();
                if (defaultVersion != -1) {
                    if (defaultVersion != version) {
                        sql = "Update `template_groups` Set `default_version`=@defaultVersion Where `id`=@id";
                        conn.Open();
                        cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@defaultVersion", version);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        public void createTemplate(int groupId, int ownerId, int version, string templateName, TemplateFileModel.Type templateType, string pathOriginal) {
            string path = pathOriginal.Replace("\\", "/");
            string[] paths = path.Split('/');
            MySqlConnection conn = getConnection();
            string sql = "Insert into `template_files` (`name`,`path`,`local_name`,`type`,`owner_id`,`group_id`,`version`) Values(@name,@path,@localName,@type,@ownerId,@groupId,@version)";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", templateName);
            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@localName", paths[paths.Length-1]);
            cmd.Parameters.AddWithValue("@type", templateType.ToString());
            cmd.Parameters.AddWithValue("@ownerId", ownerId);
            cmd.Parameters.AddWithValue("@groupId", groupId);
            cmd.Parameters.AddWithValue("@version", version);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public int getNextVersionOfGroup(int groupId) {
            int version = -1;
            MySqlConnection conn = getConnection();
            string sql = "Select latest_version from template_groups Where id=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", groupId);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                version = Convert.ToInt32(reader["latest_version"]);
            }
            conn.Close();
            if(version == -1) {
                return version;
            } else {
                sql = "Update `template_groups` Set `latest_version`=@latestVersion, `default_version`=@defaultVersion Where `id`=@id";
                conn.Open();
                cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", groupId);
                cmd.Parameters.AddWithValue("@latestVersion", version+1);
                cmd.Parameters.AddWithValue("@defaultVersion", version + 1);
                cmd.ExecuteNonQuery();
                return version + 1;
            }
        }

        public int getOwnerOfGroup(int groupId) {
            int id = -1;
            MySqlConnection conn = getConnection();
            string sql = "Select owner_id from template_groups Where id=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", groupId);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                id = Convert.ToInt32(reader["owner_id"]);
            }
            conn.Close();
            return id;
        }

        public int removeTemplate(int id) {
            string path = "";
            int groupId = -1;
            MySqlConnection conn = getConnection();
            string sql = "Select path, group_id from template_files Where id=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                groupId = Convert.ToInt32(reader["group_id"]);
                path = reader["path"].ToString();
            }
            conn.Close();
            if (groupId != -1) {
                conn = getConnection();
                sql = "Delete from template_files Where id=@id";
                conn.Open();
                cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                conn.Close();
                path = path.Replace('/', '\\');
                if (File.Exists(path)) {
                    File.Delete(path);
                }
            }
            return groupId;
        }

        public bool deleteGroup(int id) {
            int ownerId = -1;
            MySqlConnection conn = getConnection();
            string sql = "Select owner_id from template_groups Where id=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                ownerId = Convert.ToInt32(reader["owner_id"]);
            }
            conn.Close();
            if (ownerId != -1) {
                string directory = Path.Combine(Directory.GetCurrentDirectory(), "Resources\\Templates\\" + ownerId + "\\" + id);
                if (Directory.Exists(directory)) {
                    Directory.Delete(directory, true);
                }
                conn = getConnection();
                sql = "Delete from template_groups Where id=@id";
                conn.Open();
                cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            } else {
                return false;
            }
        }

        public bool createGroup(GrouppedTemplatesModel group) {
            MySqlConnection conn = getConnection();
            string sql = "Insert into `template_groups` (`name`,`description`,`latest_version`,`default_version`,`owner_id`) Values(@name,@description,@latestVersion,@defaultVersion,@ownerId)";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", group.groupName);
            cmd.Parameters.AddWithValue("@description", group.description);
            cmd.Parameters.AddWithValue("@latestVersion", 0);
            cmd.Parameters.AddWithValue("@defaultVersion", 0);
            cmd.Parameters.AddWithValue("@ownerId", group.ownerId);
            cmd.ExecuteNonQuery();
            conn.Close();
            return true;
        }

        public Boolean tesztGroupName(string groupName, int oid) {
            int count=1;
            MySqlConnection conn = getConnection();
            string sql = "Select Count(*) count from `template_groups`  where name=@groupName And `owner_id`=@ownerId";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@groupName", groupName);
            cmd.Parameters.AddWithValue("@ownerId", oid);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                count = Convert.ToInt32(reader["count"]);
            }
            conn.Close();
            return count==0 ? true : false;
        }
    }
}
