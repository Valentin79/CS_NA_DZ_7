using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatCommon.Abstraction;
using ChatCommon.Models;

namespace ChatApp
{
    public class UdpMessageSourceClient : IMessageSourseClient<IPEndPoint>
    {
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _udpEndPoint;
        public UdpMessageSourceClient(string ip = "172.0.0.1", int port = 0)
        {
            _udpClient = new UdpClient(12345);
            _udpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public IPEndPoint CreateEndpoint()
        {
            return new IPEndPoint(IPAddress.Any, 0);
        }

        public IPEndPoint GetServer()
        {
            return _udpEndPoint;
        }

        public NetMessage Recive(ref IPEndPoint ep)
        {
            byte[] data = _udpClient.Receive(ref ep);
            string str = Encoding.UTF8.GetString(data);
            return NetMessage.DeserializeFromJson(str) ?? new NetMessage();
        }

        public async Task SendAsync(NetMessage message, IPEndPoint ep)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message.SerialazeMessageToJson());
            await _udpClient.SendAsync(buffer, buffer.Length, ep);
        }
    }
}
