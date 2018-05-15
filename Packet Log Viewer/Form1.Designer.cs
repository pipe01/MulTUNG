namespace Packet_Log_Viewer
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lvPackets = new System.Windows.Forms.ListView();
            this.colTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWay = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.pgPacket = new System.Windows.Forms.PropertyGrid();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkOut = new System.Windows.Forms.CheckBox();
            this.chkIn = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.lvPacketTypes = new System.Windows.Forms.ListView();
            this.panel3 = new System.Windows.Forms.Panel();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvPackets
            // 
            this.lvPackets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTitle,
            this.colTime,
            this.colWay});
            this.lvPackets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPackets.FullRowSelect = true;
            this.lvPackets.HideSelection = false;
            this.lvPackets.Location = new System.Drawing.Point(0, 0);
            this.lvPackets.MultiSelect = false;
            this.lvPackets.Name = "lvPackets";
            this.lvPackets.Size = new System.Drawing.Size(592, 398);
            this.lvPackets.TabIndex = 0;
            this.lvPackets.UseCompatibleStateImageBehavior = false;
            this.lvPackets.View = System.Windows.Forms.View.Details;
            this.lvPackets.SelectedIndexChanged += new System.EventHandler(this.lvPackets_SelectedIndexChanged);
            this.lvPackets.DoubleClick += new System.EventHandler(this.lvPackets_DoubleClick);
            // 
            // colTitle
            // 
            this.colTitle.Text = "Type";
            this.colTitle.Width = 202;
            // 
            // colTime
            // 
            this.colTime.Text = "Time";
            this.colTime.Width = 93;
            // 
            // colWay
            // 
            this.colWay.Text = "Way";
            this.colWay.Width = 100;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(978, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::Packet_Log_Viewer.Properties.Resources.directory_open;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Open log";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // pgPacket
            // 
            this.pgPacket.Dock = System.Windows.Forms.DockStyle.Right;
            this.pgPacket.Location = new System.Drawing.Point(602, 0);
            this.pgPacket.Name = "pgPacket";
            this.pgPacket.Size = new System.Drawing.Size(212, 398);
            this.pgPacket.TabIndex = 2;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Log files (*.log)|*.log|All files (*.*)|*.*";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.chkOut);
            this.panel1.Controls.Add(this.chkIn);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 432);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(954, 16);
            this.panel1.TabIndex = 3;
            // 
            // chkOut
            // 
            this.chkOut.AutoSize = true;
            this.chkOut.Checked = true;
            this.chkOut.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOut.Location = new System.Drawing.Point(72, -1);
            this.chkOut.Name = "chkOut";
            this.chkOut.Size = new System.Drawing.Size(41, 17);
            this.chkOut.TabIndex = 3;
            this.chkOut.Text = "out";
            this.chkOut.UseVisualStyleBackColor = true;
            this.chkOut.CheckedChanged += new System.EventHandler(this.chkHidePackets_CheckedChanged);
            // 
            // chkIn
            // 
            this.chkIn.AutoSize = true;
            this.chkIn.Checked = true;
            this.chkIn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIn.Location = new System.Drawing.Point(38, -1);
            this.chkIn.Name = "chkIn";
            this.chkIn.Size = new System.Drawing.Size(34, 17);
            this.chkIn.TabIndex = 2;
            this.chkIn.Text = "in";
            this.chkIn.UseVisualStyleBackColor = true;
            this.chkIn.CheckedChanged += new System.EventHandler(this.chkHidePackets_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Show";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lvPackets);
            this.panel2.Controls.Add(this.splitter1);
            this.panel2.Controls.Add(this.pgPacket);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(140, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(814, 398);
            this.panel2.TabIndex = 4;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(592, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(10, 398);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // lvPacketTypes
            // 
            this.lvPacketTypes.CheckBoxes = true;
            this.lvPacketTypes.Dock = System.Windows.Forms.DockStyle.Left;
            this.lvPacketTypes.Location = new System.Drawing.Point(0, 0);
            this.lvPacketTypes.Name = "lvPacketTypes";
            this.lvPacketTypes.Size = new System.Drawing.Size(130, 398);
            this.lvPacketTypes.TabIndex = 4;
            this.lvPacketTypes.UseCompatibleStateImageBehavior = false;
            this.lvPacketTypes.View = System.Windows.Forms.View.List;
            this.lvPacketTypes.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvPacketTypes_ItemChecked);
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.panel2);
            this.panel3.Controls.Add(this.splitter2);
            this.panel3.Controls.Add(this.lvPacketTypes);
            this.panel3.Location = new System.Drawing.Point(12, 28);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(954, 398);
            this.panel3.TabIndex = 5;
            // 
            // splitter2
            // 
            this.splitter2.Location = new System.Drawing.Point(130, 0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(10, 398);
            this.splitter2.TabIndex = 5;
            this.splitter2.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(978, 460);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panel3);
            this.Name = "Form1";
            this.Text = "Packet log viewer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lvPackets;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.PropertyGrid pgPacket;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ColumnHeader colTitle;
        private System.Windows.Forms.ColumnHeader colTime;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ColumnHeader colWay;
        private System.Windows.Forms.CheckBox chkOut;
        private System.Windows.Forms.CheckBox chkIn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ListView lvPacketTypes;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Splitter splitter2;
    }
}

