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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( MainForm ) );
            this.panel1 = new System.Windows.Forms.Panel();
            this.selfControl = new Vapor.FriendControl();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnAddFriend = new Vapor.VaporButton();
            this.friendsFlow = new Vapor.FriendsListControl();
            this.vaporContextMenu1 = new Vapor.VaporContextMenu();
            this.addFriendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.vaporContextMenu1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add( this.selfControl );
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point( 0, 0 );
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size( 210, 56 );
            this.panel1.TabIndex = 0;
            // 
            // selfControl
            // 
            this.selfControl.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 39 ) ) ) ) );
            this.selfControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.selfControl.IsHighlighted = true;
            this.selfControl.Location = new System.Drawing.Point( 3, 3 );
            this.selfControl.Name = "selfControl";
            this.selfControl.Size = new System.Drawing.Size( 204, 46 );
            this.selfControl.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add( this.btnAddFriend );
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point( 0, 442 );
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size( 210, 28 );
            this.panel3.TabIndex = 1;
            // 
            // btnAddFriend
            // 
            this.btnAddFriend.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.btnAddFriend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddFriend.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnAddFriend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddFriend.ForeColor = System.Drawing.Color.White;
            this.btnAddFriend.Location = new System.Drawing.Point( 0, 0 );
            this.btnAddFriend.Name = "btnAddFriend";
            this.btnAddFriend.Size = new System.Drawing.Size( 208, 26 );
            this.btnAddFriend.TabIndex = 0;
            this.btnAddFriend.Text = "Add Friend...";
            this.btnAddFriend.UseVisualStyleBackColor = true;
            this.btnAddFriend.Click += new System.EventHandler( this.btnAddFriend_Click );
            // 
            // friendsFlow
            // 
            this.friendsFlow.AutoScroll = true;
            this.friendsFlow.ContextMenuStrip = this.vaporContextMenu1;
            this.friendsFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.friendsFlow.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.friendsFlow.Location = new System.Drawing.Point( 0, 56 );
            this.friendsFlow.Name = "friendsFlow";
            this.friendsFlow.Size = new System.Drawing.Size( 210, 386 );
            this.friendsFlow.TabIndex = 0;
            this.friendsFlow.WrapContents = false;
            // 
            // vaporContextMenu1
            // 
            this.vaporContextMenu1.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 39 ) ) ) ) );
            this.vaporContextMenu1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.addFriendToolStripMenuItem,
            this.toolStripMenuItem1,
            this.refreshToolStripMenuItem} );
            this.vaporContextMenu1.Name = "vaporContextMenu1";
            this.vaporContextMenu1.ShowImageMargin = false;
            this.vaporContextMenu1.Size = new System.Drawing.Size( 128, 76 );
            // 
            // addFriendToolStripMenuItem
            // 
            this.addFriendToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.addFriendToolStripMenuItem.Name = "addFriendToolStripMenuItem";
            this.addFriendToolStripMenuItem.Size = new System.Drawing.Size( 127, 22 );
            this.addFriendToolStripMenuItem.Text = "Add Friend";
            this.addFriendToolStripMenuItem.Click += new System.EventHandler( this.addFriendToolStripMenuItem_Click );
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size( 124, 6 );
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size( 127, 22 );
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler( this.refreshListToolStripMenuItem_Click );
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 39 ) ) ) ) );
            this.ClientSize = new System.Drawing.Size( 210, 470 );
            this.Controls.Add( this.friendsFlow );
            this.Controls.Add( this.panel3 );
            this.Controls.Add( this.panel1 );
            this.Icon = ( ( System.Drawing.Icon )( resources.GetObject( "$this.Icon" ) ) );
            this.MinimumSize = new System.Drawing.Size( 8, 200 );
            this.Name = "MainForm";
            this.Text = "Vapor";
            this.Resize += new System.EventHandler( this.MainForm_Resize );
            this.panel1.ResumeLayout( false );
            this.panel3.ResumeLayout( false );
            this.vaporContextMenu1.ResumeLayout( false );
            this.ResumeLayout( false );

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
    }
}