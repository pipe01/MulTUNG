using Common.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Client
{
    public class BoardManager
    {
        private struct RemoteBoard
        {
            public int ID { get; }
            public GameObject Object { get; }
        }

        public static BoardManager Instance { get; } = new BoardManager();

        private IDictionary<int, RemoteBoard> Boards = new Dictionary<int, RemoteBoard>();

        private BoardManager()
        {
        }

        public void NewBoard(PlaceBoardPacket packet)
        {
            
        }
    }
}
