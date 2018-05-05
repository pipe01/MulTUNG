using MulTUNG.Packeting.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MulTUNG.Utils
{
    public class Transfer
    {
        private Stream DataStream;

        public Transfer(Stream data)
        {
            this.DataStream = data;
        }

        public void Send()
        {
            byte[] buffer = new byte[TransferDataPacket.DataBufferSize];
            int count = 0;
            int read = 0;

            while ((read = DataStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                Network.SendPacket(new TransferDataPacket
                {
                    Data = buffer,
                    Index = count++
                });
            }
        }
    }
}
