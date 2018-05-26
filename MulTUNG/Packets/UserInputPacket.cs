namespace MulTUNG.Packets
{
    public class UserInputPacket : Packet
    {
        public enum UserInputReceiver
        {
            Button,
            Switch
        }

        public override PacketType Type => PacketType.UserInput;
        public override bool ShouldBroadcast => true;

        public int NetID { get; set; }
        public UserInputReceiver Receiver { get; set; }
        public bool State { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(NetID)
                .Write((int)Receiver)
                .Write(State)
                .Done();
        }

        public static UserInputPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<UserInputPacket>();
            packet.NetID = reader.ReadInt32();
            packet.Receiver = (UserInputReceiver)reader.ReadInt32();
            packet.State = reader.ReadBool();

            return packet;
        }
    }
}
