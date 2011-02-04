using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Tester
{
    public partial class ChatTextBox : RichTextBox
    {
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
