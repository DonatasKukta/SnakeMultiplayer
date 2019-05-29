using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnakeMultiplayer.Services;
using JsonLibrary;
using System.Collections.Concurrent;

namespace SnakeMultiplayer
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        //private ConcurrentDictionary<string, List<WebSocket>> lobbies;

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, [FromServices]GameServerService gameServer, 
            [FromServices] GameServerService webSocketHandler)
        {
            if (httpContext.Request.Path == "/ws")
            {
                if (httpContext.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                    //WebSocketReceiveResult response = await webSocket.ReceiveAsync();
                    //await forward(webSocket, gameServer);
                    //await echo(httpContext, webSocket, service);

                    await webSocketHandler.HandleWebSocketAsync(webSocket, gameServer);
                }
                else
                {
                    httpContext.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(httpContext);
            }
        }
        /*

        private async void ReceiveMessage(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue && result.MessageType == WebSocketMessageType.Text)
            {
                //string converted = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                // Console.WriteLine(converted);
                string converted = Strings.getString(buffer);

                buffer = Strings.getBytes(converted, buffer.Length);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
        
        private async Task forward(WebSocket webSocket, [FromServices]GameServerService service)
        {
            byte[] buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.CloseStatus.HasValue)
            {
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            else if ( result.MessageType != WebSocketMessageType.Text)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Message type must be text.", CancellationToken.None);
            }
            else if (result.Count <= 0)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Message must not be empty.", CancellationToken.None);
            }
            else
            {
                string m = Strings.getString(buffer);
                Message message = Message.Deserialize(m);
                if (message != null)
                    return;
                //service.Forward(webSocket, message);
                else
                    await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Unexpected message error.", CancellationToken.None);
            }
        }

        private async Task echo(HttpContext context, WebSocket webSocket, [FromServices]GameServerService service)
        {
            byte[] buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue && result.MessageType == WebSocketMessageType.Text)
            {
                //string converted = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                // Console.WriteLine(converted);
                string converted = Strings.getString(buffer);

                buffer = Strings.getBytes(converted, buffer.Length);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
        */
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CustomMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketMiddleware>();
        }
    }
}
