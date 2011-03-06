using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace NetHookAnalyzer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            fbd.Description = "Please select a nethook dump directory to load...";
            fbd.ShowNewFolderButton = false;

            if ( fbd.ShowDialog() != DialogResult.OK )
                return;

            DirectoryInfo di = new DirectoryInfo( fbd.SelectedPath );

            FileInfo[] fileList = di.GetFiles( "*.bin", SearchOption.TopDirectoryOnly );

            if ( fileList.Length == 0 )
            {
                Utils.MsgBox( this, "No dump files could be located in this directory.", MessageBoxButtons.OK, MessageBoxIcon.Error );
                return;
            }

            DumpForm df = new DumpForm( this, fileList, fbd.SelectedPath );
            df.Show();
        }

        private void exitToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.Close();
        }
    }
}
