using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;

namespace Wireline.Core {
    public class HttpRequest {
        NameValueCollection routeMatches = new NameValueCollection();
        public HttpListenerRequest Request = null;
        public NameValueCollection FormCollection = new NameValueCollection();
        public NameValueCollection QueryCollection = new NameValueCollection();
        public NameValueCollection HeaderCollection = new NameValueCollection();
        public List<HttpAttachment> AttachmentList = new List<HttpAttachment>();
        public String RequestBody;
        public byte[] RequestData;
        public String ContentType;
        public EncodingTypes EncodingType;

        public HttpRequest(HttpListenerRequest req) {
            Request = req;
            QueryCollection = req.QueryString;
            HeaderCollection = req.Headers;
            ContentType = req.ContentType;
            try {
                ParseBody(req);
            } catch (Exception e) {
                throw (e);
            }
        }

        private void ParseBody(HttpListenerRequest req){                        
            if (ContentType == null) {
                return;
            } else if (ContentType.StartsWith(@"multipart/form-data")) {
                EncodingType = EncodingTypes.MULTIPART_FORMDATA;
                String boundary = ContentType.Split(new String[] { " boundary=" }, StringSplitOptions.None).Last();
                byte[] boundaryByteArray = System.Text.Encoding.UTF8.GetBytes("--" + boundary);
                byte[] requestData = null;
                using (BinaryReader br = new BinaryReader(req.InputStream)) {
                    const int bufferSize = 4096;
                    using (var ms = new MemoryStream()) {
                        byte[] buffer = new byte[bufferSize];
                        int count;
                        while ((count = br.Read(buffer, 0, buffer.Length)) != 0) {
                            ms.Write(buffer, 0, count);
                        }
                        requestData = ms.ToArray();
                    }                    
                }
                RequestData = requestData;
                ParseMultipartBody(boundaryByteArray, RequestData);
            } else if (ContentType.StartsWith(@"text/plain")) {
                EncodingType = EncodingTypes.TEXT_PLAIN;
                using (StreamReader sr = new StreamReader(req.InputStream)) {
                    RequestBody = sr.ReadToEnd();
                }
                FormCollection = System.Web.HttpUtility.ParseQueryString(RequestBody);
            } else if (ContentType.StartsWith(@"application/x-www-form-urlencoded")) {
                EncodingType = EncodingTypes.APPLICATION_X_WWW_FORM_URLENCODED;
                using (StreamReader sr = new StreamReader(req.InputStream)) {
                    RequestBody = sr.ReadToEnd();
                }
                FormCollection = System.Web.HttpUtility.ParseQueryString(RequestBody);
            } else {
                throw new Exception("Unknown type " + ContentType);
            }            
        }

        private void ParseMultipartBody(byte[] boundaryArray, byte[] parseBody) {
            byte[][] files = SplitByteArray(boundaryArray, parseBody);
            foreach (byte[] file in files) {
                ParseAttachmentBody(file);
            }
        }

        private byte[][] SplitByteArray(byte[] boundary, byte[] body) {
            List<List<byte>> list = new List<List<byte>>();
            list.Add(new List<byte>());
            for (int x = 0; x < body.Length; x++) {
                if (body[x] == boundary[0]) {
                    int y = 0;
                    bool detectFail = false;
                    for (y = 0; y < boundary.Length; y++) {
                        int tVal = x + y;                        
                        if (body.Length < tVal || body[tVal] != boundary[y]) {
                            list.Last().Add(body[x]);
                            detectFail = true;
                            break;
                        }
                    }
                    if (detectFail == false) {
                        list.Add(new List<byte>());
                        x = x + y - 1;
                    }
                } else {
                    list.Last().Add(body[x]);
                }
            }
            byte[][] bas = new byte[list.Count()][];
            for (int x = 0; x < list.Count(); x++) {
                bas[x] = list[x].ToArray();
            }
            return bas;
        }


        private enum ContentBodyStates {
            NO_SUCH_VALUE,
            START,
            IN_HEADERS,
            IN_BODY,
            END
        }

