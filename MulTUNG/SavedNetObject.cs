using SavedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
