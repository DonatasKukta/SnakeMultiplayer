using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonLibrary
{
    [JsonObject]
    public class ArenaStatus
    {
        public List<Snake> snakes;
        public XY food;

        public ArenaStatus(XY food)
        {
            this.food = food;
            snakes = new List<Snake>();
        }
        public void  AddSnake(Snake s)
        {
            snakes.Add(s);
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
        public List<XY> body;
        public string player;
        public Snake(string player)
        {
            this.player = player;
            body = new List<XY>();
        }

        public void AddCoord(XY c)
        {
            body.Add(c);
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
