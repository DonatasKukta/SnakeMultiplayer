using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonLibrary
{
    public class Settings
    {
        public int cellCount;
        public bool isWall;

        public static Message Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<Message>(json);
        }
        public static string Serialize(Message m)
        {
            return JsonConvert.SerializeObject(m);
        }

        public static Settings Deserialize(object jsonObj)
        {
            return JsonConvert.DeserializeObject<Settings>(jsonObj.ToString());
        }

    }
}
