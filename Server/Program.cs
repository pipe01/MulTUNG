using Common.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Server
{
    class Program
    {
        static void Main(string[] args) => new Program().Run(args);

        private NetworkServer Server = new NetworkServer();

        public void Run(string[] args)
        {
            Server.Start();
            Console.WriteLine("Listening. Press the ENTER key to enter commands");
            
            while (true)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Log.WriteToConsole = false;

                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("> ");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    string line = Console.ReadLine();
                    ParseCommand(line);

                    Log.WriteToConsole = true;
                    Log.FlushQueue();
                }
            }
        }

        private void ParseCommand(string cmd)
        {
            if (cmd == "board")
            {
                Console.WriteLine("Broadcasting test board packet.");

                Server.Broadcast(new PlaceBoardPacket
                {
                    Width = 5,
                    Height = 5,
                    Position = new Vector3(0, 0.5f, 0),
                    SenderID = -1,
                    Time = 10
                });

                Console.WriteLine("Done!");
            }
        }
    }
}
