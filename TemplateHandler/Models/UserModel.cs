using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateHandler.Models {

    public class UserModel {
        public enum Role { admin, user }
        public int id { get; set; }
        public string userName { get; set; }
        public string nativeName { get; set; }
        public Role role { get; set; }
        public string password { get; set; }
        public string email { get; set; }
    }
}
