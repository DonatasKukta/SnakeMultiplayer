using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SnakeMultiplayer.Services;

namespace SnakeMultiplayer.Middlewares;

public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;

    public WebSocketMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, [FromServices] IWebSocketHandler webSocketHandler)
    {
        if (httpContext.Request.Path == "/ws")
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                await webSocketHandler.HandleWebSocketAsync(webSocket);
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