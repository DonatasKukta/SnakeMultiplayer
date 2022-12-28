using Newtonsoft.Json;

namespace JsonLibrary;

public class Message
{
    public string sender;
    public string lobby;
    public string type;
    public dynamic body;

    public Message() { }
    public Message(string sender, string lobby, string type, dynamic body)
    {
        this.sender = sender;
        this.lobby = lobby;
        this.type = type;
        this.body = body;
    }

    public static Message Deserialize(string json) => JsonConvert.DeserializeObject<Message>(json);
    public static string Serialize(Message m) => JsonConvert.SerializeObject(m);
}