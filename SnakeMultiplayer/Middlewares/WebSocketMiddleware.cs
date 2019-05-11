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


namespace SnakeMultiplayer
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, [FromServices]GameServerService service)
        {
            if (httpContext.Request.Path == "/ws")
            {
                if (httpContext.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                    await Echo(httpContext, webSocket, service);
                    
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


        private async Task Echo(HttpContext context, WebSocket webSocket, [FromServices]GameServerService service)
        {
            byte[] buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue && result.MessageType == WebSocketMessageType.Text)
            {
                //string converted = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                // Console.WriteLine(converted);
                string converted = getString(buffer);

                buffer = getBytes(converted, buffer.Length);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        static string getString(byte[] array)
        {
            return Encoding.UTF8.GetString(array, 0, array.Length).Replace("\0", string.Empty);
        }

        static byte[] getBytes(string text, int bufferLength)
        {
            byte[] newBuffer = new byte[bufferLength];
            byte[] charsBuffer = Encoding.UTF8.GetBytes(text.ToCharArray());
            Buffer.BlockCopy(charsBuffer, 0, newBuffer, 0, text.Length);
            return newBuffer;
        }
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
