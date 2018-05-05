using MulTUNG.Packeting.Packets;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MulTUNG.Client
{
    public class RemotePlayer : MonoBehaviour
    {
        public float LastUpdateTime { get; set; }

        private Vector3 LastPosition = Vector3.zero,
                        NextPosition = Vector3.zero;

        private Quaternion LastRotation = Quaternion.identity,
                           NextRotation = Quaternion.identity;

        private float TimeBetweenStates = 0;
        private float LastStateTime = 0;

        void FixedUpdate()
        {
            float t = (Time.time - LastStateTime) / TimeBetweenStates;
            
            transform.position = Vector3.Lerp(LastPosition, NextPosition, t);
            transform.rotation = Quaternion.Lerp(LastRotation, NextRotation, t);
        }

        public void UpdateWithPacket(PlayerStatePacket state)
        {
            //if (state.Time < LastStateTime)
            //    return;
            
            TimeBetweenStates = Time.time - LastStateTime;
            LastStateTime = Time.time;

            LastPosition = transform.position;
            NextPosition = state.Position - new Vector3(0, 1.68f / 2, 0);

            LastRotation = transform.rotation;
            NextRotation = Quaternion.Euler(state.EulerAngles);
        }
    }
}
