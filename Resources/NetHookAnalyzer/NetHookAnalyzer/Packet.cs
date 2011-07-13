using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;

namespace NetHookAnalyzer
{
    class PacketItem : ListViewItem
    {
        static Regex NameRegex = new Regex(
            @"(?<num>\d+)_(?<direction>in|out)_(?<emsg>\d+)_k_EMsg(?<name>[\w_<>]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );


        public string FileName { get; private set; }

        public int PacketNum { get; private set; }
        public string Direction { get; private set; }
        public int EMsg { get; private set; }
        public new string Name { get; private set; }

        public bool IsValid { get; private set; }


        public PacketItem( string fileName )
        {
            Match m = NameRegex.Match( fileName );

            if ( !m.Success )
            {
                IsValid = false;
                return;
            }

            this.FileName = fileName;

            this.PacketNum = Convert.ToInt32( m.Groups[ "num" ].Value );
            this.Direction = m.Groups[ "direction" ].Value;
            this.EMsg = Convert.ToInt32( m.Groups[ "emsg" ].Value );
            this.Name = m.Groups[ "name" ].Value;

            this.IsValid = true;

            this.Text = this.PacketNum.ToString();
            this.SubItems.Add( this.Direction );
            this.SubItems.Add( this.Name );
        }
    }

    class PacketComparer : IComparer
    {

        public int Column { get; set; }
        public int Order { get; set; }


        public PacketComparer()
        {
            this.Column = 0;
            this.Order = 1;
        }


        public int Compare( object x, object y )
        {
            PacketItem l = ( PacketItem )x;
            PacketItem r = ( PacketItem )y;

            switch ( Column )
            {
                case 0:
                    return this.Order * Comparer<int>.Default.Compare( l.PacketNum, r.PacketNum );

                case 1:
                    return this.Order * StringComparer.OrdinalIgnoreCase.Compare( l.Direction, r.Direction );

                case 2:
                    return this.Order * StringComparer.OrdinalIgnoreCase.Compare( l.Name, r.Name );
            }

            return 0;
        }
    }
}
