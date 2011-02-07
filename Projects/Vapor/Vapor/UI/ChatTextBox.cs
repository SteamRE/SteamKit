using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Vapor
{
    public partial class ChatTextBox : RichTextBox
    {
        public ChatTextBox()
        {
            this.BackColor = Color.FromArgb( 38, 38, 39 );
            this.ForeColor = Color.White;
            this.BorderStyle = BorderStyle.None;
        }


        public event EventHandler EnterPressed;
        protected virtual void OnEnterPressed( EventArgs e )
        {
            if ( EnterPressed != null )
                EnterPressed( this, e );
        }

        protected override void OnKeyPress( KeyPressEventArgs e )
        {
            if ( e.KeyChar == ( char )13 )
            {
                this.Text = this.Text.TrimEnd( Environment.NewLine.ToCharArray() );

                OnEnterPressed( EventArgs.Empty );
                e.Handled = true;
                return;
            }

            base.OnKeyPress( e );
        }
    }
}
