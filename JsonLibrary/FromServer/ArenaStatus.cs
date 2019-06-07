using Newtonsoft.Json;
using System.Collections.Generic;

namespace JsonLibrary
{
    [JsonObject]
    public class ArenaStatus
    {
        public List<Snake> ActiveSnakes;
        public List<string> DisabledSnakes;
        public XY food;

        public ArenaStatus(XY food)
        {
            this.food = food;
            ActiveSnakes = new List<Snake>();
            DisabledSnakes =  new List<string>();
        }
        public void  AddActiveSnake(Snake s)
        {
            ActiveSnakes.Add(s);
        }

        public void AddDisabledSnake(string player)
        {
            DisabledSnakes.Add(player);
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
    public class Snake
    {
        public string player;
        public string color;
        public XY head;
        public XY tail;
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
        public int x;
        public int y;
        public XY(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
