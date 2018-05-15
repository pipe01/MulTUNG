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
                    item.gameObject.AddComponent<NetObject>().NetID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
        }

        public static byte[] Serialize()
        {
            var world = new SavedWorld();

            //Get the top level objects
            world.TopLevelObjects = SaveManager.GetTopLevelObjects();
            
            //Add the components' IDs to the dictionary
            world.NetIDs = new Dictionary<Tuple<SerializableVector3, SerializableVector3>, int>();
            foreach (var item in GameObject.FindObjectsOfType<NetObject>())
            {
                world.NetIDs.Add(new Tuple<SerializableVector3, SerializableVector3>(item.transform.position, item.transform.eulerAngles), item.NetID);
            }
            
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
            MulTUNG.SynchronizationContext.Send(o =>
            {
                var netIds = (Dictionary<Tuple<SerializableVector3, SerializableVector3>, int>)o;

                //Load all the objects from the save
                foreach (var item in world.TopLevelObjects)
                {
                    SavedObjectUtilities.LoadSavedObject(item);
                }

                SaveManager.RecalculateAllClustersEverywhereWithDelay();
                
                //Add a NetObject component to all components
                World.AddNetObjects();

                //Go through each NetObject and assign them an ID taken from the net IDs queue
                foreach (var item in GameObject.FindObjectsOfType<NetObject>())
                {
                    if (netIds.TryGetValue(new Tuple<SerializableVector3, SerializableVector3>(item.transform.position, item.transform.eulerAngles), out var id))
                    {
                        item.NetID = id;
                    }
                    else
                    {
                        MyDebug.Log("ERROR: Missing object ID for " + item.gameObject);
                    }
                }

            }, world.NetIDs);
        }
        
        [Serializable]
        private class SavedWorld
        {
            public List<SavedObjectV2> TopLevelObjects;
            public Dictionary<Tuple<SerializableVector3, SerializableVector3>, int> NetIDs;
        }
    }
}
