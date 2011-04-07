namespace Vapor
{
    partial class SteamGuardDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( SteamGuardDialog ) );
            this.btnOk = new Vapor.VaporButton();
            this.txtAuthCode = new Vapor.VaporTextBox();
            this.vaporLabel1 = new Vapor.VaporLabel();
            this.vaporLabel2 = new Vapor.VaporLabel();
            this.btnCancel = new Vapor.VaporButton();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location = new System.Drawing.Point( 154, 130 );
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size( 75, 23 );
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // txtAuthCode
            // 
            this.txtAuthCode.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.txtAuthCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAuthCode.Font = new System.Drawing.Font( "Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte )( 0 ) ) );
            this.txtAuthCode.ForeColor = System.Drawing.Color.White;
            this.txtAuthCode.Location = new System.Drawing.Point( 116, 68 );
            this.txtAuthCode.Name = "txtAuthCode";
            this.txtAuthCode.Size = new System.Drawing.Size( 113, 35 );
            this.txtAuthCode.TabIndex = 1;
            // 
            // vaporLabel1
            // 
            this.vaporLabel1.ForeColor = System.Drawing.Color.White;
            this.vaporLabel1.Location = new System.Drawing.Point( 12, 9 );
            this.vaporLabel1.Name = "vaporLabel1";
            this.vaporLabel1.Size = new System.Drawing.Size( 217, 47 );
            this.vaporLabel1.TabIndex = 2;
            this.vaporLabel1.Text = "This account is protected by Steam Guard. Please enter the authentication code se" +
                "nt to your email address.";
            this.vaporLabel1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // vaporLabel2
            // 
            this.vaporLabel2.AutoSize = true;
            this.vaporLabel2.Font = new System.Drawing.Font( "Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte )( 0 ) ) );
            this.vaporLabel2.ForeColor = System.Drawing.Color.White;
            this.vaporLabel2.Location = new System.Drawing.Point( 12, 77 );
            this.vaporLabel2.Name = "vaporLabel2";
            this.vaporLabel2.Size = new System.Drawing.Size( 89, 20 );
            this.vaporLabel2.TabIndex = 3;
            this.vaporLabel2.Text = "Auth Code:";
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point( 12, 130 );
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size( 75, 23 );
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // SteamGuardDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 241, 165 );
            this.Controls.Add( this.btnCancel );
            this.Controls.Add( this.vaporLabel2 );
            this.Controls.Add( this.vaporLabel1 );
            this.Controls.Add( this.txtAuthCode );
            this.Controls.Add( this.btnOk );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ( ( System.Drawing.Icon )( resources.GetObject( "$this.Icon" ) ) );
            this.MaximizeBox = false;
            this.Name = "SteamGuardDialog";
            this.Text = "Steam Guard";
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private VaporButton btnOk;
        private VaporTextBox txtAuthCode;
        private VaporLabel vaporLabel1;
        private VaporLabel vaporLabel2;
        private VaporButton btnCancel;

    }
}