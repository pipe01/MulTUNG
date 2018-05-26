using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packeting.Packets
{
    public class ChatMessagePacket : Packet
    {
        public override PacketType Type => PacketType.ChatMessage;
        public override bool ShouldBroadcast => true;
        public override bool ReceiveOwn => true;

        public string Username { get; set; }
        public string Text { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(Username)
                .Write(Text)
                .Done();
        }

        public static ChatMessagePacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<ChatMessagePacket>();
            packet.Username = reader.ReadString();
            packet.Text = reader.ReadString();

            return packet;
        }
    }
}