        /// <summary>
        /// A file consists of CRLF + Headers(semicolon+space separated) + CRLF + CRLF + Data + CRLF
        /// </summary>
        /// <param name="body"></param>
        private void ParseAttachmentBody(byte[] bodyArray) {
            //char[] body = System.Text.Encoding.UTF8.GetChars(bodyArray);
            byte[] body = bodyArray;
            if (body.Length == 0) {
                return;
            }
            if (body.Length> 1 && body[0] == 0x2D && body[1] == 0x2D) {
                return;
            }

            List<byte> headersArray = new List<byte>();
            String headers = null;
            StringBuilder file = new System.Text.StringBuilder();
            ContentBodyStates state = ContentBodyStates.START;
            List<byte> bodyList = new List<byte>();
            for (int x = 0; x < body.Length; x++) {
                switch (state) {
                    case ContentBodyStates.START:
                        if (body[0] != 0x0D || body[1] != 0x0A) {
                            // TODO: Something wrong with the body here. Log and return...
                            throw new Exception("Missing CRLF at beginning of multipart file section");
                        }
                        x++;
                        state = ContentBodyStates.IN_HEADERS;
                        break;
                    case ContentBodyStates.IN_HEADERS:
                        if(body[x] == 0x0D && body[x+1] == 0x0A && body[x+2] == 0x0D && body[x+3] == 0x0A){
                            x = x + 3;
                            state = ContentBodyStates.IN_BODY;
                            break;
                        }
                        if (body[x] != 0x0D && body[x] != 0x0A) {
                            headersArray.Add(body[x]);
                        } else {
                            // turn CRLF into a semicolon separator
                            headersArray.Add(0x3B);
                            headersArray.Add(0x20);
                        }
                        break;
                    case ContentBodyStates.IN_BODY:
                        for (int y = x; y < body.Length - 2; y++) {
                            bodyList.Add(body[y]);
                        }
                        state = ContentBodyStates.END;
                        x = body.Length;
                        continue;
                    default:
                        throw new Exception("");
                }
            }
            // 
            headers = System.Text.Encoding.UTF8.GetString(headersArray.ToArray());
            // Parse out headers            
            String[] headersList = headers.ToString().Split(new String[]{"; "}, StringSplitOptions.RemoveEmptyEntries);
            HttpAttachment h = new HttpAttachment();
            h.Body = bodyList.ToArray();
            foreach (String header in headersList) {
                
                if (header.StartsWith(@"Content-Disposition: ")) {
                    // Check for Content-Disposition
                    /* This will be inline; attachment; form-data; signal; alert; icon; render; recipient-list-history; session; aib; early-session; recipient-list; notification; by-reference; info-package */
                    // 21
                    h.ContentDisposition = header.Substring(21);
                } else if (header.StartsWith("Content-Type: ")) {
                    // 14
                    h.ContentType = header.Substring(14);
                }  else if (header.StartsWith("filename=")) {
                    // Check for filename
                    // 9
                    h.FileName = header.Substring(10, header.Length - 11);
                } else if (header.StartsWith("creation-date=")) {
                    // Check for creation-date
                    // 14
                    h.CreationDate = header.Substring(15, header.Length - 16);
                } else if (header.StartsWith("modification-date=")) {
                    // Check for modification-date
                    // 18
                    h.ModificationDate = header.Substring(19, header.Length - 20);
                } else if (header.StartsWith("read-date=")) {
                    // Check for read-date
                    // 10
                    h.ReadDate = header.Substring(11, header.Length - 12);
                } else if (header.StartsWith("size=")) {
                    // Check for size
                    // 5
                    h.Size = Int32.Parse(header.Substring(6, header.Length - 7));
                } else if (header.StartsWith("name=")) {
                    // Check for name
                    // 5
                    h.Name = header.Substring(6, header.Length - 7);
                } else if (header.StartsWith("voice=")) {
                    // Check for voice
                    // 6
                    h.Voice = header.Substring(7, header.Length - 8);
                } else if (header.StartsWith("Voice-Message=")) {
                    // Check for Voice-Message
                    // 14
                    h.VoiceMessage = header.Substring(15, header.Length - 16);
                } else if (header.StartsWith("Voice-Message-Notification=")) {
                    // Check for Voice-Message-Notification
                    // 27
                    h.VoiceMessageNotification = header.Substring(28, header.Length - 29);
                } else if (header.StartsWith("Originator-Spoken-Name=")) {
                    // Check for Originator-Spoken-Name
                    // 23
                    h.OriginatorSpokenName = header.Substring(24, header.Length - 25);
                } else if (header.StartsWith("Recipient-Spoken-Name=")) {
                    // Check for Recipient-Spoken-Name
                    // 22
                    h.RecipientSpokenName = header.Substring(23, header.Length - 24);
                } else if (header.StartsWith("Spoken-Subject=")) {
                    // Check for Spoken-Subject
                    // 15
                    h.SpokenSubject = header.Substring(16, header.Length - 17);
                } else if (header.StartsWith("handling=")) {
                    // Check for handling
                    // 9
                    h.Handling = header.Substring(10, header.Length - 11);
                } else if (header.StartsWith("required=")) {
                    // Check for required
                    // 9
                    h.Required = header.Substring(10, header.Length - 11);
                } else if (header.StartsWith("optional=")) {
                    // Check for optional
                    // 9
                    h.Optional = header.Substring(10, header.Length - 11);
                }
                
            }
            if (h.FileName != null && h.FileName != "" && file.Length > 1) {
                h.Body = System.Text.Encoding.UTF8.GetBytes(file.ToString());
            } else {
                h.Value = file.ToString();
            }
            AttachmentList.Add(h);
        }

        public NameValueCollection RouteCollection {
            get {
                return routeMatches;
            }
        }

        public void AddCookie() {
        }

        public void AddRouteMatch(String name, String value){
            routeMatches.Add(name, value);
        }

    }
}
