using System.Collections.Generic;

namespace LogApi
{
    public class LogModel
    {
        public LogModel(
            string path,
            string body,
            Dictionary<string, string> queryString,
            Dictionary<string, string> headers,
            Dictionary<string, string> cookies)
        {
            Path = path;
            Body = body;
            QueryString = queryString;
            Headers = headers;
            Cookies = cookies;
        }

        public string Path { get; }

        public string Body { get; set; }

        public Dictionary<string, string> QueryString { get; }

        public Dictionary<string, string> Headers { get; }

        public Dictionary<string, string> Cookies { get; }
    }
}
