using MulTUNG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeadlessServer
{
    public class HeadlessClient
    {
        private TcpClient Client;

        public StreamReader In { get; private set; }
        public StreamWriter Out { get; private set; }

        public bool TryConnect()
        {
            Client = new TcpClient();

            try
            {
                Client.Connect(new IPEndPoint(IPAddress.Loopback, Constants.HeadlessPort));
            }
            catch
            {
                return false;
            }

            if (Client.Connected)
            {
                var stream = Client.GetStream();

                In = new StreamReader(stream);
                Out = new StreamWriter(stream);

                Console.Clear();
                ReadIn();
            }

            return Client.Connected;
        }

        private void ReadIn()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                while (!In.EndOfStream)
                {
                    Console.WriteLine(In.ReadLine());
                }
            });
        }
    }
}
