using MulTUNG.Utils;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using StateKey = System.Collections.Generic.KeyValuePair<int, byte>;

namespace MulTUNG.Packeting.Packets
{
    public class CircuitStatePacket : Packet
    {
        private static BinaryFormatter BinFormatter = new BinaryFormatter();

        public override PacketType Type => PacketType.CircuitState;

        public Dictionary<StateKey, bool> States { get; set; }

        protected override byte[] SerializeInner()
        {
            byte[] data = new PacketBuilder()
                .WriteBinaryObject(States)
                .Done();
            
            return Compressor.Compress(data);
        }

        public static CircuitStatePacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<CircuitStatePacket>();

            byte[] data = reader.ReadRaw(int.MaxValue);
            byte[] decompressed = Compressor.Decompress(data);

            using (var stream = new MemoryStream(decompressed))
            {
                packet.States = (Dictionary<StateKey, bool>)BinFormatter.Deserialize(stream);
            }

            return packet;
        }

        public static CircuitStatePacket Build()
        {
            var states = new Dictionary<StateKey, bool>();

            foreach (var obj in NetObject.Alive)
            {
                byte ioCounter = 0;

                foreach (var io in obj.Value.IO)
                {
                    var output = io.GetComponent<CircuitOutput>();

                    if (output == null)
                        continue;

                    states.Add(new KeyValuePair<int, byte>(obj.Key, ioCounter++), output.On);
                }
            }

            return new CircuitStatePacket
            {
                States = states
            };
        }
    }
}
