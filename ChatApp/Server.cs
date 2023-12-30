
using System.Net;
using System.Net.Sockets;
using ChatCommon.Abstraction;
using ChatCommon.Models;
using ChatDB;


namespace ChatApp
{
    public class Server<T>
    {
        Dictionary<string, T> clients = new Dictionary<string, T>();
        private T iPEndPoint;
        private readonly IMessageSourseServer<T>  _messageSource;
        public Server(IMessageSourseServer<T> messageSourse)
        {
            _messageSource = messageSourse;
            iPEndPoint = _messageSource.CreateEndpoint();
        }

        bool work = true;
        public void Stop()
        {
            work = false;
        }

        private async Task Register(NetMessage message)
        {
            Console.WriteLine($"Message Register name = {message.NicknameFrom}");

            if(clients.TryAdd(message.NicknameFrom, _messageSource.CopyEndpoint(message.EndPoint))) 
            { 
                using(ChatContext context = new ChatContext())
                {
                    context.Users.Add(new User
                    {
                        Fullname = message.NicknameFrom
                    });
                    await context.SaveChangesAsync();
                }
            }
        }

        private async Task RelyMessage(NetMessage message)
        {
            int id = 0;
            if (clients.TryGetValue(message.NicknameTo, out T ep)) // if (clients.TryGetValue(message.NicknameTo, out IPEndPoint ep))
            {
                using (var ctx = new ChatContext())
                {
                    var fromUser = ctx.Users.First(x => x.Fullname == message.NicknameFrom);
                    var toUser = ctx.Users.First(x => x.Fullname == message.NicknameTo);
                    var msg = new Message { UserFrom = fromUser, UserTo = toUser, IsSend = false, Text = message.Text };
                    ctx.Messages.Add(msg);

                    ctx.SaveChanges();

                    id = msg.MessageId;
                }

                message.Id = id;
                await _messageSource.SendAsync(message, ep);
                //var forwardMessageJson = message.SerialazeMessageToJson();
                Console.WriteLine($"Message Relied, from = {message.NicknameFrom} to = {message.NicknameTo}");
            }
            else
            {
                Console.WriteLine("Пользователь не найден.");
            }
        }


        async Task ConfirmMessageReceived(int? id)
        {
            Console.WriteLine("Message confirmation id=" + id);

            using (var ctx = new ChatContext())
            {
                var msg = ctx.Messages.FirstOrDefault(x => x.MessageId == id);
                //var msg = ctx.Messages.FirstOrDefault(x => x.id == id);

                if (msg != null)
                {
                    msg.IsSend = true;
                    await ctx.SaveChangesAsync();
                }
            }
        }

        async Task ProcessMessage(NetMessage message)
        {
            switch (message.command)
            {
                case Command.register:
                    await Register(message);
                    break;
                case Command.message:
                    await RelyMessage(message);
                    break;
                case Command.confirm:
                    await ConfirmMessageReceived(message.Id);
                    break;
            }
        }

        public async Task Start()
        {
            //IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //UdpClient udpClient = new UdpClient();

            Console.WriteLine("Сервер ожидает собщение");

            while (work)
            {
                try 
                {
                    var message = _messageSource.Recive(ref iPEndPoint);
                    Console.WriteLine(message.ToString);
                    await ProcessMessage(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        //==========================================================================

        private async Task SendListMessages(string user, T ep)
        {

            using (var ctx = new ChatContext())
            {
                var messages = ctx.Messages.Where(x => x.UserTo.Equals(user) && x.IsSend == false).ToList();
                 
                foreach (var message in messages)
                {
                    NetMessage netMessage = new NetMessage()
                    {
                        Text = message.Text,
                        DateTime = message.DateSend,
                        NicknameFrom = message.UserFrom.ToString(),  
                        NicknameTo = message.UserTo.ToString(),
                    };

                    await _messageSource.SendAsync(netMessage, ep);
                }
            }
            
        }
    }
}
