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
    public class Transfer
    {
        private Stream DataStream;
        private ISender Sender;

        public Transfer(Stream data, ISender sender)
        {
            this.DataStream = data;
            this.Sender = sender;
        }

        public void Send()
        {
            IGConsole.Log("----SENDING");

            byte[] buffer = new byte[TransferDataPacket.DataBufferSize];
            int count = 0;
            int read = 0;
            int totalRead = 0;
            //long length = DataStream.Length;
            
            Sender.Send(new SignalPacket(SignalData.BeginTransfer));
            WaitAck();

            while ((read = DataStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                IGConsole.Log("Send " + count);

                totalRead += read;

                Sender.Send(new TransferDataPacket
                {
                    Data = buffer,
                    Index = count++
                });

                WaitAck();

                IGConsole.Log($"{totalRead}");///{length} ({totalRead / (float)length:0.0}%)");
            }

            Thread.Sleep(100);

            Sender.Send(new SignalPacket(SignalData.EndTransfer));

            IGConsole.Log("End transfer");
            
            void WaitAck() => Network.WaitForPacket(o => o is SignalPacket signal && signal.Data == SignalData.AckTransfer);
        }

        public static void Receive(Stream outputStream)
        {
            Network.WaitForPacket(o => o is SignalPacket signal && signal.Data == SignalData.BeginTransfer);
            Network.SendPacket(new SignalPacket(SignalData.AckTransfer));

            while (true)
            {
                var packet = Network.WaitForPacket(o => (o is SignalPacket signal && signal.Data == SignalData.EndTransfer) || o is TransferDataPacket);

                if (packet is TransferDataPacket data)
                {
                    outputStream.Write(data.Data, 0, data.Data.Length);
                    IGConsole.Log("Transfer index: " + data.Index);

                    Network.SendPacket(new SignalPacket(SignalData.AckTransfer));
                }
                else
                {
                    break;
                }
            }
        }
    }
}
