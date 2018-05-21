﻿using MulTUNG.Packeting.Packets;
using PiTung;
using PiTung.Console;
using System;
using UnityEngine;

namespace MulTUNG.Client
{
    public class RemotePlayer : MonoBehaviour
    {
        private const float MaxDistance = 8;
        private const float UsernameSize = 4;

        public string Username { get; set; }
        public float LastUpdateTime { get; set; }
        
        private Vector3 LastPosition = Vector3.zero,
                        NextPosition = Vector3.zero;

        private Quaternion LastRotation = Quaternion.identity,
                           NextRotation = Quaternion.identity;

        private float TimeBetweenStates = 0;
        private float LastStateTime = 0;

        private GUIStyle UsernameStyle;
        private Rect UsernameRect = new Rect();

        void FixedUpdate()
        {
            float t = (Time.time - LastStateTime) / TimeBetweenStates;

            transform.position = Vector3.Lerp(LastPosition, NextPosition, t);
            transform.rotation = Quaternion.Lerp(LastRotation, NextRotation, t);

            if (UsernameStyle == null)
                return;

            Vector3 worldPos = transform.position + new Vector3(0, 1.5f, 0);
            Vector3 screenPos = FirstPersonInteraction.FirstPersonCamera.WorldToScreenPoint(worldPos);

            float distance = Vector3.Distance(worldPos, FirstPersonInteraction.FirstPersonCamera.transform.position);
            float scale = Mathf.LerpUnclamped(0, UsernameSize, MaxDistance / distance);
            UsernameStyle.fontSize = (int)(UsernameSize * scale);

            UsernameRect = new Rect(Vector2.zero, UsernameStyle.CalcSize(new GUIContent(Username)));
            UsernameRect.x = screenPos.x - (UsernameRect.width / 2);
            UsernameRect.y = Screen.height - screenPos.y;
        }

        void OnGUI()
        {
            if (UsernameStyle == null)
                UsernameStyle = new GUIStyle(GUI.skin.label);

            GUI.Label(UsernameRect, Username, UsernameStyle);
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
