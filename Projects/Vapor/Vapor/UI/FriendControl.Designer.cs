namespace Vapor
{
    partial class FriendControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.avatarBox = new System.Windows.Forms.PictureBox();
            this.nameLbl = new System.Windows.Forms.Label();
            this.statusLbl = new System.Windows.Forms.Label();
            this.gameLbl = new System.Windows.Forms.Label();
            this.btnDeny = new Vapor.VaporButton();
            this.btnAccept = new Vapor.VaporButton();
            this.vaporContextMenu1 = new Vapor.VaporContextMenu();
            this.removeFriendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.addFriendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ( ( System.ComponentModel.ISupportInitialize )( this.avatarBox ) ).BeginInit();
            this.vaporContextMenu1.SuspendLayout();
            this.SuspendLayout();
            // 
            // avatarBox
            // 
            this.avatarBox.Location = new System.Drawing.Point( 3, 3 );
            this.avatarBox.Name = "avatarBox";
            this.avatarBox.Size = new System.Drawing.Size( 40, 40 );
            this.avatarBox.TabIndex = 0;
            this.avatarBox.TabStop = false;
            // 
            // nameLbl
            // 
            this.nameLbl.AutoSize = true;
            this.nameLbl.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( ( byte )( 0 ) ) );
            this.nameLbl.Location = new System.Drawing.Point( 49, 3 );
            this.nameLbl.Name = "nameLbl";
            this.nameLbl.Size = new System.Drawing.Size( 65, 16 );
            this.nameLbl.TabIndex = 1;
            this.nameLbl.Text = "<Name>";
            // 
            // statusLbl
            // 
            this.statusLbl.AutoSize = true;
            this.statusLbl.Font = new System.Drawing.Font( "Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte )( 0 ) ) );
            this.statusLbl.Location = new System.Drawing.Point( 49, 19 );
            this.statusLbl.Name = "statusLbl";
            this.statusLbl.Size = new System.Drawing.Size( 42, 12 );
            this.statusLbl.TabIndex = 2;
            this.statusLbl.Text = "<Status>";
            // 
            // gameLbl
            // 
            this.gameLbl.AutoSize = true;
            this.gameLbl.Font = new System.Drawing.Font( "Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte )( 0 ) ) );
            this.gameLbl.Location = new System.Drawing.Point( 49, 31 );
            this.gameLbl.Name = "gameLbl";
            this.gameLbl.Size = new System.Drawing.Size( 40, 12 );
            this.gameLbl.TabIndex = 3;
            this.gameLbl.Text = "<Game>";
            // 
            // btnDeny
            // 
            this.btnDeny.Anchor = ( ( System.Windows.Forms.AnchorStyles )( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnDeny.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 39 ) ) ) ) );
            this.btnDeny.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnDeny.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeny.ForeColor = System.Drawing.Color.White;
            this.btnDeny.Location = new System.Drawing.Point( 157, 3 );
            this.btnDeny.Name = "btnDeny";
            this.btnDeny.Size = new System.Drawing.Size( 22, 40 );
            this.btnDeny.TabIndex = 5;
            this.btnDeny.Text = "-";
            this.btnDeny.UseVisualStyleBackColor = false;
            this.btnDeny.Click += new System.EventHandler( this.btnDeny_Click );
            // 
            // btnAccept
            // 
            this.btnAccept.Anchor = ( ( System.Windows.Forms.AnchorStyles )( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnAccept.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 39 ) ) ) ) );
            this.btnAccept.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnAccept.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAccept.ForeColor = System.Drawing.Color.White;
            this.btnAccept.Location = new System.Drawing.Point( 129, 3 );
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size( 22, 40 );
            this.btnAccept.TabIndex = 4;
            this.btnAccept.Text = "+";
            this.btnAccept.UseVisualStyleBackColor = false;
            this.btnAccept.Click += new System.EventHandler( this.btnAccept_Click );
            // 
            // vaporContextMenu1
            // 
            this.vaporContextMenu1.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 38 ) ) ) ), ( ( int )( ( ( byte )( 39 ) ) ) ) );
            this.vaporContextMenu1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.removeFriendToolStripMenuItem,
            this.toolStripMenuItem1,
            this.addFriendToolStripMenuItem,
            this.toolStripMenuItem2,
            this.refreshToolStripMenuItem} );
            this.vaporContextMenu1.Name = "vaporContextMenu1";
            this.vaporContextMenu1.ShowImageMargin = false;
            this.vaporContextMenu1.Size = new System.Drawing.Size( 129, 82 );
            // 
            // removeFriendToolStripMenuItem
            // 
            this.removeFriendToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.removeFriendToolStripMenuItem.Name = "removeFriendToolStripMenuItem";
            this.removeFriendToolStripMenuItem.Size = new System.Drawing.Size( 128, 22 );
            this.removeFriendToolStripMenuItem.Text = "Remove Friend";
            this.removeFriendToolStripMenuItem.Click += new System.EventHandler( this.removeFriendToolStripMenuItem_Click );
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size( 125, 6 );
            // 
            // addFriendToolStripMenuItem
            // 
            this.addFriendToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.addFriendToolStripMenuItem.Name = "addFriendToolStripMenuItem";
            this.addFriendToolStripMenuItem.Size = new System.Drawing.Size( 128, 22 );
            this.addFriendToolStripMenuItem.Text = "Add Friend";
            this.addFriendToolStripMenuItem.Click += new System.EventHandler( this.addFriendToolStripMenuItem_Click );
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size( 125, 6 );
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size( 128, 22 );
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler( this.refreshToolStripMenuItem_Click );
            // 
            // FriendControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ContextMenuStrip = this.vaporContextMenu1;
            this.Controls.Add( this.btnDeny );
            this.Controls.Add( this.btnAccept );
            this.Controls.Add( this.gameLbl );
            this.Controls.Add( this.statusLbl );
            this.Controls.Add( this.nameLbl );
            this.Controls.Add( this.avatarBox );
            this.Name = "FriendControl";
            this.Size = new System.Drawing.Size( 184, 48 );
            ( ( System.ComponentModel.ISupportInitialize )( this.avatarBox ) ).EndInit();
            this.vaporContextMenu1.ResumeLayout( false );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox avatarBox;
        private System.Windows.Forms.Label nameLbl;
        private System.Windows.Forms.Label statusLbl;
        private System.Windows.Forms.Label gameLbl;
        private VaporButton btnAccept;
        private VaporButton btnDeny;
        private VaporContextMenu vaporContextMenu1;
        private System.Windows.Forms.ToolStripMenuItem removeFriendToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem addFriendToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
    }
}
