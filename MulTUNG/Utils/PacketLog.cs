﻿using MulTUNG.Packets;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MulTUNG.Utils
{
    public static class PacketLog
    {
        private static Stream LogFile;
        private static BinaryFormatter BinFormatter = new BinaryFormatter();

        private static string Random = "";
        private static string LogPath => "./logs/" + (Network.IsClient ? "client" : Network.IsServer ? "server" : "unknown") + Random + ".log";
        
        private static void OpenFile(bool @catch = true)
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");

            if (LogFile == null)
            {
                if (File.Exists(LogPath))
                {
                    if (File.Exists(LogPath + ".old"))
                        File.Delete(LogPath + ".old");

                    try
                    {
                        File.Move(LogPath, LogPath + ".old");
                    }
                    catch (IOException) when (@catch)
                    {
                        Random = UnityEngine.Random.Range(1, 99).ToString();
                        OpenFile(false);
                    }
                }

                LogFile = File.Open(LogPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            }
        }

        public static void LogSend(Packet packet) => Log(packet, true);
        public static void LogReceive(Packet packet) => Log(packet, false);

        [Conditional("DEBUG")]
        private static void Log(Packet packet, bool sending)
        {
            lock (BinFormatter)
            {
                try
                {
                    OpenFile();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Couldn't open packet log file. " + ex);
                    return;
                }

                if (packet == null || LogFile == null)
                    return;

                try
                {
                    BinFormatter.Serialize(LogFile, new PacketLogEntry(packet, sending));
                }
                catch { }
            }
        }
    }
    
    [Serializable]
    public struct PacketLogEntry : ISerializable
    {
        public float UnityTime { get; }
        public Packet Packet { get; }

        public bool In { get; }
        public bool Out => !In;

        public bool Client { get; }
        public bool Server => !Client;

        public PacketLogEntry(Packet packet, bool sending)
        {
            this.Packet = packet;
            this.UnityTime = Time.time;

            this.In = !sending;

            this.Client = Network.IsClient;
        }

        public PacketLogEntry(SerializationInfo info, StreamingContext context)
        {
            this.UnityTime = info.GetSingle("UnityTime");
            this.In = info.GetBoolean("In");
            this.Client = info.GetBoolean("Client");

            byte[] packetData = info.GetValue("Packet", typeof(byte[])) as byte[];
            this.Packet = PacketDeserializer.DeserializePacket(new PacketReader(packetData));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("UnityTime", UnityTime);
            info.AddValue("In", In);
            info.AddValue("Client", Client);

            byte[] packetData = Packet.Serialize();
            info.AddValue("Packet", packetData);
        }
    }
}
