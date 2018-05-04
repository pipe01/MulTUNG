using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace MulTUNG
{
    public class NetObject : MonoBehaviour
    {
        public int NetID { get; set; }

        public IList<GameObject> IO { get; private set; } = new GameObject[0];

        void Awake()
        {
            //Incoming LINQ shenanigans
            this.IO = new ReadOnlyCollection<GameObject>(GetComponentsInChildren<CircuitInput>().Select(o => o.gameObject).Concat(GetComponentsInChildren<CircuitOutput>().Select(o => o.gameObject)).ToList());
        }

        public static GameObject GetByNetId(int id) => FindObjectsOfType<NetObject>().SingleOrDefault(o => o.NetID == id)?.gameObject;
    }
}
