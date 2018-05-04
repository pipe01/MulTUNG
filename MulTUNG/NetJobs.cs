using MulTUNG.Packeting.Packets;
using PiTung;
using PiTung.Console;
using References;
using System.Linq;
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
        public PlaceBoardJob(PlaceBoardPacket packet) : base(packet)
        {
        }

        public override void Do()
        {
            GameObject parentBoard = NetObject.GetByNetId(Packet.ParentBoardID);

            GameObject gameObject = Object.Instantiate(Prefabs.CircuitBoard, Packet.Position, Quaternion.Euler(Packet.EulerAngles), parentBoard?.transform);

            gameObject.AddComponent<ObjectInfo>().ComponentType = ComponentType.CircuitBoard;
            gameObject.AddComponent<NetObject>().NetID = Packet.BoardID;

            CircuitBoard component = gameObject.GetComponent<CircuitBoard>();
            component.x = Packet.Width;
            component.z = Packet.Height;
            component.CreateCuboid();

            MegaMeshManager.AddComponentsIn(gameObject);
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
            
            if (board != null)
                StuffDeleter.DeleteThing(board.gameObject);
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
            var obj = NetObject.GetByNetId(Packet.ComponentNetID);

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
            var netObj1 = NetObject.GetByNetId(Packet.NetObj1Id)?.GetComponent<NetObject>();
            var netObj2 = NetObject.GetByNetId(Packet.NetObj2Id)?.GetComponent<NetObject>();

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
            var wire = NetObject.GetByNetId(Packet.WireNetID);

            if (wire == null)
                return;

            GameObject.Destroy(wire.GetComponent<NetObject>());

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
            var RotateThis = NetObject.GetByNetId(Packet.ComponentID);

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
