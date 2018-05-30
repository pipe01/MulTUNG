using MulTUNG.Utils;
using System.Collections.Generic;

namespace MulTUNG.Packets
{
    public class PlayerWelcomePacket : Packet
    {
        public override PacketType Type => PacketType.PlayerWelcome;

        public int YourID { get; set; }
        public string ServerUsername { get; set; }
        public List<Tuple<int, string>> Players { get; set; }

        protected override byte[] SerializeInner()
        {
            var builder = new PacketBuilder()
                .Write(YourID)
                .Write(ServerUsername);

            builder.Write(Players.Count);
            foreach (var item in Players)
            {
                builder.Write(item.Item1);
                builder.Write(item.Item2);
            }

            return builder.Done();
        }

        public override void Deserialize(IReader reader)
        {
            this.YourID = reader.ReadInt32();
            this.ServerUsername = reader.ReadString();
            this.Players = new List<Tuple<int, string>>();

            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                this.Players.Add(new Tuple<int, string>(reader.ReadInt32(), reader.ReadString()));
            }
        }
    }
}
