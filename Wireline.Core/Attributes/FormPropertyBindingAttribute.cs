using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wireline.Core.Attributes {
    public class FormPropertyBindingAttribute : System.Attribute {
        public String Name { get; set; }
        public bool IsCaseSensitive { get; set; }
        public FormPropertyBindingAttribute(String name, bool isCaseSensitive) {
            Name = name;
            IsCaseSensitive = isCaseSensitive;
        }
    }
}
