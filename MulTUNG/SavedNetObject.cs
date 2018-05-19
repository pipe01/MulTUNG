using SavedObjects;
using System;

namespace MulTUNG
{
    [Serializable]
    public class SavedNetObject : SavedObjectV2
    {
        public int NetID { get; set; }

        public SavedNetObject()
        {
        }

        public SavedNetObject(int netId)
        {
            this.NetID = netId;
        }
    }
}
