using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using TemplateHandler.Connection;
using TemplateHandler.Models;

namespace TemplateHandler.Connection {
    public class ConnectionContext {

        public static T parseEnum<T>(string value) {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        private static ConnectionContext instance;

        private string connectionString { get; set; }

        public static ConnectionContext Instace {
            get {
                if( instance== null) {
                    instance = new ConnectionContext();
                }
                return instance;
            }
        }

        public void setConnectionString(string connectionString) {
            this.connectionString = connectionString;
        }

        public UserContext createUserContext() {
            return new UserContext(connectionString);
        }

        public TemplateFileContext createTemplateFileContext() {
            return new TemplateFileContext(connectionString);
        }
    }
}