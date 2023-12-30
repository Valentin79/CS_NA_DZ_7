using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatCommon.Models
{
    public enum Command
    {
        register,
        message,
        confirm
    }

    public class NetMessage
    {

        public int Id { get; set; }
        public Command command { get; set; }
        public string Text { get; set; }
        public DateTime DateTime { get; set; }
        public string NicknameFrom { get; set; }
        public string NicknameTo { get; set; }

        public IPEndPoint EndPoint { get; set; }

        /*public string SerializeMessageToJson()
        {
            return JsonSerializer.Serialize(this);
        }*/
        // Метод сверху можно сделать так же. => - вместо ретурн.
        public string SerialazeMessageToJson() => JsonSerializer.Serialize(this);
        public static NetMessage? DeserializeFromJson(string message) => JsonSerializer.Deserialize<NetMessage>(message);

        public void Print()
        {
            Console.WriteLine(ToString());
        }

        public override string ToString()
        {
            return $"{DateTime} получено сообщение {Text} от {NicknameFrom}";
        }
    }
}
