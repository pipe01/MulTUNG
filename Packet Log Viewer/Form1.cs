using MulTUNG.Packeting.Packets;
using MulTUNG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Packet_Log_Viewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private PacketLog Log;
        private List<PacketLogEntry> ShownEntries = new List<PacketLogEntry>();
        private bool Loading = false;
        private List<Type> HiddenPacketTypes = new List<Type>();

        private IList<Type> PacketTypes =
            typeof(Packet).Assembly.GetTypes()
                .Where(o => o.BaseType == typeof(Packet))
                .OrderBy(o => o.Name)
                .ToList();

        void LoadLog(PacketLog log = null)
        {
            log = log ?? this.Log;

            lvPackets.Items.Clear();
            ShownEntries.Clear();

            lvPackets.BeginUpdate();
            foreach (var entry in log.Entries)
            {
                if (HiddenPacketTypes.Contains(entry.Packet.GetType())
                    || (entry.In && !chkIn.Checked)
                    || (entry.Out && !chkOut.Checked))
                {
                    continue;
                }

                var item = lvPackets.Items.Add(entry.Packet.Type.ToString());
                var time = TimeSpan.FromSeconds(entry.UnityTime);

                item.SubItems.Add(time.ToString(@"hh\:mm\:ss\.ffff"));
                item.SubItems.Add(entry.In ? "In" : "Out");

                ShownEntries.Add(entry);
            }
            lvPackets.EndUpdate();
        }

        private void LoadCheckedPacketTypes()
        {
            if (lvPacketTypes.Items.Count != PacketTypes.Count)
                return;

            HiddenPacketTypes.Clear();

            for (int i = 0; i < PacketTypes.Count; i++)
            {
                var lvItem = lvPacketTypes.Items[i];

                if (!lvItem.Checked)
                    HiddenPacketTypes.Add(PacketTypes[i]);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Log = PacketLog.Load(openFileDialog1.FileName);

                LoadLog();
            }
        }

        private void chkHidePackets_CheckedChanged(object sender, EventArgs e)
        {
            LoadLog();
        }

        private void lvPackets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvPackets.SelectedIndices.Count == 0)
                return;

            var entry = ShownEntries[lvPackets.SelectedIndices[0]];

            pgPacket.SelectedObject = entry.Packet;
        }

        private void lvPackets_DoubleClick(object sender, EventArgs e)
        {
            if (lvPackets.SelectedIndices.Count == 0)
                return;

            var entry = ShownEntries[lvPackets.SelectedIndices[0]];

            byte[] data = entry.Packet.Serialize();

            new frmHexViewer(data).Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Loading = true;
            lvPacketTypes.BeginUpdate();

            foreach (var type in PacketTypes)
            {
                var item = lvPacketTypes.Items.Add(type.Name.Replace("Packet", ""));

                item.Checked = 
                    type != typeof(PlayerStatePacket)
                    && type != typeof(StateListPacket)
                    && type != typeof(CircuitStatePacket);
            }

            LoadCheckedPacketTypes();

            lvPacketTypes.EndUpdate();
            Loading = false;

            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                try
                {
                    Log = PacketLog.Load(args[1]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to open log file. Details: \n" + ex, "Error", MessageBoxButtons.OK);
                    return;
                }

                LoadLog(Log);
            }
        }

        private void lvPacketTypes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (Loading)
                return;

            LoadCheckedPacketTypes();
            LoadLog();
        }
    }
}
