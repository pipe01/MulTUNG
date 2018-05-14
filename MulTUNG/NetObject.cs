using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace MulTUNG
{
    public class NetObject : MonoBehaviour
    {
        public static IDictionary<int, NetObject> Alive { get; } = new Dictionary<int, NetObject>();

        private int _netID;
        public int NetID
        {
            get => _netID;
            set
            {
                Alive.Remove(_netID);
                _netID = value;
                Alive.Add(_netID, this);
            }
        }

        public IList<GameObject> IO { get; private set; } = new GameObject[0];

        void Awake()
        {
            Alive.Add(NetID, this);

            //Incoming LINQ shenanigans
            this.IO = new ReadOnlyCollection<GameObject>(GetComponentsInChildren<CircuitInput>().Select(o => o.gameObject).Concat(GetComponentsInChildren<CircuitOutput>().Select(o => o.gameObject)).ToList());
        }

        void OnDestroy()
        {
            Alive.Remove(NetID);
        }

        public static NetObject GetByNetId(int id)
        {
            if (Alive.TryGetValue(id, out var obj))
                return obj;

            return null;
        }
    }
}
