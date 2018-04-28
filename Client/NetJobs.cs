using Common.Packets;
using References;
using UnityEngine;

namespace Client
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
            GameObject gameObject = Object.Instantiate(Prefabs.CircuitBoard, Packet.Position, Quaternion.Euler(Packet.EulerAngles));

            gameObject.AddComponent<ObjectInfo>().ComponentType = ComponentType.CircuitBoard;
            gameObject.AddComponent<NetBoard>().BoardID = Packet.BoardID;
            gameObject.tag = "NetBoard" + Packet.BoardID;

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
            var obj = GameObject.FindGameObjectWithTag("NetBoard" + BoardID);

            if (!obj)
                return;

            GameObject.Destroy(obj);
        }
    }
}
