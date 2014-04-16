using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wireline;
using Wireline.Core;
using Wireline.Core.Attributes;
using Wireline.Core.Binding;

namespace SimpleHttpServer {
    class Program {
        static void Main(string[] args) {
            HttpServer server = new HttpServer();
            String local = "http://+:1234/";
            server.AddPrefix(local);
            PageCreator spc = new PageCreator();
            server.GetStatusPage = spc.CreateStatusPage;
            server.AddRoute(new UrlRoute() {
                Name = "Home",
                Priority = 1,
                RouteRegex = new System.Text.RegularExpressions.Regex(@"^\/$"),
                HttpMethod = "GET",
                DelegatedController = spc.CreateHomePage
            });

            server.AddRoute(new UrlRoute() {
                Name = "Static",
                Priority = 1,
                RouteRegex = new System.Text.RegularExpressions.Regex(@"^\/[Ss][Tt][Aa][Tt][Ii][Cc]\/.+\.[A-Za-z]+$"),
                HttpMethod = "GET",
                DelegatedController = spc.GetStaticFile
            });

            server.AddRoute(new UrlRoute() {
                Name = "EchoFile",
                Priority = 2,
                HttpMethod = "POST",
                RouteRegex = new System.Text.RegularExpressions.Regex(@"^\/Echo\/File$"),
                DelegatedController = spc.GetEchoFile
            });

            server.AddRoute(new UrlRoute() {
                Name = "EchoForm",
                Priority = 2,
                HttpMethod = "POST",
                RouteRegex = new System.Text.RegularExpressions.Regex(@"^\/Echo\/Form$"),
                DelegatedController = spc.GetEchoForm,
                Model = typeof(EchoModel)
            });

            server.Start();
            Console.WriteLine("Press any key to stop");
            Console.ReadKey();
            server.Stop();
        }
    }
}
