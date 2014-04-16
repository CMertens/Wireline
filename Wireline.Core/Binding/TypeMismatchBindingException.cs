using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wireline.Core.Binding {
    public class TypeMismatchBindingException : System.Exception {
        public TypeMismatchBindingException(String s) : base(s) {
            
        }

        public TypeMismatchBindingException(String s, Exception e) : base(s,e) {

        }

    }
}
