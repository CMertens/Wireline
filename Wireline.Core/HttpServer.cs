using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace Wireline.Core
{
    public class HttpServer
    {
        public delegate void CleanUpAfterStopDelegate();
        public CleanUpAfterStopDelegate CleanUpAfterStop = null;

        public delegate bool AuthenticationStrategyDelegate();
        public AuthenticationStrategyDelegate AuthenticationStrategy = null;

        public delegate String GetStatusPageDelegate(int status, Uri path, Exception exception);
        public GetStatusPageDelegate GetStatusPage = null;


        public List<String> Prefixes {
            get {
                return listener.Prefixes.ToList();
            }
        }

        List<UrlRoute> routes = new List<UrlRoute>();
        HttpListener listener = new HttpListener();
        HttpServerState state = new HttpServerState();

        int MaxConnections = 1024;
        bool run = false;

        public HttpServer() {
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
        }

        public void AddRoute(UrlRoute route) {
            route.Bind();
            routes.Add(route);
        }

        public void AddPrefix(String prefix) {
            
            listener.Prefixes.Add(prefix);
        }



        public async void Start() {
            run = true;
            Task.Run(async () => {
                try {
                    listener.Start();
                } catch (Exception e) {
                    Console.Error.WriteLine("Failed to start server: " + e.Message);
                    return;
                }
                Semaphore sem = new Semaphore(MaxConnections, MaxConnections);
                while (run) {
                    sem.WaitOne();
                    listener.GetContextAsync().ContinueWith(async (T) => {
                        HttpListenerContext context = await T;
                        try {
                            sem.Release();
                            // Test for route, 404 if not found
                            UrlRoute route = DiscoverRoute(context.Request.Url, context.Request.HttpMethod.ToUpperInvariant());
                            if (route == null) {
                                context.Response.StatusCode = 404;
                                byte[] str = System.Text.Encoding.UTF8.GetBytes(GetStatusPage(404, context.Request.Url, null));
                                context.Response.OutputStream.Write(str, 0, str.Length);
                            } else {
                                try {                                    
                                    // Execute response
                                    HttpResponse response = await ProcessRequest(route, context);
                                    try {
                                        // Finish stuff up
                                        response.Response.Close();
                                    } catch (Exception e) {
                                        context.Response.StatusCode = 500;
                                        byte[] str = System.Text.Encoding.UTF8.GetBytes(GetStatusPage(500, context.Request.Url, e));
                                        context.Response.OutputStream.Write(str, 0, str.Length);
                                    } finally {
                                        response.Close();
                                    }
                                } catch (Exception e) {
                                    context.Response.StatusCode = 500;
                                    byte[] str = System.Text.Encoding.UTF8.GetBytes(GetStatusPage(500, context.Request.Url, e));
                                    context.Response.OutputStream.Write(str, 0, str.Length);
                                } 
                            }
                        } catch (Exception e) {
                            context.Response.StatusCode = 500;
                            byte[] str = System.Text.Encoding.UTF8.GetBytes(GetStatusPage(500, context.Request.Url, e));
                            context.Response.OutputStream.Write(str, 0, str.Length);
                        } finally {
                            context.Response.Close();
                        }
                        return;
                    });
                }
                listener.Stop();
                listener.Close();
                CleanUpAfterStop();
            }).Wait();
        }

        public void Stop() {
            run = false;            
        }

        private UrlRoute DiscoverRoute(Uri url, String method) {
            UrlRoute route = null;
            String path = url.AbsolutePath;
            var myRoutes = routes.OrderBy(r => r.Priority).Where(r => r.HttpMethod.ToUpperInvariant() == method.ToUpperInvariant());
            foreach(UrlRoute myRoute in myRoutes)
            {
                if (myRoute.RouteRegex.IsMatch(path)) {                    
                    route = myRoute;
                }
            }
            
            return route;
        }


        private async Task<HttpResponse> ProcessRequest(UrlRoute route, HttpListenerContext context) {
            HttpRequest request = new HttpRequest(context.Request);
            HttpResponse response = new HttpResponse();
            Object model = null;
            response.Response = context.Response;
            // BIND MODEL
            if (route.Model != null) {
                var ctors = route.Model.GetConstructors();
                if (ctors.Length < 1) {
                    throw new Exception("Could not find parameterless constructor for type " + route.Model.Name);
                }
                model = ctors[0].Invoke(new object[]{});
                route.RequestBinder.Bind(request, ref model);
            }
            // DISPATCH
            if (route.DelegatedController != null) {
                route.DelegatedController(state, request, response, model);
            } else {
                throw new NotImplementedException();
            }
            return response;
        }

    }
}
