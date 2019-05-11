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
        private LobbyStates state;
        Dictionary<string, WebSocket> players = new Dictionary<string, WebSocket>();
        private string HostPlayer;
        public LobbyService(string id, string host, WebSocket socket)
        {
            this.ID = id;
            this.HostPlayer = host;
            players.Add(host, socket);
        }
    }

    enum LobbyStates
    {
        Idle,
        inGame,
        closed,
    }
}
