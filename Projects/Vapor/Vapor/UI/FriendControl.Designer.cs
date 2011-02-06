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
            ( ( System.ComponentModel.ISupportInitialize )( this.avatarBox ) ).BeginInit();
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
            // FriendControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add( this.gameLbl );
            this.Controls.Add( this.statusLbl );
            this.Controls.Add( this.nameLbl );
            this.Controls.Add( this.avatarBox );
            this.Name = "FriendControl";
            this.Size = new System.Drawing.Size( 184, 48 );
            ( ( System.ComponentModel.ISupportInitialize )( this.avatarBox ) ).EndInit();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox avatarBox;
        private System.Windows.Forms.Label nameLbl;
        private System.Windows.Forms.Label statusLbl;
        private System.Windows.Forms.Label gameLbl;
    }
}
