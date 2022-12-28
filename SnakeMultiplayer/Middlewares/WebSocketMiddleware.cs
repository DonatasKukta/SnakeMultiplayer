using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SnakeMultiplayer.Services;

namespace SnakeMultiplayer.Middlewares;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;

    public WebSocketMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Each time an application receives new request, this method checks wether or not its web socket
    /// request. If it is, then the request is being handled accordingly.
    /// </summary>
    public async Task InvokeAsync(HttpContext httpContext, [FromServices] GameServerService gameServer)
    {
        if (httpContext.Request.Path == "/ws")
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                // Pass new web socket to be handled.
                await gameServer.HandleWebSocketAsync(webSocket);
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
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class CustomMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<WebSocketMiddleware>();
}