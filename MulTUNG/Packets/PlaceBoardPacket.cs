using MulTUNG.Utils;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MulTUNG.Packets
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

        public override void Deserialize(IReader reader)
        {
            this.ParentBoardID = reader.ReadInt32();
            this.AuthorID = reader.ReadInt32();
            this.Position = reader.ReadVector3();
            this.EulerAngles = reader.ReadVector3();
            this.SavedBoard = Compressor.Decompress(reader.ReadByteArray());
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
