using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Packet_Log_Viewer
{
    public partial class frmHexViewer : Form
    {
        public frmHexViewer(byte[] data)
        {
            InitializeComponent();

            var ctrl = new ByteViewer();
            ctrl.SetBytes(data);
            ctrl.Dock = DockStyle.Fill;

            this.Controls.Add(ctrl);
        }

        private void frmHexViewer_Load(object sender, EventArgs e)
        {

        }
    }
}
