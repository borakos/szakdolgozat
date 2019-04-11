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

        public List<TemplateFileModel> getAllTemplate(out string error) {
            try {
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
                error = null;
                return list;
            } catch (Exception ex) {
                error = "[TemplateFileContext/getAllTemplate] " + ex.Message;
                return null;
            }
        }

        public TemplateFileModel getTemplate(int id, out string error) {
            try {
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
                    error = null;
                    return null;
                }
                conn.Close();
                error = null;
                return file;
            } catch (Exception ex) {
                error = "[TemplateFileContext/getTemplate] " + ex.Message;
                return null;
            }
        }

        public GrouppedTemplatesModel[] getGroupedTemplate(int id, out string error) {
            try {
                List<GrouppedTemplatesModel> list = new List<GrouppedTemplatesModel>();
                MySqlConnection conn = getConnection();
                string sql = "SELECT tg.id ,tg.name group_name, tg.description, tg.latest_version, tg.default_version, tg.owner_id, ug.name owner_name, Count(tf.id) count ";
                sql += "FROM `template_groups` tg LEFT JOIN `template_files` tf on tf.group_id=tg.id JOIN `user_groups` ug on tg.owner_id=ug.id ";
                sql += "WHERE ug.id IN (SELECT group_id FROM users_user_groups uug WHERE user_id = @id AND rights>'5') GROUP BY tg.id";
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
                        ownerName = reader["owner_name"].ToString(),
                    });
                }
                conn.Close();
                error = null;
                return list.ToArray();
            } catch (Exception ex) {
                error = "[TemplateFileContext/getGroupedTemplate] " + ex.Message;
                return null;
            }
        }

        public GrouppedTemplatesModel[] getGroupedTemplateExecution(int id, out string error) {
            try {
                List<GrouppedTemplatesModel> list = new List<GrouppedTemplatesModel>();
                MySqlConnection conn = getConnection();
                string sql = "SELECT tg.id ,tg.name group_name, tg.description, tg.latest_version, tg.default_version, tg.owner_id, ug.name owner_name, Count(tf.id) count ";
                sql += "FROM `template_groups` tg LEFT JOIN `template_files` tf on tf.group_id=tg.id JOIN `user_groups` ug on tg.owner_id=ug.id ";
                sql += "WHERE ug.id IN (SELECT group_id FROM users_user_groups uug WHERE user_id = @id AND rights%2 = '1') GROUP BY tg.id";
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
                        ownerName = reader["owner_name"].ToString(),
                    });
                }
                conn.Close();
                error = null;
                return list.ToArray();
            } catch (Exception ex) {
                error = "[TemplateFileContext/getGroupedTemplateExecution] " + ex.Message;
                return null;
            }
        }

        public List<TemplateFileModel> getTemplatesInGroup(int id, out string error) {
            try {
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
                error = null;
                return list;
            } catch (Exception ex) {
                error = "[TemplateFileContext/getTemplatesInGroup] " + ex.Message;
                return null;
            }
        }

        public GrouppedTemplatesModel getGroup(int id, out string error) {
            try {
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
                        ownerId = Convert.ToInt32(reader["owner_id"]),
                        fileNumber = 0
                    });
                }
                conn.Close();
                error = null;
                return list[0];
            } catch (Exception ex) {
                error = "[TemplateFileContext/getGroup] " + ex.Message;
                return null;
            }
        }

        public UserGroupModel[] getUserGroups(int id, out string error) {
            try {
                List<UserGroupModel> list = new List<UserGroupModel>();
                MySqlConnection conn = getConnection();
                string sql = "SELECT ug.id, ug.name group_name, ug.description, ug.real_group FROM users_user_groups uug JOIN user_groups ug ON uug.group_id=ug.id ";
                sql += "WHERE ug.id IN (SELECT group_id FROM users_user_groups uug WHERE user_id = @id And uug.rights > 5) And uug.user_id =@id";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    list.Add(new UserGroupModel {
                        id = Convert.ToInt32(reader["id"]),
                        name = reader["group_name"].ToString(),
                        description = reader["description"].ToString(),
                        realGroup = Convert.ToInt32(reader["real_group"]) == 1,
                        memberNumber = 0
                    });
                }
                conn.Close();
                error = null;
                return list.ToArray();
            } catch (Exception ex) {
                error = "[TemplateFileContext/getUserGroups] " + ex.Message;
                return null;
            }
        }

        private bool setOwnerOfFiles(int groupId, int ownerId, out string error) {
            try {
                MySqlConnection conn = getConnection();
                string sql = "Select owner_id from template_groups Where id=@id";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", groupId);
                MySqlDataReader reader = cmd.ExecuteReader();
                int owner = -1;
                if (reader.Read()) {
                    owner = Convert.ToInt32(reader["owner_id"]);
                }
                conn.Close();
                if ((owner != -1) && (owner != ownerId)) {
                    conn.Open();
                    sql = "Update template_files Set owner_id=@newId Where group_id=@groupId";
                    cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@newId", ownerId);
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                error = null;
                return true;
            } catch (Exception ex) {
                error = "[TemplateFileContext/setOwnerOfFiles] " + ex.Message;
                return false;
            }
        }

        public bool editGroup(int groupId, string description, string groupName, int ownerId, int defaultVersion, out string error) {
            try {
                if (setOwnerOfFiles(groupId, ownerId, out error)) {
                    MySqlConnection conn = getConnection();
                    string sql = "Update `template_groups` Set `name`=@name, `description`=@description,`default_version`=@defaultVersion, `owner_id`=@ownerId Where `id`=@id";
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", groupId);
                    cmd.Parameters.AddWithValue("@defaultVersion", defaultVersion);
                    cmd.Parameters.AddWithValue("@name", groupName);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@ownerId", ownerId);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    error = null;
                    return true;
                } else {
                    error = "[TemplateFileContext/editGroup] " + error;
                    return false;
                }
            } catch (Exception ex) {
                error = "[TemplateFileContext/editGroup] " + ex.Message;
                return false;
            }
        }

        public bool setMaxVersionDefault(int id, out string error) {
            try {
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
                        error = null;
                        return true;
                    } else {
                        error = null;
                        return false;
                    }
                } else {
                    error = null;
                    return false;
                }
            } catch (Exception ex) {
                error = "[TemplateFileContext/setMaxVersionDefault] " + ex.Message;
                return false;
            }
        }

        public bool createTemplate(int groupId, int ownerId, int version, string templateName, TemplateFileModel.Type templateType, string pathOriginal, out string error) {
            try {
                string path = pathOriginal.Replace("\\", "/");
                string[] paths = path.Split('/');
                MySqlConnection conn = getConnection();
                string sql = "Insert into `template_files` (`name`,`path`,`local_name`,`type`,`owner_id`,`group_id`,`version`) Values(@name,@path,@localName,@type,@ownerId,@groupId,@version)";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", templateName);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@localName", paths[paths.Length - 1]);
                cmd.Parameters.AddWithValue("@type", templateType.ToString());
                cmd.Parameters.AddWithValue("@ownerId", ownerId);
                cmd.Parameters.AddWithValue("@groupId", groupId);
                cmd.Parameters.AddWithValue("@version", version);
                cmd.ExecuteNonQuery();
                conn.Close();
                error = null;
                return true;
            } catch (Exception ex) {
                error = "[TemplateFileContext/createTemplate] " + ex.Message;
                return false;
            }
        }

        public int getNextVersionOfGroup(int groupId, out string error) {
            try {
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
                if (version == -1) {
                    error = null;
                    return -1;
                } else {
                    sql = "Update `template_groups` Set `latest_version`=@latestVersion, `default_version`=@defaultVersion Where `id`=@id";
                    conn.Open();
                    cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", groupId);
                    cmd.Parameters.AddWithValue("@latestVersion", version + 1);
                    cmd.Parameters.AddWithValue("@defaultVersion", version + 1);
                    cmd.ExecuteNonQuery();
                    error = null;
                    return version + 1;
                }
            } catch (Exception ex) {
                error = "[TemplateFileContext/getNextVersionOfGroup] " + ex.Message;
                return -1;
            }
        }

        public int getOwnerOfGroup(int groupId, out string error) {
            try {
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
                error = null;
                return id;
            } catch (Exception ex) {
                error = "[TemplateFileContext/getOwnerOfGroup] " + ex.Message;
                return -1;
            }
        }

        public int removeTemplate(int id, out string error) {
            try {
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
                error = null;
                return groupId;
            } catch (Exception ex) {
                error = "[TemplateFileContext/removeTemplate] " + ex.Message;
                return -1;
            }
        }

        public bool deleteGroup(int id, out string error) {
            try {
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
                    error = null;
                    return true;
                } else {
                    error = null;
                    return false;
                }
            } catch (Exception ex) {
                error = "[TemplateFileContext/deleteGroup] " + ex.Message;
                return false;
            }
        }

        public bool createGroup(GrouppedTemplatesModel group, out string error) {
            try {
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
                error = null;
                return true;
            } catch (Exception ex) {
                error = "[TemplateFileContext/createGroup] " + ex.Message;
                return false;
            }
        }

        public bool tesztGroupName(string groupName, int oid, out string error) {
            try {
                int count = 1;
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
                error = null;
                return count == 0 ? true : false;
            } catch (Exception ex) {
                error = "[TemplateFileContext/tesztGroupName] " + ex.Message;
                return false;
            }
        }
    }
}
