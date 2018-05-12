﻿using SavedObjects;
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

            //Create a new queue that will store all of the objects' net IDs in order
            Queue<int> netIds = new Queue<int>();

            foreach (var item in GameObject.FindObjectsOfType<NetObject>())
            {
                netIds.Enqueue(item.NetID);
            }

            world.NetIDs = netIds.ToList();

            BinaryFormatter bin = new BinaryFormatter();

            //Serialize the SavedWorld
            using (MemoryStream mem = new MemoryStream())
            {
                bin.Serialize(mem, world);
                
                return mem.ToArray();
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
            using (MemoryStream mem = new MemoryStream(data))
            {
                world = (SavedWorld)new BinaryFormatter().Deserialize(mem);
            }
            
            //Run on Unity thread
            MulTUNG.SynchronizationContext.Send(o =>
            {
                var netIds = (Queue<int>)o;

                //Load all the objects from the save
                foreach (var item in world.TopLevelObjects)
                {
                    var obj = SavedObjectUtilities.LoadSavedObject(item);
                }

                //Add a NetObject component to all of them
                World.AddNetObjects();

                //Go through each NetObject and assign them an ID taken from the net IDs queue
                foreach (var item in GameObject.FindObjectsOfType<NetObject>())
                {
                    item.NetID = netIds.Dequeue();
                }

                SaveManager.RecalculateAllClustersEverywhereWithDelay();
            }, new Queue<int>(world.NetIDs));
        }
        
        [Serializable]
        private class SavedWorld
        {
            public List<SavedObjectV2> TopLevelObjects;
            public List<int> NetIDs;
        }
    }
}