using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wireline.Core {
    public class HttpAttachment {
        public String ContentDisposition { get; set; }
        public String ContentType { get; set; }
        public String FileName { get; set; }
        public String CreationDate { get; set; }
        public String ModificationDate { get; set; }
        public String ReadDate { get; set; }
        public int Size { get; set; }
        public String Name { get; set; }
        public String Voice { get; set; }
        public String VoiceMessage { get; set; }
        public String VoiceMessageNotification { get; set; }
        public String OriginatorSpokenName { get; set; }
        public String RecipientSpokenName { get; set; }
        public String SpokenSubject { get; set; }
        public String Handling { get; set; }
        public String Required { get; set; }
        public String Optional { get; set; }
        public byte[] Body { get; set; }
        public String Value { get; set; }

        public HttpAttachment() {
            ContentDisposition = "";
            ContentType = "";
            FileName = "";
            CreationDate = "";
            ModificationDate = "";
            ReadDate = "";
            Size = 0;
            Name = "";
            Voice = "";
            VoiceMessage = "";
            VoiceMessageNotification = "";
            OriginatorSpokenName = "";
            RecipientSpokenName = "";
            SpokenSubject = "";
            Handling = "";
            Required = "";
            Optional = "";
            Body = null;
            Value = null;
        }
    }
}
