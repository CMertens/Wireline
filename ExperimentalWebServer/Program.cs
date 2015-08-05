using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentalWebServer {
    public static class Program {
        public static void Main(String[] args) {
            ExHttpServer server = new ExHttpServer();
            server.Run(new String[] { "http://+:1234/" });
        }
    }
}
