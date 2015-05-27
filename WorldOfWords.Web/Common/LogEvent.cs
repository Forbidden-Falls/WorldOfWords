 using System;
 using System.Web.Management;

namespace WorldOfWords.Web.Common
{
    public class LogEvent : WebRequestErrorEvent
    {
        public LogEvent(string message)
            : base(null, null, 100001, new Exception(message))
        {
        }
    }
}
}
