using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packeting.Packets
{
    public class UserInputPacket : Packet
    {
        public enum UserInputReceiver
        {
            Button,
            Switch
        }

        public override PacketType Type => PacketType.UserInput;

        public int NetID { get; set; }
        public bool State { get; set; }
        public UserInputReceiver Receiver { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(NetID)
                .Write(State)
                .Write((int)Receiver)
                .Done();
        }

        public static UserInputPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<UserInputPacket>();
            packet.NetID = reader.ReadInt32();
            packet.State = reader.ReadBool();
            packet.Receiver = (UserInputReceiver)reader.ReadInt32();

            return packet;
        }
    }
}
