using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyQuotesCommon
{
    public class FunnyQuotesConfiguration
    {
        public string ClientType { get; set; } = "local";
        public string FailedMessage { get; set; }
        public bool EnableSecurity { get; set; }
    }
}
