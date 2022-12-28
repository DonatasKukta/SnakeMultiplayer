using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SnakeMultiplayer.Middlewares;
using SnakeMultiplayer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc(o => o.EnableEndpointRouting = false);

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddSingleton<GameServerService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Error");
    _ = app.UseHsts();
}

app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMvc(routes =>
{
    _ = routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();