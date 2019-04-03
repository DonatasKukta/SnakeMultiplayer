using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace SnakeMultiplayer.Services
{
    public class GameServerService : IHostedService
    {
        LinkedList<Message> sharedResource = new LinkedList<Message>();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        public Task AddMessage(string user, string message)
        {
            sharedResource.AddLast(new Message(user, message));
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
