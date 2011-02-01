using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Tester
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            SteamContext.SteamFriends.FriendsList += ( obj, e ) =>
                {
                    this.Invoke( new MethodInvoker( () =>
                        {
                            lbUsers.Items.AddRange( e.List.ConvertAll<object>( ( input ) => { return ( object )input; } ).ToArray() );
                        } ) );
                };
        }
    }
}
