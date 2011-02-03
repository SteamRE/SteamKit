namespace Tester
{
    partial class TraceDialog
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
            this.txtTrace = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtTrace
            // 
            this.txtTrace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTrace.Font = new System.Drawing.Font( "Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte )( 0 ) ) );
            this.txtTrace.Location = new System.Drawing.Point( 0, 0 );
            this.txtTrace.Multiline = true;
            this.txtTrace.Name = "txtTrace";
            this.txtTrace.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTrace.Size = new System.Drawing.Size( 433, 613 );
            this.txtTrace.TabIndex = 0;
            // 
            // TraceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 433, 613 );
            this.Controls.Add( this.txtTrace );
            this.Name = "TraceDialog";
            this.Text = "TraceDialog";
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtTrace;
    }
}