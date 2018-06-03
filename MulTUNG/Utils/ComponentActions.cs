using MulTUNG.Packets;
using PiTung;
using System.Collections.Generic;
using System.Reflection;
using StateKey = System.Collections.Generic.KeyValuePair<int, byte>;

namespace MulTUNG.Utils
{
    public static class ComponentActions
    {
        public static List<int> CurrentlyActing { get; } = new List<int>();
        public static List<Button> PushedDownButtons { get; } = new List<Button>();
        public static bool HasCalledCircuitUpdate { get; set; }

        public static CircuitOutput CurrentlyUpdating = null;
        
        private static Dictionary<CircuitOutput, StateKey> KeysCache = new Dictionary<CircuitOutput, StateKey>();
        
        public static void DoAction(UserInputPacket packet)
        {
            var obj = NetObject.GetByNetId(packet.NetID);

            CurrentlyActing.Add(packet.NetID);

            switch (packet.Receiver)
            {
                case UserInputPacket.UserInputReceiver.Button:
                    DoButton(obj.GetComponentInChildren<Button>(), packet.State);
                    break;
                case UserInputPacket.UserInputReceiver.Switch:
                    DoSwitch(obj.GetComponentInChildren<Switch>(), packet.State);
                    break;
            }

            CurrentlyActing.Remove(packet.NetID);
        }

        private static void DoButton(Button button, bool state)
        {
            if (state)
            {
                ModUtilities.ExecuteMethod(button, "ButtonDown");
                PushedDownButtons.Add(button);
            }
            else
            {
                ModUtilities.ExecuteMethod(button, "ButtonUp");
                PushedDownButtons.Remove(button);
            }
        }

        private static void DoSwitch(Switch @switch, bool state)
        {
            @switch.On = state;
            @switch.UpdateLever();
        }

        public static void DoData(ComponentDataPacket packet)
        {
            var netObj = NetObject.GetByNetId(packet.NetID);

            if (netObj == null)
                return;

            switch (packet.ComponentType)
            {
                case ComponentType.Noisemaker:
                    var noisemaker = netObj.GetComponentInChildren<Noisemaker>();
                    noisemaker.ToneFrequency = (float)packet.Data[0];

                    break;
                case ComponentType.Display:
                    var display = netObj.GetComponentInChildren<Display>();
                    display.DisplayColor = (DisplayColor)packet.Data[0];
                    display.ForceVisualRefresh();

                    break;
                case ComponentType.Label:
                    var label = netObj.GetComponent<Label>();
                    label.text.text = (string)packet.Data[0];
                    label.text.fontSize = (float)packet.Data[1];

                    break;
            }
        }

        public static void UpdateStates(CircuitStatePacket packet)
        {
            foreach (var state in packet.States)
            {
                var netObj = NetObject.GetByNetId(state.Key.Key);
                
                if (netObj == null || !netObj.enabled)
                    continue;

                var ios = netObj.GetComponentsInChildren<CircuitOutput>();

                if (ios.Length <= state.Key.Value)
                    continue;

                var io = ios[state.Key.Value];

                CurrentlyUpdating = io;
                io.On = state.Value;
            }

            CurrentlyUpdating = null;
            HasCalledCircuitUpdate = true;
        }

        public static bool TryGetKeyFromOutput(CircuitOutput output, out StateKey key, bool clearCache = false)
        {
            if (clearCache || !KeysCache.TryGetValue(output, out key))
            {
                var component = ComponentPlacer.FullComponent(output.transform);

                var netObj = component.GetComponent<NetObject>();

                if (netObj == null)
                {
                    key = default(StateKey);
                    return false;
                }

                byte ioIndex = 0;

                foreach (var item in component.GetComponentsInChildren<CircuitOutput>())
                {
                    if (item == output)
                        break;

                    ioIndex++;
                }

                key = KeysCache[output] = new KeyValuePair<int, byte>(netObj.NetID, ioIndex);
            }

            return true;
        }

        public static void DoPaint(PaintBoardPacket packet)
        {
            var obj = NetObject.GetByNetId(packet.BoardID).gameObject;
            var board = obj.GetComponent<CircuitBoard>();

            board.SetBoardColor(packet.Color);
        }
    }
}
