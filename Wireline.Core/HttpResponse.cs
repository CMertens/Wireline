using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Wireline.Core {
    public class HttpResponse {

        public HttpListenerResponse Response { get; set; }


        public void Close() {
        }
    }
}
