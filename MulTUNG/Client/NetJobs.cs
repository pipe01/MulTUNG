using MulTUNG.Packeting.Packets;
using References;
using System.Linq;
using UnityEngine;

namespace MulTUNG
{
    public interface INetJob
    {
        void Do();
    }

    public struct PlaceBoardJob : INetJob
    {
        private PlaceBoardPacket Packet;

        public PlaceBoardJob(PlaceBoardPacket packet)
        {
            this.Packet = packet;
        }

        public void Do()
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

    public struct PlaceComponentJob : INetJob
    {
        private PlaceComponentPacket Packet;

        public PlaceComponentJob(PlaceComponentPacket packet)
        {
            this.Packet = packet;
        }

        public void Do()
        {
            var parentBoard = NetObject.GetByNetId(Packet.ParentBoardID);

            var component = SavedObjectUtilities.LoadSavedObject(Packet.SavedObject, parentBoard?.transform);
            component.AddComponent<NetObject>().NetID = Packet.NetID;
        }
    }

    public struct DeleteComponentJob : INetJob
    {
        private int NetID;

        public DeleteComponentJob(int id)
        {
            this.NetID = id;
        }

        public void Do()
        {
            var obj = NetObject.GetByNetId(NetID);

            GameObject.Destroy(obj);
        }
    }
}
