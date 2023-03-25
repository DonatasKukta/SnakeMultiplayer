using Newtonsoft.Json;

namespace JsonLibrary.FromClient;

public class Settings
{
    public int cellCount { get; set; }
    public bool? isWall { get; set; }
    public string speed { get; set; }

    public Settings(int c, bool w, string s)
    {
        cellCount = c;
        isWall = w;
        speed = s;
    }

    public static Settings Deserialize(string json) => JsonConvert.DeserializeObject<Settings>(json);
    public static string Serialize(Message m) => JsonConvert.SerializeObject(m);
    public static Settings Deserialize(object jsonObj) => JsonConvert.DeserializeObject<Settings>(jsonObj.ToString());
}