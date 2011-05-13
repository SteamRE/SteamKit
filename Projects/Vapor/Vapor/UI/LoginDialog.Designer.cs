namespace Vapor
{
    partial class LoginDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( LoginDialog ) );
            this.btnLogin = new Vapor.VaporButton();
            this.vaporGroupBox1 = new Vapor.VaporGroupBox();
            this.vaporLabel2 = new Vapor.VaporLabel();
            this.txtPass = new Vapor.VaporTextBox();
            this.vaporLabel1 = new Vapor.VaporLabel();
            this.txtUser = new Vapor.VaporTextBox();
            this.btnCancel = new Vapor.VaporButton();
            this.chkBoxAltLogon = new Vapor.VaporCheckBox();
            this.vaporGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLogin
            // 
            this.btnLogin.Anchor = ( ( System.Windows.Forms.AnchorStyles )( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.btnLogin.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Location = new System.Drawing.Point( 260, 94 );
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size( 75, 23 );
            this.btnLogin.TabIndex = 0;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler( this.btnLogin_Click );
            // 
            // vaporGroupBox1
            // 
            this.vaporGroupBox1.Anchor = ( ( System.Windows.Forms.AnchorStyles )( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.vaporGroupBox1.Controls.Add( this.vaporLabel2 );
            this.vaporGroupBox1.Controls.Add( this.txtPass );
            this.vaporGroupBox1.Controls.Add( this.vaporLabel1 );
            this.vaporGroupBox1.Controls.Add( this.txtUser );
            this.vaporGroupBox1.ForeColor = System.Drawing.Color.White;
            this.vaporGroupBox1.Location = new System.Drawing.Point( 12, 12 );
            this.vaporGroupBox1.Name = "vaporGroupBox1";
            this.vaporGroupBox1.Size = new System.Drawing.Size( 323, 76 );
            this.vaporGroupBox1.TabIndex = 1;
            this.vaporGroupBox1.TabStop = false;
            this.vaporGroupBox1.Text = "Login";
            // 
            // vaporLabel2
            // 
            this.vaporLabel2.AutoSize = true;
            this.vaporLabel2.ForeColor = System.Drawing.Color.White;
            this.vaporLabel2.Location = new System.Drawing.Point( 8, 47 );
            this.vaporLabel2.Name = "vaporLabel2";
            this.vaporLabel2.Size = new System.Drawing.Size( 56, 13 );
            this.vaporLabel2.TabIndex = 3;
            this.vaporLabel2.Text = "Password:";
            // 
            // txtPass
            // 
            this.txtPass.Anchor = ( ( System.Windows.Forms.AnchorStyles )( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.txtPass.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.txtPass.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPass.ForeColor = System.Drawing.Color.White;
            this.txtPass.Location = new System.Drawing.Point( 70, 45 );
            this.txtPass.Name = "txtPass";
            this.txtPass.PasswordChar = '*';
            this.txtPass.Size = new System.Drawing.Size( 247, 20 );
            this.txtPass.TabIndex = 2;
            // 
            // vaporLabel1
            // 
            this.vaporLabel1.AutoSize = true;
            this.vaporLabel1.ForeColor = System.Drawing.Color.White;
            this.vaporLabel1.Location = new System.Drawing.Point( 6, 21 );
            this.vaporLabel1.Name = "vaporLabel1";
            this.vaporLabel1.Size = new System.Drawing.Size( 58, 13 );
            this.vaporLabel1.TabIndex = 1;
            this.vaporLabel1.Text = "Username:";
            // 
            // txtUser
            // 
            this.txtUser.Anchor = ( ( System.Windows.Forms.AnchorStyles )( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.txtUser.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUser.ForeColor = System.Drawing.Color.White;
            this.txtUser.Location = new System.Drawing.Point( 70, 19 );
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size( 247, 20 );
            this.txtUser.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ( ( System.Windows.Forms.AnchorStyles )( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb( ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ), ( ( int )( ( ( byte )( 58 ) ) ) ) );
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point( 12, 94 );
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size( 75, 23 );
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // chkBoxAltLogon
            // 
            this.chkBoxAltLogon.Anchor = ( ( System.Windows.Forms.AnchorStyles )( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.chkBoxAltLogon.AutoSize = true;
            this.chkBoxAltLogon.ForeColor = System.Drawing.Color.White;
            this.chkBoxAltLogon.Location = new System.Drawing.Point( 153, 98 );
            this.chkBoxAltLogon.Name = "chkBoxAltLogon";
            this.chkBoxAltLogon.Size = new System.Drawing.Size( 101, 17 );
            this.chkBoxAltLogon.TabIndex = 3;
            this.chkBoxAltLogon.Text = "Alternate Logon";
            this.chkBoxAltLogon.UseVisualStyleBackColor = true;
            // 
            // LoginDialog
            // 
            this.AcceptButton = this.btnLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size( 347, 129 );
            this.Controls.Add( this.chkBoxAltLogon );
            this.Controls.Add( this.btnCancel );
            this.Controls.Add( this.vaporGroupBox1 );
            this.Controls.Add( this.btnLogin );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ( ( System.Drawing.Icon )( resources.GetObject( "$this.Icon" ) ) );
            this.MaximizeBox = false;
            this.Name = "LoginDialog";
            this.Text = "Vapor - Login";
            this.vaporGroupBox1.ResumeLayout( false );
            this.vaporGroupBox1.PerformLayout();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private VaporButton btnLogin;
        private VaporGroupBox vaporGroupBox1;
        private VaporTextBox txtUser;
        private VaporLabel vaporLabel1;
        private VaporTextBox txtPass;
        private VaporLabel vaporLabel2;
        private VaporButton btnCancel;
        private VaporCheckBox chkBoxAltLogon;

    }
}