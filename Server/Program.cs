using Common;
using Common.Packets;
using System;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace Server
{
    class Program
    {
        static void Main(string[] args) => new Program().Run(args);

        private NetworkServer Server = new NetworkServer();
        private bool Exit = false;

        public void Run(string[] args)
        {
            Server.Start();
            Console.WriteLine($"Listening on port {Constants.Port}. Press ENTER to enter commands");

            Task.Run(() =>
            {
                string ip = new WebClient().DownloadString("http://api.ipify.org/");

                Console.WriteLine("Your public IP address is " + ip);
            });
            
            while (!Exit)
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

            Console.WriteLine("Bye!");
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
                    Time = 10,
                    BoardID = 0xB00B5
                });

                Console.WriteLine("Done!");
            }
            else if (cmd == "board del")
            {
                Console.WriteLine("Broadcasting test delete board packet.");

                Server.Broadcast(new DeleteBoardPacket
                {
                    BoardID = 0xB00B5
                });

                Console.WriteLine("Done!");
            }
            else if (cmd == "quit" || cmd == "exit" || cmd == "stop")
            {
                Exit = true;
            }
            else if (cmd == "update" || cmd == "u")
            {
                
            }
        }
    }
}
