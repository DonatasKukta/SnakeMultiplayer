using Microsoft.AspNetCore.SignalR;

using System.Threading.Tasks;

namespace SnakeMultiplayer.Services
{
    public class LobbyHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
