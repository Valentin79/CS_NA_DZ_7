using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using ChatCommon.Models;


namespace ChatCommon.Abstraction
{
    public interface IMessageSourseServer<T>
    {
        Task SendAsync(NetMessage message, T ep);

        NetMessage Recive(ref T ep);

        T CreateEndpoint();

        T CopyEndpoint(IPEndPoint ep);

    }
}
