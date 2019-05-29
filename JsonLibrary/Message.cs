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
        public string body;

        public Message() { }
        public Message(string s, string l, string t, string b)
        {
            this.sender = s;
            this.lobby = l;
            this.type = t;
            this.body = b;
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
