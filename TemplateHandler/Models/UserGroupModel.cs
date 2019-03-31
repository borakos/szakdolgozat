using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateHandler.Models {

    public class UserGroupModel {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool realGroup { get; set; }
        public int memberNumber { get; set; }
    }
}
