using MulTUNG.Utils;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MulTUNG.Packeting.Packets
{
    public class PlaceBoardPacket : Packet
    {
        private static BinaryFormatter BinFormatter = new BinaryFormatter();

        public override PacketType Type => PacketType.PlaceBoard;
        public override bool ShouldBroadcast => true;
        
        public int ParentBoardID { get; set; }
        public int AuthorID { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 EulerAngles { get; set; }
        public byte[] SavedBoard { get; set; } = new byte[0];

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(ParentBoardID)
                .Write(AuthorID)
                .Write(Position)
                .Write(EulerAngles)
                .Write(Compressor.Compress(SavedBoard))
                .Done();
        }

        public static PlaceBoardPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<PlaceBoardPacket>();
            packet.ParentBoardID = reader.ReadInt32();
            packet.AuthorID = reader.ReadInt32();
            packet.Position = reader.ReadVector3();
            packet.EulerAngles = reader.ReadVector3();
            packet.SavedBoard = Compressor.Decompress(reader.ReadByteArray());
            
            return packet;
        }

        public static PlaceBoardPacket BuildFromBoard(CircuitBoard board, Transform parent)
        {
            var netObj = board.GetComponent<NetObject>();
            
            if (netObj == null)
            {
                netObj = board.gameObject.AddComponent<NetObject>();
                netObj.NetID = NetObject.GetNewID();
            }

            var packet = new PlaceBoardPacket
            {
                AuthorID = NetworkClient.Instance.PlayerID,
                ParentBoardID = parent?.GetComponent<NetObject>()?.NetID ?? 0,
                Position = board.transform.position,
                EulerAngles = board.transform.eulerAngles
            };
            var savedObj = SavedObjectUtilities.CreateSavedObjectFrom(board.gameObject);

            using (var mem = new MemoryStream())
            {
                BinFormatter.Serialize(mem, savedObj);

                packet.SavedBoard = mem.ToArray();
            }
            
            return packet;
        }
    }
}
