using System.Collections.Generic;

using Newtonsoft.Json;

namespace JsonLibrary.FromServer;

[JsonObject]
public class ArenaStatus
{
    public List<Snake> ActiveSnakes { get; set; }
    public List<string> DisabledSnakes { get; set; }
    public XY food { get; set; }

    public ArenaStatus(XY food)
    {
        this.food = food;
        ActiveSnakes = new List<Snake>();
        DisabledSnakes = new List<string>();
    }
    public void AddActiveSnake(Snake s) => ActiveSnakes.Add(s);

    public void AddDisabledSnake(string player) => DisabledSnakes.Add(player);

    public static Players Deserialize(string json) => JsonConvert.DeserializeObject<Players>(json);
    public static string Serialize(Players m) => JsonConvert.SerializeObject(m);
}

[JsonObject]
public class Snake
{
    public string player { get; set; }
    public string color { get; set; }
    public XY head { get; set; }
    public XY tail { get; set; }
    public Snake(string player, string color, XY head, XY tail)
    {
        this.player = player;
        this.color = color;
        this.head = head;
        this.tail = tail;
    }
}
[JsonObject]
public class XY
{
    public int x { get; set; }
    public int y { get; set; }
    public XY(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}