using PiTung;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    public class NetUtilitiesComponent : MonoBehaviour
    {
        private static NetUtilitiesComponent _Instance;
        public static NetUtilitiesComponent Instance => _Instance ?? (_Instance = ModUtilities.DummyComponent.gameObject.AddComponent<NetUtilitiesComponent>());
        
        private Queue<INetJob> JobQueue = new Queue<INetJob>();

        public NetUtilitiesComponent()
        {
            if (_Instance != null)
            {
                Destroy(this);
            }
        }

        public void Enqueue(INetJob job)
        {
            JobQueue.Enqueue(job);
        }

        void Update()
        {
            while (JobQueue.Count > 0)
            {
                JobQueue.Dequeue().Do();
            }
        }
    }
}
