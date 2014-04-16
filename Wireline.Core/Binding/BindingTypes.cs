using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wireline.Core.Binding {
    [Flags]
    public enum BindingTypes {
        NO_SUCH_TYPE,
        HEADER,
        COOKIE,
        FORM,
        ROUTE,
        AS_JSON,
        AS_OPTIONAL
    }
}
