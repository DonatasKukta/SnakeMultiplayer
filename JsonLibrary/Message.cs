using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;

namespace JsonLibrary
{
    public class Message
    {
        public string sender;
        public string lobby;
        public string type;
        public object body;

        public Message() { }
        public Message(string sender, string lobby, string type, object body)
        {
            this.sender = sender;
            this.lobby = lobby;
            this.type = type;
            this.body = body;
        }

        public static Message Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<Message>(json);
        }
        public static string Serialize(Message m)
        {
            return JsonConvert.SerializeObject(m);
        }
    }
}
