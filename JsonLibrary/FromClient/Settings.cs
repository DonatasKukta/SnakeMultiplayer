using Newtonsoft.Json;

namespace JsonLibrary.FromClient;

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

    public static Message Deserialize(string json) => JsonConvert.DeserializeObject<Message>(json);
    public static string Serialize(Message m) => JsonConvert.SerializeObject(m);
    public static Settings Deserialize(object jsonObj) => JsonConvert.DeserializeObject<Settings>(jsonObj.ToString());
}