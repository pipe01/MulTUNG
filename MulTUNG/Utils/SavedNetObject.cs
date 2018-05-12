using SavedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Utils
{
    [Serializable]
    public class SavedNetObject
    {
        public int NetID;
        public SavedObjectV2 SavedObject;
        public List<SavedNetObject> Children = new List<SavedNetObject>();

        public SavedNetObject(SavedObjectV2 savedObj, int netId)
        {
            this.SavedObject = savedObj;
            this.NetID = netId;
        }
    }
}
