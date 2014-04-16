using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wireline.Core.Attributes {
    public class RouteMatchBindingAttribute : System.Attribute {
        public String Name { get; set; }
        public RouteMatchBindingAttribute(String name) {
            Name = name;
        }
    }
}
