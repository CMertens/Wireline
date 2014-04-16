using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wireline.Core;

namespace SimpleHttpServer {
    class PageCreator {
        long increment = 0;

        String staticFilePath = ".";

        public void GetEchoForm(HttpServerState Server, HttpRequest Request, HttpResponse Response, Object Model) {
            try {
                EchoModel model = ((EchoModel)Model);
                String str = model.EchoString;
                if (model.UseUpperCase) {
                    str = str.ToUpperInvariant();
                }
                byte[] ba = System.Text.Encoding.UTF8.GetBytes("Complete: " + str);
                Response.Response.Cookies.Add(new System.Net.Cookie() {
                    Name = "SES-PUBLIC",
                    Value = "ABCDEF1234-567890"
                });
                Response.Response.Cookies.Add(new System.Net.Cookie() {
                    Name = "SES-EXPIRES",
                    Value = (DateTime.Now.Ticks + 1000).ToString()
                });
                Response.Response.Cookies.Add(new System.Net.Cookie() {
                    Name = "SES-FINGERPRINT",
                    Value = "ASKljdsajlkdiuAUSD7898234hTG&^7676789"
                });
                Response.Response.OutputStream.Write(ba, 0, ba.Length);
            } catch (Exception e) {
                String page = this.CreateStatusPage(500, Request.Request.Url, e);
                Byte[] p = System.Text.Encoding.UTF8.GetBytes(page);
                
                Response.Response.OutputStream.Write(p, 0, p.Length);
            }
        }


        public void GetEchoFile(HttpServerState Server, HttpRequest Request, HttpResponse Response, Object Model) {
            increment++;
            String idx = "<h1>Form Results</h1>";
            foreach (String key in Request.HeaderCollection.AllKeys) {
                idx = idx + "<b>" + key + ":</b> " + Request.HeaderCollection[key] + "<br/>";
            }
            Response.Response.StatusCode = 200;
            Response.Response.StatusDescription = "It's All Good";
            Response.Response.AddHeader("x-do-the-dance", "safety-dance");

            /*
            idx = idx + "<hr/><h3>Body:</h3>";
            idx = idx + "Content Type: " + Request.Request.ContentType + "<br/>Encoding Type: " + Request.EncodingType + "<br/><br/>";
            idx = idx + Request.RequestBody;
            */

            if (Request.AttachmentList.Count > 0) {
                idx = idx + "<hr/><h3>Attachments</h3>";
                foreach (HttpAttachment a in Request.AttachmentList) {
                    idx = idx + "<b>Name:</b> " + a.Name + "<br/>";
                    idx = idx + "<b>Content-Disposition:</b> " + a.ContentDisposition + "<br/>";
                    idx = idx + "<b>Content-Type:</b> " + a.ContentType + "<br/>";
                    idx = idx + "<b>File Name:</b> " + a.FileName + "<br/>";
                    idx = idx + "<hr/>";
                    if (a.Body != null) {
                        if (a.FileName != null && a.FileName != "") {
                            System.IO.File.WriteAllBytes(a.FileName, a.Body);
                        }
                    } else {
                        idx = idx + "<b>Value:</b> " + a.Value + "<br/>";
                    }
                }
            }

            if (Request.FormCollection != null && Request.FormCollection.AllKeys.Length > 0) {
                String q = "<br/><br/><hr/><h3>Collection</h3><br/><br/>";
                foreach (String s in Request.FormCollection) {
                    q = q + s + " = " + Request.FormCollection[s] + "<br/>";
                }
                idx = idx + q;
            }
            byte[] b = System.Text.Encoding.UTF8.GetBytes(idx);
            Response.Response.OutputStream.Write(b, 0, idx.Length);
        }

        public void GetStaticFile(HttpServerState Server, HttpRequest Request, HttpResponse Response, Object Model) {
            String path = staticFilePath + Request.Request.Url.AbsolutePath;
            if (System.IO.File.Exists(path) == false) {
                String page = this.CreateStatusPage(404, Request.Request.Url, null);
                Byte[] p = System.Text.Encoding.UTF8.GetBytes(page);
                Response.Response.OutputStream.Write(p, 0, p.Length);
            }
            
            try {
                byte[] b = System.IO.File.ReadAllBytes(path);
                Response.Response.StatusCode = 200;
                Response.Response.OutputStream.Write(b, 0, b.Length);
            } catch (Exception e) {
                String page = this.CreateStatusPage(500, Request.Request.Url, e);
                Byte[] p = System.Text.Encoding.UTF8.GetBytes(page);
                Response.Response.OutputStream.Write(p, 0, p.Length);
            }
        }

        public void CreateHomePage(HttpServerState Server, HttpRequest Request, HttpResponse Response, Object Model) {
            increment++;
            String idx = "<h1>A long time ago, in a galaxy far, far away...</h1><h2>It's the ship that made the Kessel run in less than " + increment + " parsecs!</h2><h3>Aren't you a little short for a stormtrooper?</h3>";
            Response.Response.StatusCode = 200;
            Response.Response.StatusDescription = "It's All Good";
            Response.Response.AddHeader("x-do-the-dance", "safety-dance");

            String q = "<hr/>";
            foreach (String s in Request.QueryCollection) {
                q = q + s + "=" + Request.QueryCollection[s] + "<br/>";
            }
            idx = idx + q;
            byte[] b = System.Text.Encoding.UTF8.GetBytes(idx);
            Response.Response.OutputStream.Write(b, 0, idx.Length);
        }

        public String CreateStatusPage(int status, Uri path, Exception exception){            
            increment++;
            if (status == 404) {
                return ("<h1>404</h1><h2>This is not the " + path.AbsolutePath + " you are looking for.</h2><h3>Move along.</h3>");
            } else if (status == 500) {
                String ctx = "";
                if(exception != null){
                    ctx = exception.Message + "\r\n" + exception.StackTrace;
                }
                return ("<h1>500</h1><h2>A precise hit at " + path.AbsolutePath + " has set off a chain reaction.</h2><h3>I used to bullseye womp rats in my T-16 back home. They're not much bigger than that.</h3><pre>" + ctx + "</pre>");
            } else {
                return ("Status: " + status + "; called " + increment + " times.");
            }
        }
    }
}
