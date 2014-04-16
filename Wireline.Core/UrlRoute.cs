using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using Wireline.Core.Binding;
using System.Net;

namespace Wireline.Core {
    public class UrlRoute {
        public delegate void ProcessRequestDelegate(HttpServerState Server, HttpRequest Request, HttpResponse Response, Object Model);
        public delegate bool AuthenticateDelegate(HttpServerState Server, HttpRequest Request);

        public String HttpMethod { get; set; }
        public int Priority { get; set; }
        public String Name { get; set; }
        public Regex RouteRegex { get; set; }

        public Type Model { get; set; }
        public ModelBinder RequestBinder { get; set; }

        public Object Controller { get; set; }
        public MethodInfo ControllerMethod { get; set; }

        public ProcessRequestDelegate DelegatedController { get; set; }
        public AuthenticateDelegate Authenticator { get; set; }

        public void Bind() {
            if (Model != null) {
                ModelBinder mb = new ModelBinder(Model);
                RequestBinder = mb;
            }
        }
    }
}
