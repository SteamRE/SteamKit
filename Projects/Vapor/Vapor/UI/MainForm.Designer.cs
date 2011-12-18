namespace Vapor
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.friendsFlow = new Vapor.FriendsListControl();
            this.vaporContextMenu1 = new Vapor.VaporContextMenu();
            this.addFriendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddFriend = new Vapor.VaporButton();
            this.stateComboBox = new Vapor.VaporComboBox();
            this.selfControl = new Vapor.FriendControl();
            this.vaporContextMenu3 = new Vapor.VaporContextMenu();
            this.changeNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.vaporContextMenu2 = new Vapor.VaporContextMenu();
            this.showHideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.vaporContextMenu1.SuspendLayout();
            this.vaporContextMenu3.SuspendLayout();
            this.vaporContextMenu2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.stateComboBox);
            this.panel1.Controls.Add(this.selfControl);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(210, 55);
            this.panel1.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.btnAddFriend);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 442);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(210, 28);
            this.panel3.TabIndex = 1;
            // 
            // friendsFlow
            // 
            this.friendsFlow.AutoScroll = true;
            this.friendsFlow.ContextMenuStrip = this.vaporContextMenu1;
            this.friendsFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.friendsFlow.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.friendsFlow.Location = new System.Drawing.Point(0, 55);
            this.friendsFlow.Name = "friendsFlow";
            this.friendsFlow.Size = new System.Drawing.Size(210, 387);
            this.friendsFlow.TabIndex = 0;
            this.friendsFlow.WrapContents = false;
            // 
            // vaporContextMenu1
            // 
            this.vaporContextMenu1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(39)))));
            this.vaporContextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addFriendToolStripMenuItem,
            this.toolStripMenuItem1,
            this.refreshToolStripMenuItem});
            this.vaporContextMenu1.Name = "vaporContextMenu1";
            this.vaporContextMenu1.ShowImageMargin = false;
            this.vaporContextMenu1.Size = new System.Drawing.Size(117, 54);
            // 
            // addFriendToolStripMenuItem
            // 
            this.addFriendToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.addFriendToolStripMenuItem.Name = "addFriendToolStripMenuItem";
            this.addFriendToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.addFriendToolStripMenuItem.Text = "Add Friend...";
            this.addFriendToolStripMenuItem.Click += new System.EventHandler(this.addFriendToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(124, 6);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshListToolStripMenuItem_Click);
            // 
            // btnAddFriend
            // 
            this.btnAddFriend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.btnAddFriend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddFriend.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnAddFriend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddFriend.ForeColor = System.Drawing.Color.White;
            this.btnAddFriend.Location = new System.Drawing.Point(0, 0);
            this.btnAddFriend.Name = "btnAddFriend";
            this.btnAddFriend.Size = new System.Drawing.Size(208, 26);
            this.btnAddFriend.TabIndex = 0;
            this.btnAddFriend.Text = "Add Friend...";
            this.btnAddFriend.UseVisualStyleBackColor = true;
            this.btnAddFriend.Click += new System.EventHandler(this.btnAddFriend_Click);
            // 
            // stateComboBox
            // 
            this.stateComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stateComboBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.stateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.stateComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.stateComboBox.ForeColor = System.Drawing.Color.White;
            this.stateComboBox.FormattingEnabled = true;
            this.stateComboBox.Items.AddRange(new object[] {
            "Online",
            "Away",
            "Busy",
            "Snooze",
            "Offline"});
            this.stateComboBox.Location = new System.Drawing.Point(53, 24);
            this.stateComboBox.Name = "stateComboBox";
            this.stateComboBox.Size = new System.Drawing.Size(152, 21);
            this.stateComboBox.TabIndex = 1;
            this.stateComboBox.SelectedIndexChanged += new System.EventHandler(this.stateComboBox_SelectedIndexChanged);
            // 
            // selfControl
            // 
            this.selfControl.AvatarHash = null;
            this.selfControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(39)))));
            this.selfControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.selfControl.CanOpenProfile = false;
            this.selfControl.ContextMenuStrip = this.vaporContextMenu3;
            this.selfControl.IsHighlighted = true;
            this.selfControl.Location = new System.Drawing.Point(3, 3);
            this.selfControl.Name = "selfControl";
            this.selfControl.Size = new System.Drawing.Size(204, 46);
            this.selfControl.TabIndex = 0;
            // 
            // vaporContextMenu3
            // 
            this.vaporContextMenu3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(39)))));
            this.vaporContextMenu3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeNameToolStripMenuItem});
            this.vaporContextMenu3.Name = "vaporContextMenu3";
            this.vaporContextMenu3.ShowImageMargin = false;
            this.vaporContextMenu3.Size = new System.Drawing.Size(128, 48);
            // 
            // changeNameToolStripMenuItem
            // 
            this.changeNameToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.changeNameToolStripMenuItem.Name = "changeNameToolStripMenuItem";
            this.changeNameToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.changeNameToolStripMenuItem.Text = "Change Name";
            this.changeNameToolStripMenuItem.Click += new System.EventHandler(this.changeNameToolStripMenuItem_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.vaporContextMenu2;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Vapor";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // vaporContextMenu2
            // 
            this.vaporContextMenu2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(39)))));
            this.vaporContextMenu2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showHideToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
            this.vaporContextMenu2.Name = "vaporContextMenu2";
            this.vaporContextMenu2.ShowImageMargin = false;
            this.vaporContextMenu2.Size = new System.Drawing.Size(117, 54);
            // 
            // showHideToolStripMenuItem
            // 
            this.showHideToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.showHideToolStripMenuItem.Name = "showHideToolStripMenuItem";
            this.showHideToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.showHideToolStripMenuItem.Text = "[Show/Hide]";
            this.showHideToolStripMenuItem.Click += new System.EventHandler(this.showHideToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(113, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(39)))));
            this.ClientSize = new System.Drawing.Size(210, 470);
            this.Controls.Add(this.friendsFlow);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(16, 200);
            this.Name = "MainForm";
            this.Text = "Vapor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.VisibleChanged += new System.EventHandler(this.MainForm_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.vaporContextMenu1.ResumeLayout(false);
            this.vaporContextMenu3.ResumeLayout(false);
            this.vaporContextMenu2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private FriendControl selfControl;
        private FriendsListControl friendsFlow;
        private System.Windows.Forms.Panel panel3;
        private VaporButton btnAddFriend;
        private VaporContextMenu vaporContextMenu1;
        private System.Windows.Forms.ToolStripMenuItem addFriendToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private VaporContextMenu vaporContextMenu2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showHideToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private VaporComboBox stateComboBox;
        private VaporContextMenu vaporContextMenu3;
        private System.Windows.Forms.ToolStripMenuItem changeNameToolStripMenuItem;
    }
}