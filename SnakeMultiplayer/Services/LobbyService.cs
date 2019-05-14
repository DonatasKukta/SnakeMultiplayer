using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SnakeMultiplayer.Services
{
    public class LobbyService
    {
        public string ID {  get; private set; }
        private LobbyState state;
        Dictionary<string, WebSocket> players = new Dictionary<string, WebSocket>();
        private string HostPlayer;
        public LobbyService(string id, string host, WebSocket socket)
        {
            this.ID = id;
            this.HostPlayer = host;
            players.Add(host, socket);
        }

        public void receiveSocketMessage(string player ,string message)
        {

        }

        public void sendSocketMessage(string player, string message)
        {
            
        }
    }

    enum LobbyState
    {
        Idle,
        inGame,
        closed,
    }
}
