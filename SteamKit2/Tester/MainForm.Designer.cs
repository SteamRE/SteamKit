namespace Tester
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
            this.lbUsers = new Tester.RefreshableListBox();
            this.SuspendLayout();
            // 
            // lbUsers
            // 
            this.lbUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbUsers.FormattingEnabled = true;
            this.lbUsers.IntegralHeight = false;
            this.lbUsers.Location = new System.Drawing.Point( 0, 0 );
            this.lbUsers.Name = "lbUsers";
            this.lbUsers.Size = new System.Drawing.Size( 274, 455 );
            this.lbUsers.TabIndex = 0;
            this.lbUsers.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler( this.lbUsers_MouseDoubleClick );
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 274, 455 );
            this.Controls.Add( this.lbUsers );
            this.Name = "MainForm";
            this.Text = "Friends List";
            this.ResumeLayout( false );

        }

        #endregion

        private RefreshableListBox lbUsers;

    }
}