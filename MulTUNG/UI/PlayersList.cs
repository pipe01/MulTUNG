using PiTung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MulTUNG.UI
{
    public class PlayersList
    {
        public static PlayersList Instance { get; } = new PlayersList();

        private const int FontSize = 15;

        private GUIStyle NormalStyle, SpecialStyle, HeaderStyle;

        private PlayersList()
        {
            NormalStyle = new GUIStyle
            {
                normal = new GUIStyleState
                {
                    background = ModUtilities.Graphics.CreateSolidTexture(1, 1, new Color(0, 0, 0, 0.2f)),
                    textColor = Color.white
                },
                fontSize = FontSize,
                padding = new RectOffset(1, 1, 1, 1)
            };

            SpecialStyle = new GUIStyle(NormalStyle)
            {
                normal = new GUIStyleState
                {
                    background = ModUtilities.Graphics.CreateSolidTexture(1, 1, new Color(0, 0.3f, 0, 0.3f)),
                    textColor = Color.cyan
                }
            };

            HeaderStyle = new GUIStyle(NormalStyle)
            {
                normal = new GUIStyleState
                {
                    background = ModUtilities.Graphics.CreateSolidTexture(1, 1, new Color(1, 0.8f, 0, 0.4f))
                }
            };
        }
        
        public void Draw()
        {
            IList<string> players = PlayerManager.Players.Select(o => o.Username).ToList();
            players.Insert(0, Network.Username);

            int longest = Math.Max(12, players.Max(o => o.Length));

            Vector2 entrySize = NormalStyle.CalcSize(new GUIContent(new string('A', longest)));
            Vector2 mainPosition = new Vector2(Screen.width - 40 - entrySize.x, 40);

            GUI.Label(new Rect(mainPosition, entrySize), $"Connected (<b>{players.Count}</b>)", HeaderStyle);

            float y = mainPosition.y + entrySize.y;

            foreach (var item in players.OrderBy(o => o))
            {
                var style = item == Network.Username ? SpecialStyle : NormalStyle;

                GUI.Label(new Rect(mainPosition.x, y, entrySize.x, entrySize.y), item, style);

                y += entrySize.y;
            }
        }
    }
}
