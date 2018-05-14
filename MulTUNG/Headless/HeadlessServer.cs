using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace MulTUNG.Headless
{
    public class HeadlessServer
    {
        public static HeadlessServer Instance { get; } = new HeadlessServer();

        private TcpListener Listener;
        public StreamReader In { get; private set; }
        public StreamWriter Out { get; private set; }

        private HeadlessServer()
        {
        }

        public void Start()
        {
            Console.WriteLine("Starting headless listener...");

            try
            {
                Listener = new TcpListener(IPAddress.Loopback, Constants.HeadlessPort);
                Listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Application.Quit();
            }

            Listener.BeginAcceptTcpClient(AcceptCallback, null);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var client = Listener.EndAcceptTcpClient(ar);
            var stream = client.GetStream();

            In = new StreamReader(stream);
            Out = new StreamWriter(stream);
            Out.AutoFlush = true;

            Application.logMessageReceivedThreaded += Application_logMessageReceived;
            System.Diagnostics.Debug.Listeners.Add(new TextWriterTraceListener(stream));
            Console.SetOut(Out);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                while (true)
                {
                    Console.WriteLine("Hey what's up debug");
                    Thread.Sleep(1000);
                }
            }, null);
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            Out.WriteLine(condition);
        }
    }
}
