using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ExperimentalWebServer {
    public sealed class ExHttpServer {
        HttpListener _listener = new HttpListener();
        readonly int _accepts = 1024;
               

        public List<string> Prefixes {
            get { return _listener.Prefixes.ToList(); }
        }


        public void Run(params string[] uriPrefixes) {
            // Establish a host-handler context:

            _listener.IgnoreWriteExceptions = true;

            // Add the server bindings:
            foreach (var prefix in uriPrefixes)
                _listener.Prefixes.Add(prefix);

            Task.Run(async () => {

                // Initialize the handler:
                try {
                    // Start the HTTP listener:
                    _listener.Start();
                } catch (HttpListenerException hlex) {
                    Console.Error.WriteLine(hlex.Message);
                    return;
                }

                var sem = new Semaphore(_accepts, _accepts);

                while (true) {
                    sem.WaitOne();
                    _listener.GetContextAsync().ContinueWith(async (t) => {
                        string errMessage;

                        try {
                            sem.Release();
                            Console.WriteLine("Connected");
                            var ctx = await t;
                            await ProcessListenerContext(ctx);
                            return;
                        } catch (Exception ex) {
                            errMessage = ex.ToString();
                        }

                        await Console.Error.WriteLineAsync(errMessage);
                    });
                }
            }).Wait();
        }

        static async Task ProcessListenerContext(HttpListenerContext listenerContext) {

            Console.WriteLine("Sending response");
            listenerContext.Response.StatusCode = 200;
            listenerContext.Response.StatusDescription = "OK-COOL-FROOD";
            listenerContext.Response.Headers["x-test"] = "true";
            listenerContext.Response.AppendCookie(new Cookie("COOOOKIE","This:is,A:Cookie"));
            
            byte[] b = System.Text.Encoding.UTF8.GetBytes("Seed: " + new Random().Next() + "<br/>Verb: " + listenerContext.Request.HttpMethod.ToString());

            listenerContext.Response.ContentLength64 = b.Length;
            listenerContext.Response.OutputStream.Write(b, 0, b.Length);
            listenerContext.Response.Close();
        }
    }
}