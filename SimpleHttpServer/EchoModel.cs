using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wireline.Core.Attributes;

namespace SimpleHttpServer {
    internal class EchoModel
    {
        [FormPropertyBinding("SayThis", false)]
        [RequiredBinding]
        internal String EchoString { get; set; }

        [FormPropertyBinding("RememberMe", false)]
        [OptionalBinding]
        internal bool RememberUser { get; set; }

        [FormPropertyBinding("ToUpper", false)]
        [OptionalBinding]
        [IgnoreTypeMismatchFailures]
        internal bool UseUpperCase { get; set; }

        [FormPropertyBinding("SendDate", false)]
        [OptionalBinding]
        [IgnoreTypeMismatchFailures]
        internal DateTime SendOnDateTime { get; set; }

        [CookiePropertyBinding("SES-PUBLIC")]
        [OptionalBinding]
        internal String SessionKey { get; set; }

        [CookiePropertyBinding("SES-EXPIRES")]
        [OptionalBinding]
        [IgnoreTypeMismatchFailures]
        internal long SessionExpiresAt { get; set; }

        [CookiePropertyBinding("SES-FINGERPRINT")]
        [OptionalBinding]
        internal String SessionFingerprint { get; set; }

        internal EchoModel()
        {
        }
    }
}
