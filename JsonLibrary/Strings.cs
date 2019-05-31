using Newtonsoft.Json;
using System;
using System.Text;

namespace JsonLibrary
{
    public class Strings
    {
        public static string getString(byte[] array)
        {
            return Encoding.UTF8.GetString(array, 0, array.Length).Replace("\0", string.Empty);
        }
        public static byte[] getBytes(string text)
        {
            byte[] newBuffer = new byte[text.Length];
            byte[] charsBuffer = Encoding.UTF8.GetBytes(text.ToCharArray());
            Buffer.BlockCopy(charsBuffer, 0, newBuffer, 0, text.Length);
            return newBuffer;
        }
        public static byte[] getBytes(string text, int bufferLength)
        {
            byte[] newBuffer = new byte[bufferLength];
            byte[] charsBuffer = Encoding.UTF8.GetBytes(text.ToCharArray());
            Buffer.BlockCopy(charsBuffer, 0, newBuffer, 0, text.Length);
            return newBuffer;
        }

        public static dynamic getObject(string json)
        {
            return JsonConvert.DeserializeObject(json);
        }
    }
}
