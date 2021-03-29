using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LogApi
{
    public class Startup
    {
        private readonly ConcurrentDictionary<string, LogModel> _clientLogs;
        private readonly IList<WebSocket> _webSockets;

        public Startup()
        {
            _clientLogs = new ConcurrentDictionary<string, LogModel>();
            _webSockets = new List<WebSocket>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseStaticFiles();
            app.UseWebSockets();

            app.Run(async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        _webSockets.Add(webSocket);
                        CancellationTokenSource cts = new(TimeSpan.FromMinutes(10));

                        byte[] message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_clientLogs));

                        await webSocket.SendAsync(
                            new ArraySegment<byte>(message, 0, message.Length),
                            WebSocketMessageType.Text,
                            endOfMessage: true,
                            cts.Token);

                        try
                        {
                            while (!cts.IsCancellationRequested)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1), cts.Token);
                            }
                        }
                        catch { }
                        finally
                        {
                            _webSockets.Remove(webSocket);
                        }
                    }
                }
                else
                {
                    if (!context.Request.Path.HasValue && context.Request.Path.Value.Contains("favicon"))
                    {
                        var logModel = new LogModel(
                                context.Request.Path,
                                context.Request.QueryString.ToString(),
                                context.Request.Headers.ToDictionary(k => k.Key, v => string.Join(", ", v.Value)),
                                context.Request.Cookies.ToDictionary(k => k.Key, v => v.Value));

                        _clientLogs.AddOrUpdate(Guid.NewGuid().ToString(), logModel, (key, value) => logModel);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () =>
                        {
                            foreach (var webSocket in _webSockets)
                            {
                                if (webSocket.State == WebSocketState.Open)
                                {
                                    byte[] message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(logModel));

                                    await webSocket.SendAsync(
                                        new ArraySegment<byte>(message, 0, message.Length),
                                        WebSocketMessageType.Text,
                                        endOfMessage: true,
                                        CancellationToken.None);
                                }
                            }
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        await context.Response.WriteAsync("Success!");
                    }
                }
            });
        }
    }
}
