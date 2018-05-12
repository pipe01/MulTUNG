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

        void LoadLog(PacketLog log)
        {
            lvPackets.Items.Clear();
            ShownEntries.Clear();

            lvPackets.BeginUpdate();
            foreach (var entry in log.Entries)
            {
                if ((chkHidePackets.Checked && (entry.Packet is StateListPacket || entry.Packet is PlayerStatePacket))
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

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Log = PacketLog.Load(openFileDialog1.FileName);

                LoadLog(Log);
            }
        }

        private void chkHidePackets_CheckedChanged(object sender, EventArgs e)
        {
            LoadLog(Log);
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
    }
}
