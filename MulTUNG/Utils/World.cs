using SavedObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MulTUNG.Utils
{
    public static class World
    {
        public static void AddNetObjects()
        {
            foreach (var item in GameObject.FindObjectsOfType<ObjectInfo>())
            {
                if (item.GetComponent<NetObject>() == null)
                    item.gameObject.AddComponent<NetObject>().NetID = NetObject.GetNewID();
            }
        }

        public static byte[] Serialize()
        {
            var world = new SavedWorld();

            //Get the top level objects
            world.TopLevelObjects = SaveManager.GetTopLevelObjects();
            
            BinaryFormatter bin = new BinaryFormatter();

            //Serialize the SavedWorld
            using (MemoryStream mem = new MemoryStream())
            {
                bin.Serialize(mem, world);
                
                return Compressor.Compress(mem.ToArray());
            }
        }
        
        public static void Deserialize(byte[] data)
        {
            //Clear scene
            MegaMeshManager.ClearReferences();
            BehaviorManager.ClearAllLists();
            foreach (ObjectInfo objectInfo in GameObject.FindObjectsOfType<ObjectInfo>())
            {
                GameObject.Destroy(objectInfo.gameObject);
            }
            
            SavedWorld world;

            //Deserialize the data into a SavedWorld
            using (MemoryStream mem = new MemoryStream(Compressor.Decompress(data)))
            {
                world = (SavedWorld)new BinaryFormatter().Deserialize(mem);
            }
            
            //Run on Unity thread
            MulTUNG.SynchronizationContext.Send(_ =>
            {
                //Load all the objects from the save
                foreach (var item in world.TopLevelObjects)
                {
                    SavedObjectUtilities.LoadSavedObject(item);
                }

                SaveManager.RecalculateAllClustersEverywhereWithDelay();

            }, null);
        }
        
        [Serializable]
        private class SavedWorld
        {
            public List<SavedObjectV2> TopLevelObjects;
        }
    }
}
