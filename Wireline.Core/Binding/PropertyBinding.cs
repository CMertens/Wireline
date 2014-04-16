using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Wireline.Core.Binding {
    public class PropertyBinding {
        public BindingTypes BindType { get; set; }
        public String Source { get; set; }
        public PropertyInfo Target { get; set; }
        public bool AsJson { get; set; }
        public bool AsXml { get; set; }
        public bool IgnoreTypeConversionFailures { get; set; }
        public bool IsRequired { get; set; }
        public bool IsCaseSensitive { get; set; }

        
        public PropertyBinding() {
            IsCaseSensitive = false;
            IsRequired = false;
            IgnoreTypeConversionFailures = false;
            AsJson = false;
            AsXml = false;
        }
    }
}
