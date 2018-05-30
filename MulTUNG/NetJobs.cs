using MulTUNG.Packets;
using PiTung.Console;
using References;
using SavedObjects;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MulTUNG
{
    public interface INetJob
    {
        void Do();
    }

    public abstract class PacketNetJob<TPacket> : INetJob where TPacket : Packet
    {
        protected TPacket Packet { get; }

        public PacketNetJob(TPacket packet)
        {
            this.Packet = packet;
        }

        public abstract void Do();
    }

    public class PlaceBoardJob : PacketNetJob<PlaceBoardPacket>
    {
        private static BinaryFormatter BinFormatter = new BinaryFormatter();

        public PlaceBoardJob(PlaceBoardPacket packet) : base(packet)
        {
        }

        public override void Do()
        {
            NetObject parentBoard = NetObject.GetByNetId(Packet.ParentBoardID);
            SavedCircuitBoard savedBoard;
            
            using (var mem = new MemoryStream(Packet.SavedBoard))
            {
                savedBoard = (SavedCircuitBoard)BinFormatter.Deserialize(mem);
            }
            
            var boardObj = SavedObjectUtilities.LoadSavedObject(savedBoard, parentBoard?.transform);
            boardObj.transform.position = Packet.Position;
            boardObj.transform.eulerAngles = Packet.EulerAngles;

            foreach (var item in boardObj.GetComponentsInChildren<CircuitOutput>())
            {
                item.On = item.On;
            }

            BoardFunctions.RecalculateClustersOfBoard(boardObj);
            SnappingPeg.TryToSnapIn(boardObj);
        }
    }

    public struct DeleteBoardJob : INetJob
    {
        public int BoardID { get; }

        public DeleteBoardJob(int boardId)
        {
            this.BoardID = boardId;
        }

        public void Do()
        {
            var board = NetObject.GetByNetId(BoardID);

            if (board == null)
                return;

            BoardFunctions.DestroyAllWiresConnectedToBoardButNotPartOfIt(board.gameObject);
            MegaMeshManager.RemoveComponentsImmediatelyIn(board.gameObject);

            CircuitInput[] componentsInChildren = board.GetComponentsInChildren<CircuitInput>();
            CircuitOutput[] componentsInChildren2 = board.GetComponentsInChildren<CircuitOutput>();

            foreach (CircuitInput input in componentsInChildren)
            {
                StuffDeleter.DestroyInput(input);
            }

            foreach (CircuitOutput output in componentsInChildren2)
            {
                StuffDeleter.DestroyOutput(output);
            }

            GameObject.Destroy(board.gameObject);
        }
    }

    public class PlaceComponentJob : PacketNetJob<PlaceComponentPacket>
    {
        public PlaceComponentJob(PlaceComponentPacket packet) : base(packet)
        {
        }

        public override void Do()
        {
            var parentBoard = NetObject.GetByNetId(Packet.ParentBoardID);

            var component = SavedObjectUtilities.LoadSavedObject(Packet.SavedObject, parentBoard?.transform);
            component.AddComponent<NetObject>().NetID = Packet.NetID;
        }
    }

    public class DeleteComponentJob : PacketNetJob<DeleteComponentPacket>
    {
        public DeleteComponentJob(DeleteComponentPacket packet) : base(packet)
        {
        }

        public override void Do()
        {
            var obj = NetObject.GetByNetId(Packet.ComponentNetID).gameObject;

            var interactable = obj.GetComponentInChildren<Interactable>();
            if (interactable is DisplayInteractable)
            {
                EditDisplayColorMenu.Instance.DoneMenu();
            }

            CircuitInput[] componentsInChildren = obj.GetComponentsInChildren<CircuitInput>();
            CircuitOutput[] componentsInChildren2 = obj.GetComponentsInChildren<CircuitOutput>();
            foreach (CircuitInput input in componentsInChildren)
            {
                StuffDeleter.DestroyInput(input);
            }
            foreach (CircuitOutput output in componentsInChildren2)
            {
                StuffDeleter.DestroyOutput(output);
            }

            GameObject.Destroy(obj);
        }
    }

    public class PlaceWireJob : PacketNetJob<PlaceWirePacket>
    {
        public PlaceWireJob(PlaceWirePacket packet) : base(packet)
        {
        }

        public override void Do()
        {
            var netObj1 = NetObject.GetByNetId(Packet.NetObj1Id);
            var netObj2 = NetObject.GetByNetId(Packet.NetObj2Id);

            if (netObj1 == null || netObj2 == null)
                return;
            
            var io1 = netObj1.IO[Packet.Point1Id];
            var io2 = netObj2.IO[Packet.Point2Id];

            if (io1 == null || io2 == null)
                return;

            var wireObj = GameObject.Instantiate(Prefabs.Wire);
            Wire wire;

            if (io1.tag == "Input" && io2.tag == "Input")
            {
                wire = wireObj.AddComponent<InputInputConnection>();
            }
            else
            {
                wire = wireObj.AddComponent<InputOutputConnection>();
            }

            wire.Point1 = Wire.GetWireReference(io1);
            wire.Point2 = Wire.GetWireReference(io2);

            wire.DrawWire();

            wire.SetPegsBasedOnPoints();
            StuffConnector.LinkConnection(wire);
            StuffConnector.SetAppropriateConnectionParent(wire);

            SoundPlayer.PlaySoundAt(Sounds.ConnectionFinal, wireObj);
            
            wireObj.AddComponent<ObjectInfo>().ComponentType = ComponentType.Wire;
            wireObj.AddComponent<NetObject>().NetID = Packet.NetID;
            wireObj.GetComponent<Collider>().enabled = true;
        }
    }

    public class DeleteWireJob : PacketNetJob<DeleteWirePacket>
    {
        public DeleteWireJob(DeleteWirePacket packet) : base(packet)
        {
        }

        public override void Do()
        {
            var wire = NetObject.GetByNetId(Packet.WireNetID)?.gameObject;

            if (wire == null)
                return;
            
            StuffDeleter.DestroyWire(wire);
        }
    }

    public class RotateComponentJob : PacketNetJob<RotateComponentPacket>
    {
        public RotateComponentJob(RotateComponentPacket packet) : base(packet)
        {
        }

        public override void Do()
        {
            var RotateThis = NetObject.GetByNetId(Packet.ComponentID)?.gameObject;

            if (RotateThis != null)
            {
                BoxCollider[] componentsInChildren = RotateThis.GetComponentsInChildren<BoxCollider>();
                SoundPlayer.PlaySoundAt(Sounds.RotateSomething, RotateThis);

                RotateThis.transform.localEulerAngles = Packet.EulerAngles;

                FloatingPointRounder.RoundIn(RotateThis, false);
                StuffRotater.RedrawCircuitGeometryOf(RotateThis);
                StuffRotater.DestroyIntersectingConnections(RotateThis);
                SnappingPeg.TryToSnapIn(RotateThis);
                MegaMeshManager.RecalculateGroupsOf(RotateThis);
            }
        }
    }
}
