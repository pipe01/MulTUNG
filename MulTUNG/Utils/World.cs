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

            world.TopLevelObjects = SaveManager.GetTopLevelObjects();

            Queue<int> netIds = new Queue<int>();

            foreach (var item in GameObject.FindObjectsOfType<NetObject>())
            {
                netIds.Enqueue(item.NetID);
            }

            world.NetIDs = netIds.ToList();

            BinaryFormatter bin = new BinaryFormatter();

            using (MemoryStream mem = new MemoryStream())
            {
                bin.Serialize(mem, world);
                
                return mem.ToArray();
            }
        }

        private static List<SavedNetObject> GetObjects(GameObject parent = null)
        {
            List<SavedNetObject> list = new List<SavedNetObject>();
            List<ObjectInfo> objects = SaveManager.ActiveSaveObjects;

            if (parent != null)
            {
                objects = new List<ObjectInfo>();

                List<SavedObjectV2> children = new List<SavedObjectV2>();
                for (int i = 0; i < parent.transform.childCount; i++)
                {
                    ObjectInfo component = parent.transform.GetChild(i).GetComponent<ObjectInfo>();

                    if (component != null)
                    {
                        objects.Add(component);
                    }
                }
            }

            foreach (ObjectInfo objectInfo in objects)
            {
                if (parent != null || (parent == null && objectInfo.transform.parent == null))
                {
                    var netObj = objectInfo.GetComponent<NetObject>();

                    if (netObj != null)
                    {
                        var savedObj = SavedObjectUtilities.CreateSavedObjectFrom(objectInfo);
                        var savedNet = new SavedNetObject(savedObj, netObj.NetID);
                        
                        savedNet.Children = GetObjects(objectInfo.gameObject);

                        list.Add(savedNet);
                    }
                }
            }
            return list;
        }

        public static void Deserialize(byte[] data)
        {
            MegaMeshManager.ClearReferences();
            BehaviorManager.AllowedToUpdate = false;
            BehaviorManager.ClearAllLists();
            foreach (ObjectInfo objectInfo in GameObject.FindObjectsOfType<ObjectInfo>())
            {
                GameObject.Destroy(objectInfo.gameObject);
            }

            SavedWorld world;

            using (MemoryStream mem = new MemoryStream(data))
            {
                world = (SavedWorld)new BinaryFormatter().Deserialize(mem);
            }
            
            MulTUNG.SynchronizationContext.Send(o =>
            {
                var netIds = (Queue<int>)o;

                foreach (var item in world.TopLevelObjects)
                {
                    var obj = SavedObjectUtilities.LoadSavedObject(item);
                }

                World.AddNetObjects();

                foreach (var item in GameObject.FindObjectsOfType<NetObject>())
                {
                    item.NetID = netIds.Dequeue();
                }
            }, new Queue<int>(world.NetIDs));

            BehaviorManager.AllowedToUpdate = true;
        }
        
        [Serializable]
        private class SavedWorld
        {
            public List<SavedObjectV2> TopLevelObjects;
            public List<int> NetIDs;
        }
    }
}
