using MulTUNG.Packeting.Packets;
using MulTUNG.Packeting.Packets.Utils;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MulTUNG.Utils
{
    public static class Transfer
    {
        public static bool IsSending { get; private set; } = false;
        public static bool IsReceiving { get; private set; } = false;

        public static AutoResetEvent ContinueEvent { get; private set; } = new AutoResetEvent(false);

        private static AutoResetEvent ReceiveEvent = new AutoResetEvent(false);

        private static IList<TransferDataPacket> ReceivedPackets = new List<TransferDataPacket>();

        public static void Send(Stream dataStream, ISender sender)
        {
            if (IsReceiving || IsSending)
                throw new Exception("Can't send more than once at the same time!");

            byte[] buffer = new byte[TransferDataPacket.DataBufferSize];
            int read = 0;
            int totalRead = 0;
            int i = 0;

            IsSending = true;

            sender.Send(new SignalPacket(SignalData.BeginTransfer));
            ContinueEvent.WaitOne();

            while ((read = dataStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                totalRead += read;

                IGConsole.Log("Send " + i);
                sender.Send(new TransferDataPacket
                {
                    Data = buffer,
                    Length = read,
                    Index = i++
                });

                ContinueEvent.WaitOne();
            }
            
            sender.Send(new SignalPacket(SignalData.EndTransfer));

            IsSending = false;
        }

        public static byte[] ReceiveBytes()
        {
            ReceiveEvent.WaitOne();

            using (MemoryStream mem = new MemoryStream())
            {
                EndReceive(mem);

                return mem.ToArray();
            }
        }

        public static void BeginReceive()
        {
            if (IsReceiving || IsSending)
                throw new Exception("Can't transfer more than once at the same time!");

            IsReceiving = true;
            ReceivedPackets.Clear();

            Network.SendPacket(new SignalPacket(SignalData.AckTransfer));
        }

        public static void EndReceive(Stream outputStream)
        {
            if (outputStream != null)
            {
                foreach (var item in ReceivedPackets.OrderBy(o => o.Index))
                {
                    outputStream.Write(item.Data, 0, item.Length);
                }
            }
            
            ReceiveEvent.Set();

            IsReceiving = false;
            ReceivedPackets.Clear();
        }

        public static void ReceivePacket(TransferDataPacket packet)
        {
            ReceivedPackets.Add(packet);

            Network.SendPacket(new SignalPacket(SignalData.AckTransfer));
        }
    }
}
