using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        private readonly IConfigurationRoot _configuration;
        private readonly int _socketAliveMinutes;

        public Startup(IConfiguration configuration)
        {
            _clientLogs = new ConcurrentDictionary<string, LogModel>();
            _webSockets = new List<WebSocket>();

            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            _socketAliveMinutes = int.Parse(_configuration["SocketAliveMinutes"]);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app)
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
                        CancellationTokenSource cts = new(TimeSpan.FromMinutes(_socketAliveMinutes));

                        byte[] message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_clientLogs.Values));
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(message, 0, message.Length),
                            WebSocketMessageType.Text,
                            endOfMessage: true,
                            cts.Token);

                        try
                        {
                            while (!cts.IsCancellationRequested)
                            {
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
                    if (context.Request.Path.HasValue && context.Request.Path.Value.Contains("favicon"))
                    {
                        await context.Response.WriteAsync(string.Empty);
                    }
                    else
                    {
                        string body = string.Empty;
                        using (StreamReader stream = new(context.Request.Body))
                        {
                            body = await stream.ReadToEndAsync();
                        }

                        var logModel = new LogModel(
                                context.Request.Path,
                                body,
                                context.Request.Query.ToDictionary(k => k.Key, v => string.Join(",", v.Value)),
                                context.Request.Headers.ToDictionary(k => k.Key, v => string.Join(", ", v.Value)),
                                context.Request.Cookies.ToDictionary(k => k.Key, v => v.Value));

                        _clientLogs.AddOrUpdate(Guid.NewGuid().ToString(), logModel, (key, value) => logModel);

#pragma warning disable CS4014
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
#pragma warning restore CS4014

                        await context.Response.WriteAsync("Success!");
                    }
                }
            });
        }
    }
}
