namespace NetHookAnalyzer
{
    partial class SessionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.viewPacket = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkOut = new System.Windows.Forms.CheckBox();
            this.chkIn = new System.Windows.Forms.CheckBox();
            this.treePacket = new System.Windows.Forms.TreeView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.viewPacket);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.treePacket);
            this.splitContainer1.Size = new System.Drawing.Size(731, 571);
            this.splitContainer1.SplitterDistance = 276;
            this.splitContainer1.TabIndex = 0;
            // 
            // viewPacket
            // 
            this.viewPacket.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.viewPacket.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewPacket.FullRowSelect = true;
            this.viewPacket.GridLines = true;
            this.viewPacket.HideSelection = false;
            this.viewPacket.Location = new System.Drawing.Point(0, 47);
            this.viewPacket.Name = "viewPacket";
            this.viewPacket.Size = new System.Drawing.Size(276, 524);
            this.viewPacket.TabIndex = 1;
            this.viewPacket.UseCompatibleStateImageBehavior = false;
            this.viewPacket.View = System.Windows.Forms.View.Details;
            this.viewPacket.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.viewPacket_ColumnClick);
            this.viewPacket.SelectedIndexChanged += new System.EventHandler(this.viewPacket_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "#";
            this.columnHeader1.Width = 30;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Dir";
            this.columnHeader2.Width = 50;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Type";
            this.columnHeader3.Width = 185;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkOut);
            this.groupBox1.Controls.Add(this.chkIn);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(276, 47);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filter";
            // 
            // chkOut
            // 
            this.chkOut.AutoSize = true;
            this.chkOut.Checked = true;
            this.chkOut.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOut.Location = new System.Drawing.Point(53, 19);
            this.chkOut.Name = "chkOut";
            this.chkOut.Size = new System.Drawing.Size(43, 17);
            this.chkOut.TabIndex = 1;
            this.chkOut.Text = "Out";
            this.chkOut.UseVisualStyleBackColor = true;
            this.chkOut.CheckedChanged += new System.EventHandler(this.chkOut_CheckedChanged);
            // 
            // chkIn
            // 
            this.chkIn.AutoSize = true;
            this.chkIn.Checked = true;
            this.chkIn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIn.Location = new System.Drawing.Point(12, 19);
            this.chkIn.Name = "chkIn";
            this.chkIn.Size = new System.Drawing.Size(35, 17);
            this.chkIn.TabIndex = 0;
            this.chkIn.Text = "In";
            this.chkIn.UseVisualStyleBackColor = true;
            this.chkIn.CheckedChanged += new System.EventHandler(this.chkOut_CheckedChanged);
            // 
            // treePacket
            // 
            this.treePacket.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treePacket.FullRowSelect = true;
            this.treePacket.HideSelection = false;
            this.treePacket.Location = new System.Drawing.Point(0, 0);
            this.treePacket.Name = "treePacket";
            this.treePacket.Size = new System.Drawing.Size(451, 571);
            this.treePacket.TabIndex = 0;
            // 
            // SessionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 571);
            this.Controls.Add(this.splitContainer1);
            this.Name = "SessionForm";
            this.Text = "SessionForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treePacket;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView viewPacket;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.CheckBox chkIn;
        private System.Windows.Forms.CheckBox chkOut;
    }
}