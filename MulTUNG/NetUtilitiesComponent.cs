using PiTung;
using PiTung.Console;
using System.Collections.Generic;
using UnityEngine;

namespace MulTUNG
{
    public class NetUtilitiesComponent : MonoBehaviour
    {
        public static NetUtilitiesComponent Instance { get; private set; }
        
        private Queue<INetJob> JobQueue = new Queue<INetJob>();

        public INetJob CurrentJob { get; private set; }

        public NetUtilitiesComponent()
        {
            if (Instance != null)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        void Awake()
        {
            PlayerManager.BuildPlayerPrefab();

            GameObject.Instantiate(PlayerManager.PlayerModelPrefab, new Vector3(181.5f, 25.0f, -143.1f), Quaternion.identity);
        }

        public void Enqueue(INetJob job)
        {
            JobQueue.Enqueue(job);
        }

        void Update()
        {
            if (JobQueue.Count > 0)
            {
                CurrentJob = JobQueue.Dequeue();
                CurrentJob.Do();
                CurrentJob = null;
            }
        }
    }
}
