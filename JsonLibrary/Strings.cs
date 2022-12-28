using System;
using System.Text;

using Newtonsoft.Json;

namespace JsonLibrary;

public class Strings
{
    public static string getString(byte[] array) => Encoding.UTF8.GetString(array, 0, array.Length).Replace("\0", string.Empty);
    public static byte[] getBytes(string text)
    {
        var newBuffer = new byte[text.Length];
        var charsBuffer = Encoding.UTF8.GetBytes(text.ToCharArray());
        Buffer.BlockCopy(charsBuffer, 0, newBuffer, 0, text.Length);
        return newBuffer;
    }
    public static byte[] getBytes(string text, int bufferLength)
    {
        var newBuffer = new byte[bufferLength];
        var charsBuffer = Encoding.UTF8.GetBytes(text.ToCharArray());
        Buffer.BlockCopy(charsBuffer, 0, newBuffer, 0, text.Length);
        return newBuffer;
    }

    public static dynamic getObject(string json) => JsonConvert.DeserializeObject(json);
}