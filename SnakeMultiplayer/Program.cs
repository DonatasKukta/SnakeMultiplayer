using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SnakeMultiplayer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc(o => o.EnableEndpointRouting = false);

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddSingleton<IGameServerService, GameServerService>();
builder.Services.AddSingleton<ITimerService, TimerService>();
builder.Services.AddSignalR();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseWebSockets();
app.MapHub<LobbyHub>("/LobbyHub");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();