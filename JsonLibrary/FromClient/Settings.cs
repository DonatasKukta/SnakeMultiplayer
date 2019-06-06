using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonLibrary
{
    public class Settings
    {
        public int cellCount;
        public bool? isWall;
        public string speed;

        public Settings(int c, bool w, string s)
        {
            cellCount = c;
            isWall = w;
            speed = s;
        }

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
