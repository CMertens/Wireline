using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wireline.Core.Attributes {
    public class CookiePropertyBindingAttribute : System.Attribute {
        public String Name { get; set; }

        public CookiePropertyBindingAttribute(String name) {
            Name = name;
        }
    }
}
