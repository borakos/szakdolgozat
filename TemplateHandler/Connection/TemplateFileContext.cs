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

        public List<GrouppedTemplatesModel> getGroupedTemplate(int id) {
            List<GrouppedTemplatesModel> list = new List<GrouppedTemplatesModel>();
            MySqlConnection conn = getConnection();
            string sql = "Select tg.id ,tg.name group_name, tg.description, tg.latest_version, tg.default_version, Count(tg.id) count from `template_groups` tg JOIN template_files tf on tf.group_id = tg.id Join `users` u on tf.owner_id=u.id Where u.id=@id Group by tg.id";
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
                    defaultVersion = Convert.ToInt32(reader["default_version"])
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
                    fileNumber = 0
                });
            }
            conn.Close();
            return list[0];
        }

        public void editGroup(GrouppedTemplatesModel group) {
            MySqlConnection conn = getConnection();
            string sql = "Update `template_groups` Set `name`=@name, `description`=@description,`default_version`=@defaultVersion, `latest_version`=@latestVersion Where `id`=@id";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", group.id);
            cmd.Parameters.AddWithValue("@defaultVersion", group.defaultVersion);
            cmd.Parameters.AddWithValue("@latestVersion", group.latestVersion);
            cmd.Parameters.AddWithValue("@name", group.groupName);
            cmd.Parameters.AddWithValue("@description", group.description);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void createTemplate(TemplateFileModel file) {
            MySqlConnection conn = getConnection();
            string sql = "Insert into `template_files` (`name`,`path`,`local_name`,`type`,`owner_id`,`group_id`,`version`) Values(@name,@path,@localName,@type,@ownerId,@groupId,@version)";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name",file.name);
            cmd.Parameters.AddWithValue("@path","Resource/Templates");
            cmd.Parameters.AddWithValue("@localName",file.ownerId+"_"+file.groupId+"_"+file.version+"_"+file.name);
            cmd.Parameters.AddWithValue("@type",file.type.ToString());
            cmd.Parameters.AddWithValue("@ownerId",file.ownerId);
            cmd.Parameters.AddWithValue("@groupId",file.groupId);
            cmd.Parameters.AddWithValue("@version", file.version);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public Boolean tesztGroupName(string groupName, int oid) {
            int count=1;
            MySqlConnection conn = getConnection();
            string sql = "Select Count(*) count from `template_groups` tg join `template_files` tf on tg.id=tf.group_id where tg.name=@groupName And `owner_id`=@ownerId";
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
