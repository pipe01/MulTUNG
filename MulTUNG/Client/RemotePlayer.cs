﻿using MulTUNG.Packeting.Packets;
using UnityEngine;

namespace MulTUNG.Client
{
    public class RemotePlayer : MonoBehaviour
    {
        public string Username { get; set; }
        public float LastUpdateTime { get; set; }

        private static GUIStyle DefaultUsernameStyle = new GUIStyle();

        private Vector3 LastPosition = Vector3.zero,
                        NextPosition = Vector3.zero;

        private Quaternion LastRotation = Quaternion.identity,
                           NextRotation = Quaternion.identity;

        private float TimeBetweenStates = 0;
        private float LastStateTime = 0;
        private GUIStyle UsernameStyle = new GUIStyle(DefaultUsernameStyle);

        private const float MaxUsernameDistance = 10;

        void FixedUpdate()
        {
            float t = (Time.time - LastStateTime) / TimeBetweenStates;

            transform.position = Vector3.Lerp(LastPosition, NextPosition, t);
            transform.rotation = Quaternion.Lerp(LastRotation, NextRotation, t);
        }
        
        public void UpdateState(PlayerState state)
        {
            TimeBetweenStates = Time.time - LastStateTime;
            LastStateTime = Time.time;

            LastPosition = transform.position;
            NextPosition = state.Position - new Vector3(0, 1.68f / 2, 0);

            LastRotation = transform.rotation;
            NextRotation = Quaternion.Euler(state.EulerAngles);
        }
    }
}
