using PiTung;
using System.Collections.Generic;
using UnityEngine;

namespace MulTUNG
{
    public class NetUtilitiesComponent : MonoBehaviour
    {
        private static NetUtilitiesComponent _Instance;
        public static NetUtilitiesComponent Instance => _Instance ?? (_Instance = ModUtilities.DummyComponent.gameObject.AddComponent<NetUtilitiesComponent>());
        
        private Queue<INetJob> JobQueue = new Queue<INetJob>();

        public INetJob CurrentJob { get; private set; }

        public NetUtilitiesComponent()
        {
            if (_Instance != null)
            {
                Destroy(this);
            }
        }

        void Awake()
        {
            PlayerManager.BuildPlayerPrefab();

            GameObject.Instantiate(PlayerManager.PlayerModelPrefab, new Vector3(181.5f, 27.0f, -143.1f), Quaternion.identity);
        }

        public void Enqueue(INetJob job)
        {
            JobQueue.Enqueue(job);
        }

        void Update()
        {
            while (JobQueue.Count > 0)
            {
                CurrentJob = JobQueue.Dequeue();

                CurrentJob.Do();

                CurrentJob = null;
            }
        }
    }
}
