using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Vapor
{
    partial class SteamGuardDialog : VaporForm
    {
        public string AuthCode
        {
            get { return txtAuthCode.Text; }
        }


        public SteamGuardDialog()
        {
            InitializeComponent();
        }
    }
}
