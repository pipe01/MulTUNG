using PiTung.Mod_utilities;
using System.Linq;
using System.Net;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace MulTUNG.UI
{
    public class ConnectDialog : IDialog
    {
        private Rect WindowRect = new Rect(0, 0, 200, 130);
        
        public string Host { get; private set; }
        public string Port { get; private set; }
        public string Username { get; private set; }

        public bool Visible { get; set; } = true;

        public ConnectDialog()
        {
            WindowRect.x = Screen.width / 2 - WindowRect.width / 2;
            WindowRect.y = Screen.height / 2 - WindowRect.height / 2;
            
            Host = Configuration.Get("Host", "127.0.0.1");
            Port = Configuration.Get<long>("Port", Constants.Port).ToString();
            Username = Configuration.Get("Username", "");
        }

        public void Draw()
        {
            if (!Visible)
                return;

            WindowRect = GUI.Window(8745, WindowRect, DrawWindow, "Connect");
        }

        private void DrawWindow(int id)
        {
            BeginVertical();
            {
                BeginHorizontal();
                {
                    Label("Host");
                    FlexibleSpace();
                    Host = TextField(Host, Width(WindowRect.width / 2));
                }
                EndHorizontal();

                BeginHorizontal();
                {
                    Label("Port");
                    FlexibleSpace();
                    UpdatePort(TextField(Port, Width(WindowRect.width / 2)));
                }
                EndHorizontal();

                Space(5);

                BeginHorizontal();
                {
                    Label("Username");
                    FlexibleSpace();
                    Username = TextField(Username, Width(WindowRect.width / 2));
                }
                EndHorizontal();

                if (Button("Connect"))
                {
                    Configuration.Set("Username", Username);

                    if (IPAddress.TryParse(Host, out var address))
                    {
                        Configuration.Set("Host", Host);

                        int port = int.Parse(Port);
                        Configuration.Set("Port", port);

                        NetworkClient.Instance.SetUsername(Username);
                        MulTUNG.Connect(new IPEndPoint(address, port));

                        Visible = false;
                    }
                }
            }
            EndVertical();

            GUI.DragWindow();
        }

        private void UpdatePort(string newPort)
        {
            Port = new string(newPort.Where(char.IsDigit).ToArray());
        }
    }
}
