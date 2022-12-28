using System.Collections.Generic;

using Newtonsoft.Json;

namespace JsonLibrary.FromServer;

[JsonObject]
public class Players
{
    public List<Player> players;
    public Players(List<Player> p)
    {
        players = p;
    }

    public static Players Deserialize(string json) => JsonConvert.DeserializeObject<Players>(json);
    public static string Serialize(Players m) => JsonConvert.SerializeObject(m);
}
[JsonObject]
public class Player
{
    public string name;
    public string color;
    public string type;
}