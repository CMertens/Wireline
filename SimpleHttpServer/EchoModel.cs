using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wireline.Core.Attributes;

namespace SimpleHttpServer {
    public class EchoModel {
        [FormPropertyBinding("SayThis", false)]
        [RequiredBinding]
        public String EchoString { get; set; }

        [FormPropertyBinding("RememberMe", false)]
        [OptionalBinding]
        public bool RememberUser { get; set; }

        [FormPropertyBinding("ToUpper", false)]
        [OptionalBinding]
        [IgnoreTypeMismatchFailures]
        public bool UseUpperCase { get; set; }

        [FormPropertyBinding("SendDate", false)]
        [OptionalBinding]
        [IgnoreTypeMismatchFailures]
        public DateTime SendOnDateTime { get; set; }

        [CookiePropertyBinding("SES-PUBLIC")]
        [OptionalBinding]
        public String SessionKey { get; set; }

        [CookiePropertyBinding("SES-EXPIRES")]
        [OptionalBinding]
        [IgnoreTypeMismatchFailures]
        public long SessionExpiresAt { get; set; }

        [CookiePropertyBinding("SES-FINGERPRINT")]
        [OptionalBinding]
        public String SessionFingerprint { get; set; }

        public EchoModel() {
        }
    }
}
