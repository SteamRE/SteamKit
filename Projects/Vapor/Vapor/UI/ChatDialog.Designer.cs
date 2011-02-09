namespace Vapor
{
    partial class ChatDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( ChatDialog ) );
            this.panel1 = new System.Windows.Forms.Panel();
            this.chatFriend = new Vapor.FriendControl();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtLog = new Vapor.VaporRichTextBox();
            this.txtChat = new Vapor.ChatTextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add( this.chatFriend );
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point( 0, 0 );
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size( 292, 56 );
            this.panel1.TabIndex = 0;
            // 
            // chatFriend
            // 
            this.chatFriend.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 39 ) ) ) ) );
            this.chatFriend.IsHighlighted = true;
            this.chatFriend.Location = new System.Drawing.Point( 3, 3 );
            this.chatFriend.Name = "chatFriend";
            this.chatFriend.Size = new System.Drawing.Size( 184, 48 );
            this.chatFriend.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point( 3, 0 );
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add( this.txtLog );
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add( this.txtChat );
            this.splitContainer1.Size = new System.Drawing.Size( 286, 211 );
            this.splitContainer1.SplitterDistance = 145;
            this.splitContainer1.TabIndex = 1;
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLog.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.ForeColor = System.Drawing.Color.White;
            this.txtLog.Location = new System.Drawing.Point( 0, 0 );
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size( 284, 143 );
            this.txtLog.TabIndex = 0;
            this.txtLog.Text = "";
            this.txtLog.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler( this.txtLog_LinkClicked );
            // 
            // txtChat
            // 
            this.txtChat.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.txtChat.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtChat.DetectUrls = false;
            this.txtChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtChat.ForeColor = System.Drawing.Color.White;
            this.txtChat.Location = new System.Drawing.Point( 0, 0 );
            this.txtChat.Name = "txtChat";
            this.txtChat.Size = new System.Drawing.Size( 284, 60 );
            this.txtChat.TabIndex = 0;
            this.txtChat.Text = "";
            this.txtChat.EnterPressed += new System.EventHandler( this.txtChat_EnterPressed );
            // 
            // panel2
            // 
            this.panel2.Controls.Add( this.splitContainer1 );
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point( 0, 56 );
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding( 3, 0, 3, 3 );
            this.panel2.Size = new System.Drawing.Size( 292, 214 );
            this.panel2.TabIndex = 1;
            // 
            // ChatDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 292, 270 );
            this.Controls.Add( this.panel2 );
            this.Controls.Add( this.panel1 );
            this.Icon = ( ( System.Drawing.Icon )( resources.GetObject( "$this.Icon" ) ) );
            this.Name = "ChatDialog";
            this.Text = "Chat";
            this.panel1.ResumeLayout( false );
            this.splitContainer1.Panel1.ResumeLayout( false );
            this.splitContainer1.Panel2.ResumeLayout( false );
            this.splitContainer1.ResumeLayout( false );
            this.panel2.ResumeLayout( false );
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private FriendControl chatFriend;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private ChatTextBox txtChat;
        private VaporRichTextBox txtLog;
        private System.Windows.Forms.Panel panel2;
    }
}