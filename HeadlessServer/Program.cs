using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeadlessServer
{
    class Program
    {
        public const string GameExecutable = "./The Ultimate Nerd Game.exe";

        static void Main(string[] args) => new Program().Run();

        private HeadlessClient Client = new HeadlessClient();

        public void Run()
        {
            LaunchGame();
        }

        void LaunchGame()
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = GameExecutable,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = "-batchmode -nographics -server"
            };

            Process p = new Process
            {
                StartInfo = psi
            };
            p.Start();

            while (!Client.TryConnect())
            {
                Console.WriteLine("Connecting...");
                Thread.Sleep(1000);
            }

            Console.WriteLine("Connected!");

            Console.ReadKey();

            p.Kill();
        }
    }
}
