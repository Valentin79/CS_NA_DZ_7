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
    public class Client<T>
    {
        private readonly string _name;
        private readonly IMessageSourseClient<T> _messageSourse;
        //private IPEndPoint remoteEngPoint;
        public T remoteEngPoint;
        public Client(IMessageSourseClient<T> messageSourseClient, string name)
        {
            this._name = name;
            _messageSourse = messageSourseClient;
            //remoteEngPoint = new IPEndPoint(IPAddress.Parse(adress), port);
            remoteEngPoint = _messageSourse.CreateEndpoint();
        }

        UdpClient udpClientClient = new UdpClient();


        async Task ClientListener()
        {
            //IPEndPoint remoteEngPoint = new IPEndPoint(IPAddress.Parse(adress), 12345);
            
            while (true)
            {
                var messageRecived = _messageSourse.Recive(ref remoteEngPoint);
                Console.WriteLine($"Получено сообщение от {messageRecived.NicknameFrom}");
                Console.WriteLine(messageRecived.Text );

                await Confirm(messageRecived, remoteEngPoint);
            }
        }


        async Task ClientSendler()
        {
            Register(remoteEngPoint);

            while (true)
            {
                try
                {
                    Console.WriteLine("Введите имя получателя");
                    var nameTo = Console.ReadLine();
                    Console.WriteLine("Введите сообщение");
                    var messageText = Console.ReadLine();

                    var message = new NetMessage()
                    {
                        command = Command.message,
                        NicknameFrom = _name,
                        NicknameTo = nameTo,
                        Text = messageText,
                    };
                    await _messageSourse.SendAsync(message, remoteEngPoint);
                    Console.WriteLine("Сообщение отправлено");
                }
                catch (Exception ex) { Console.WriteLine(ex); }
            }
        }


        void Register(T remoteEngPoint)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            var message = new NetMessage()
            {
                NicknameFrom = _name,
                NicknameTo = null,
                Text = null,
                command = Command.register,
                EndPoint = ep, 
            };
            _messageSourse.SendAsync(message, remoteEngPoint);
        }

        async Task Confirm(NetMessage message,  T remoteEngPoint)
        {
            message.command = Command.confirm;
            await _messageSourse.SendAsync(message, remoteEngPoint );

        }


        public async Task Start()
        {
            new Thread(async() => await ClientListener()).Start();

            await ClientSendler();

        }
    }
}
