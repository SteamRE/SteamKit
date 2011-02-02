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
            this.txtTrace.Location = new System.Drawing.Point( 12, 12 );
            this.txtTrace.Multiline = true;
            this.txtTrace.Name = "txtTrace";
            this.txtTrace.Size = new System.Drawing.Size( 338, 297 );
            this.txtTrace.TabIndex = 0;
            // 
            // TraceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 362, 321 );
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