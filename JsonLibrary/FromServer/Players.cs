using Newtonsoft.Json;
using System.Collections.Generic;

namespace JsonLibrary
{
    [JsonObject]
    public class Players
    {
        public List<Player> players;
        public Players(List<Player> p)
        {
            this.players = p;
        }

        public static Players Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<Players>(json);
        }
        public static string Serialize(Players m)
        {
            return JsonConvert.SerializeObject(m);
        }
    }
    [JsonObject]
    public class Player
    {
        public string name;
        public string color;
        public string type;
    }

}
