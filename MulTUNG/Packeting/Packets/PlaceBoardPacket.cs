using MulTUNG.Utils;
using PiTung.Console;
using System.Collections.Generic;
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

        public int BoardID { get; set; }
        public int ParentBoardID { get; set; }
        public int AuthorID { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 EulerAngles { get; set; }
        public byte[] SavedBoard { get; set; } = new byte[0];
        public Dictionary<Tuple<Vector3, Vector3>, int> IdByPosition { get; set; } = new Dictionary<Tuple<Vector3, Vector3>, int>();
        
        protected override byte[] SerializeInner()
        {
            var builder = new PacketBuilder()
                .Write(BoardID)
                .Write(ParentBoardID)
                .Write(AuthorID)
                .Write(Position)
                .Write(EulerAngles)
                .Write(Compressor.Compress(SavedBoard))
                .Write(IdByPosition.Count);

            foreach (var item in IdByPosition)
            {
                builder.Write(item.Key.Item1);
                builder.Write(item.Key.Item2);
                builder.Write(item.Value);
            }
            
            return builder.Done();
        }

        public static PlaceBoardPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<PlaceBoardPacket>();
            packet.BoardID = reader.ReadInt32();
            packet.ParentBoardID = reader.ReadInt32();
            packet.AuthorID = reader.ReadInt32();
            packet.Position = reader.ReadVector3();
            packet.EulerAngles = reader.ReadVector3();
            packet.SavedBoard = Compressor.Decompress(reader.ReadByteArray());

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                packet.IdByPosition.Add(new Tuple<Vector3, Vector3>(reader.ReadVector3(), reader.ReadVector3()), reader.ReadInt32());
            }

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
                BoardID = netObj.NetID,
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
