using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LogApi
{
    public class Startup
    {
        private readonly ConcurrentDictionary<string, IList<LogModel>> _clientLogs;

        public Startup()
        {
            _clientLogs = new ConcurrentDictionary<string, IList<LogModel>>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
                RequireHeaderSymmetry = false,
                ForwardLimit = null,
                KnownNetworks = { new IPNetwork(IPAddress.Parse("::ffff:172.17.0.1"), 104) }
            });

            app.UseStaticFiles();
            app.UseWebSockets();

            app.Run(async context =>
            {
                string ipAddress = context.Connection.RemoteIpAddress.ToString();
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));

                        while (!cts.IsCancellationRequested)
                        {
                            byte[] message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_clientLogs[ipAddress]));

                            await webSocket.SendAsync(
                                new ArraySegment<byte>(message, 0, message.Length),
                                WebSocketMessageType.Text,
                                endOfMessage: true,
                                cts.Token);

                            await Task.Delay(TimeSpan.FromMinutes(5));
                        }
                    }
                }
                else
                {
                    // TODO: Ignore browser favicon requests.
                    _clientLogs.AddOrUpdate(ipAddress, new List<LogModel>(), (key, value) =>
                    {
                        if (value == null)
                        {
                            value = new List<LogModel>();
                        }

                        if (value.Count > 100)
                        {
                            value.RemoveAt(0);
                        }

                        value.Add(new LogModel(
                            context.Request.Path,
                            context.Request.QueryString.ToString(),
                            context.Request.Headers.ToDictionary(k => k.Key, v => string.Join(", ", v.Value)),
                            context.Request.Cookies.ToDictionary(k => k.Key, v => v.Value)));
                        return value;
                    });

                    await context.Response.WriteAsync("Success!");
                }
            });
        }
    }
}
