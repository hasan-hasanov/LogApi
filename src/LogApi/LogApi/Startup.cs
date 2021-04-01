using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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
        private readonly int _socketAliveMinutes;

        public Startup(IConfiguration configuration)
        {
            _socketAliveMinutes = int.Parse(configuration["SocketAliveMinutes"]);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHostedService<RequestCleanupJob>();
            services.AddSingleton<RequestContainer>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseStaticFiles();
            app.UseWebSockets();

            app.Run(async context =>
            {
                var requestContainer = context.RequestServices.GetService<RequestContainer>();

                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        requestContainer.WebSockets.Add(webSocket);
                        CancellationTokenSource cts = new(TimeSpan.FromMinutes(_socketAliveMinutes));

                        byte[] message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestContainer.ClientLogs.Values));
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(message, 0, message.Length),
                            WebSocketMessageType.Text,
                            endOfMessage: true,
                            cts.Token);

                        try
                        {
                            while (!cts.IsCancellationRequested) { }
                        }
                        catch { }
                        finally
                        {
                            requestContainer.WebSockets.Remove(webSocket);
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

                        requestContainer.ClientLogs.AddOrUpdate(Guid.NewGuid().ToString(), logModel, (key, value) => logModel);

#pragma warning disable CS4014
                        Task.Run(async () =>
                        {
                            foreach (var webSocket in requestContainer.WebSockets)
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
