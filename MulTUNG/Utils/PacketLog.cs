using MulTUNG.Packeting.Packets;
using PiTung.Console;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MulTUNG.Utils
{
    public static class PacketLog
    {
        private static StreamWriter LogFile;

        private static string LogPath => "./" + (Network.IsClient ? "client" : Network.IsServer ? "server" : "unknown") + ".txt";

        private static string Side => Network.IsClient ? "CLIENT" : Network.IsServer ? "SERVER" : "UNKNOWN";

        private static Packet LastSent, LastReceived;

        private static void OpenFile()
        {
            if (LogFile == null)
            {
                if (File.Exists(LogPath))
                {
                    if (File.Exists(LogPath + ".old"))
                        File.Delete(LogPath + ".old");

                    File.Move(LogPath, LogPath + ".old");
                }

                LogFile = File.CreateText(LogPath);
                LogFile.AutoFlush = true;
            }
        }

        public static void LogSend(Packet packet) => Log(packet, true);
        public static void LogReceive(Packet packet) => Log(packet, false);

        [Conditional("DEBUG")]
        private static void Log(Packet packet, bool sending)
        {
            try
            {
                OpenFile();
            }
            catch { }
            
            if (packet == null || LogFile == null)
                return;

            //if ((sending ? LastSent : LastReceived)?.GetType() == packet.GetType())
            //{
            //    PrintedDot = true;
            //    LogFile.Write(".");

            //    return;
            //}
            //else if (PrintedDot)
            //{
            //    PrintedDot = false;
            //    LogFile.WriteLine();
            //}
            
            if (sending)
                LastSent = packet;
            else
                LastReceived = packet;

            StringBuilder builder = new StringBuilder();
            builder.Append($"[{DateTime.Now.ToString("hh:mm:ss.ffff")}] [{Side}] [{(sending ? "OUT" : "IN")}] {packet.SenderID}: {packet.GetType().Name}");

            string packetStr = packet.ToString();

            if (!packetStr.Equals(packet.GetType().Name))
                builder.Append($" [{packetStr}]");
            
            lock (LogFile)
            {
                LogFile.WriteLine(builder.ToString());
            }
        }
    }
}
