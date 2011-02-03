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

        protected override bool ProcessCmdKey( ref Message m, Keys keyData )
        {
            if ( m.Msg == 256 && keyData == Keys.Enter )
            {
                OnEnterPressed( EventArgs.Empty );

                return true;
            }

            return base.ProcessCmdKey( ref m, keyData );
        }
    }
}
