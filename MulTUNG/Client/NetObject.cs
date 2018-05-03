using System.Linq;
using UnityEngine;

namespace MulTUNG
{
    public class NetObject : MonoBehaviour
    {
        public int NetID { get; set; }

        public static GameObject GetByNetId(int id) => FindObjectsOfType<NetObject>().SingleOrDefault(o => o.NetID == id)?.gameObject;
    }
}
