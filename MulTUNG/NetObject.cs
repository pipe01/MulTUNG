using PiTung.Console;
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
                if (Alive.ContainsKey(value))
                    return;

                if (Alive.ContainsKey(_netID))
                    Alive.Remove(_netID);

                _netID = value;

                Alive.Add(_netID, this);
            }
        }

        public IList<GameObject> IO => GetComponentsInChildren<CircuitInput>().Select(o => o.gameObject).Concat(GetComponentsInChildren<CircuitOutput>().Select(o => o.gameObject)).ToList();

        void Awake()
        {
            if (Alive.Values.Contains(this))
            {
                Destroy(this);
            }
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

        public static int GetNewID() => Random.Range(int.MinValue, int.MaxValue);
    }
}
