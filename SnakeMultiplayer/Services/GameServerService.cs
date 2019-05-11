using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace SnakeMultiplayer.Services
{
    public class GameServerService : IHostedService
    {
        LinkedList<Message> sharedResource = new LinkedList<Message>();
        Dictionary<string, LobbyService> lobbies = new Dictionary<string, LobbyService>();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public bool createLobby()
        {
            return false; // LobbyService lobby = new LobbyService(nameof, hostName)
        }
        public async void forward(string webScoketRequest)
        {
            RequestBody obj = JsonConvert.DeserializeObject<RequestBody>(webScoketRequest);
            string lobby = obj.lobbyName;
            string message = obj.body;
            //...
        }
        
        public Task AddMessage(string user, string message)
        {
            sharedResource.AddLast(new Message(user, message));
            return Task.CompletedTask;
        }
        public Task AddMessage(string message)
        {
            sharedResource.AddLast(new Message(string.Empty, message));
            return Task.CompletedTask;
        }

        public string getAllMessages()
        {
            return getresourcestring();
        }
        private string getresourcestring()
        {
            string result = "";
            for (LinkedListNode<Message> i = sharedResource.First; i != null; i = i.Next)
            {
                result += i.Value.message + "\n";
            }
            return result;
        }

        class RequestBody
        {
            public string lobbyName;
            public string body;
        }
    }

    class Message
    {
        public string user { get; set; }
        public string message { get; set; }

        public Message(string u, string m)
        {
            user = u;
            message = m;
        }
    }

}
