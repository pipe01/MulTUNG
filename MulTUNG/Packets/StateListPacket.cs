using System.Collections.Generic;
using UnityEngine;

namespace MulTUNG.Packets
{
    public struct PlayerState
    {
        public float Time;
        public int PlayerID;
        public string Username;
        public Vector3 Position, EulerAngles;
    }

    public class StateListPacket : Packet
    {
        public override PacketType Type => PacketType.StateList;
        public override bool ReceiveOwn => true;

        public Dictionary<int, PlayerState> States { get; set; } = new Dictionary<int, PlayerState>();

        protected override byte[] SerializeInner()
        {
            var builder = new PacketBuilder();

            builder.Write(States.Count);

            foreach (var item in States)
            {
                builder.Write(item.Value.PlayerID);
                builder.Write(item.Value.Position);
                builder.Write(item.Value.EulerAngles);
                builder.Write(item.Value.Username);
            }

            return builder.Done();
        }

        public override void Deserialize(IReader reader)
        {
            int statesCount = reader.ReadInt32();

            for (int i = 0; i < statesCount; i++)
            {
                var state = new PlayerState
                {
                    PlayerID = reader.ReadInt32(),
                    Position = reader.ReadVector3(),
                    EulerAngles = reader.ReadVector3(),
                    Username = reader.ReadString(),
                    Time = this.Time
                };

                this.States.Add(state.PlayerID, state);
            }
        }
    }
}
