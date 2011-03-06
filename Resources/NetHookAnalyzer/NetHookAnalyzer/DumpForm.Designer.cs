namespace NetHookAnalyzer
{
    partial class DumpForm
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
            this.listPackets = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chBoxOut = new System.Windows.Forms.CheckBox();
            this.chBoxIn = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtSummary = new System.Windows.Forms.RichTextBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point( 0, 0 );
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add( this.listPackets );
            this.splitContainer1.Panel1.Controls.Add( this.panel1 );
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add( this.panel2 );
            this.splitContainer1.Size = new System.Drawing.Size( 650, 433 );
            this.splitContainer1.SplitterDistance = 224;
            this.splitContainer1.TabIndex = 0;
            // 
            // listPackets
            // 
            this.listPackets.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listPackets.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3} );
            this.listPackets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listPackets.FullRowSelect = true;
            this.listPackets.GridLines = true;
            this.listPackets.HideSelection = false;
            this.listPackets.Location = new System.Drawing.Point( 0, 47 );
            this.listPackets.Name = "listPackets";
            this.listPackets.Size = new System.Drawing.Size( 224, 386 );
            this.listPackets.TabIndex = 1;
            this.listPackets.UseCompatibleStateImageBehavior = false;
            this.listPackets.View = System.Windows.Forms.View.Details;
            this.listPackets.SelectedIndexChanged += new System.EventHandler( this.listPackets_SelectedIndexChanged );
            this.listPackets.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler( this.listPackets_ColumnClick );
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "#";
            this.columnHeader1.Width = 50;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Dir";
            this.columnHeader2.Width = 40;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Type";
            this.columnHeader3.Width = 125;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add( this.groupBox1 );
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point( 0, 0 );
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size( 224, 47 );
            this.panel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add( this.chBoxOut );
            this.groupBox1.Controls.Add( this.chBoxIn );
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point( 0, 0 );
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size( 222, 45 );
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filter";
            // 
            // chBoxOut
            // 
            this.chBoxOut.AutoSize = true;
            this.chBoxOut.Checked = true;
            this.chBoxOut.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chBoxOut.Location = new System.Drawing.Point( 53, 19 );
            this.chBoxOut.Name = "chBoxOut";
            this.chBoxOut.Size = new System.Drawing.Size( 43, 17 );
            this.chBoxOut.TabIndex = 1;
            this.chBoxOut.Text = "Out";
            this.chBoxOut.UseVisualStyleBackColor = true;
            this.chBoxOut.CheckedChanged += new System.EventHandler( this.filterCheckChanged );
            // 
            // chBoxIn
            // 
            this.chBoxIn.AutoSize = true;
            this.chBoxIn.Checked = true;
            this.chBoxIn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chBoxIn.Location = new System.Drawing.Point( 12, 19 );
            this.chBoxIn.Name = "chBoxIn";
            this.chBoxIn.Size = new System.Drawing.Size( 35, 17 );
            this.chBoxIn.TabIndex = 0;
            this.chBoxIn.Text = "In";
            this.chBoxIn.UseVisualStyleBackColor = true;
            this.chBoxIn.CheckedChanged += new System.EventHandler( this.filterCheckChanged );
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add( this.txtSummary );
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point( 0, 0 );
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size( 422, 433 );
            this.panel2.TabIndex = 0;
            // 
            // txtSummary
            // 
            this.txtSummary.BackColor = System.Drawing.SystemColors.Window;
            this.txtSummary.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSummary.Location = new System.Drawing.Point( 0, 0 );
            this.txtSummary.Name = "txtSummary";
            this.txtSummary.ReadOnly = true;
            this.txtSummary.Size = new System.Drawing.Size( 420, 431 );
            this.txtSummary.TabIndex = 0;
            this.txtSummary.Text = "";
            // 
            // DumpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 650, 433 );
            this.Controls.Add( this.splitContainer1 );
            this.Name = "DumpForm";
            this.Text = "DumpForm";
            this.splitContainer1.Panel1.ResumeLayout( false );
            this.splitContainer1.Panel2.ResumeLayout( false );
            this.splitContainer1.ResumeLayout( false );
            this.panel1.ResumeLayout( false );
            this.groupBox1.ResumeLayout( false );
            this.groupBox1.PerformLayout();
            this.panel2.ResumeLayout( false );
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chBoxOut;
        private System.Windows.Forms.CheckBox chBoxIn;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RichTextBox txtSummary;
        private System.Windows.Forms.ListView listPackets;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}