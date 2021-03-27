using System.Collections.Generic;

namespace LogApi
{
    public class LogModel
    {
        public LogModel(
            string path,
            string queryString,
            Dictionary<string, string> headers,
            Dictionary<string, string> cookies)
        {
            Path = path;
            QueryString = queryString;
            Headers = headers;
            Cookies = cookies;
        }

        public string Path { get; }

        public string QueryString { get; }

        public Dictionary<string, string> Headers { get; }

        public Dictionary<string, string> Cookies { get; }
    }
}
