using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateHandler.Models {

    public class MemberModel {
        public int id { get; set; }
        public int userId { get; set; }
        public string userName { get; set; }
        public string nativeName { get; set; }
        public int rights { get; set; }
    }
}
