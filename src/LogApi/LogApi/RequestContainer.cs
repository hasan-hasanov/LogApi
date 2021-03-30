using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace LogApi
{
    public class RequestContainer
    {
        public RequestContainer()
        {
            WebSockets = new List<WebSocket>();
            ClientLogs = new ConcurrentDictionary<string, LogModel>();
        }

        public IList<WebSocket> WebSockets { get; }

        public ConcurrentDictionary<string, LogModel> ClientLogs { get; }
    }
}
