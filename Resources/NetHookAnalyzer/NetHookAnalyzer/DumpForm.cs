using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using SteamKit2;
using System.Reflection;
using ProtoBuf;

namespace NetHookAnalyzer
{
    public partial class DumpForm : Form
    {
        FileInfo[] dumpList;
        PacketSorter sorter;

        public DumpForm( MainForm mdiParent, FileInfo[] fileList, string dir )
        {
            InitializeComponent();

            sorter = new PacketSorter();
            listPackets.ListViewItemSorter = sorter;

            this.Text = dir;

            this.MdiParent = mdiParent;
            this.dumpList = fileList;

            this.PopulatePacketList();

        }

        void PopulatePacketList()
        {
            listPackets.Items.Clear();

            foreach ( var file in dumpList )
            {
                Packet pk = new Packet( file.FullName );

                if ( chBoxIn.Checked && pk.Direction == "in" )
                    listPackets.Items.Add( pk );

                if ( chBoxOut.Checked && pk.Direction == "out" )
                    listPackets.Items.Add( pk );
            }
        }

        private void listPackets_ColumnClick( object sender, ColumnClickEventArgs e )
        {
            sorter.Column = e.Column;

            if ( sorter.Order == SortOrder.Ascending )
                sorter.Order = SortOrder.Descending;
            else
                sorter.Order = SortOrder.Ascending;

            listPackets.Sort();
        }

        private void filterCheckChanged( object sender, EventArgs e )
        {
            this.PopulatePacketList();
        }

        private void listPackets_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( listPackets.SelectedItems.Count == 0 )
                return;

            Packet selectedPacket = listPackets.SelectedItems[ 0 ] as Packet;

            if ( selectedPacket == null )
                return;

            txtSummary.Text = GenerateSummary( selectedPacket );

        }

        string GenerateSummary( Packet selectedPacket )
        {
            StringBuilder sb = new StringBuilder();

            byte[] data = File.ReadAllBytes( selectedPacket.FullPath );
            MemoryStream ms = new MemoryStream( data );

            uint rawEmsg = BitConverter.ToUInt32( data, 0 );

            bool isProto = MsgUtil.IsProtoBuf( rawEmsg );
            EMsg eMsg = MsgUtil.GetMsg( rawEmsg );

            sb.AppendFormat( "EMsg: {0} (Proto: {1})", eMsg, isProto );
            sb.AppendLine();

            Type clientMsgType = GetClientMsgForEMsg( rawEmsg );

            if ( clientMsgType == null && isProto )
            {
                sb.AppendLine( "Unable to find ClientMsg, handling protobuf directly!" );

                // manually parse the data
                Type protoType = GetProtoTypeForEMsg( eMsg );

                if ( protoType == null )
                {
                    sb.AppendLine( "Error: Unable to find protobuf type!" );
                    return sb.ToString();
                }

                IExtensible protoMsg;
                try
                {
                    ms.Seek( 22, SeekOrigin.Begin );
                    protoMsg = ( IExtensible )Deserialize( protoType, ms );
                }
                catch ( Exception ex )
                {
                    sb.AppendFormat( "Error when deserializing protobuf: {0}", ex.ToString() );
                    sb.AppendLine();
                    return sb.ToString();
                }

                sb.AppendLine();
                sb.AppendFormat( "{0}:", protoType.Name );
                sb.AppendLine();

                foreach ( var prop in protoType.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
                {
                    object propData = prop.GetValue( protoMsg, null );
                    BuildString( sb, prop, propData, 1 );
                }

                return sb.ToString();
            }
            else if ( clientMsgType == null )
            {
                sb.AppendLine( "Error: Unable to find ClientMsg!" );
                return sb.ToString();
            }

            sb.AppendLine();

            IClientMsg clientMsg;
            try
            {
                clientMsg = ( IClientMsg )Activator.CreateInstance( clientMsgType );
                clientMsg.SetData( data );
            }
            catch ( Exception ex )
            {
                sb.AppendFormat( "Error when deserializing client message: {0}", ex.ToString() );
                sb.AppendLine();
                return sb.ToString();
            }


            foreach ( var prop in clientMsgType.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                object propData = prop.GetValue( clientMsg, null );
                BuildString( sb, prop, propData, 0 );
            }

            try
            {
                BuildPayload( sb, clientMsgType, clientMsg );
            }
            catch ( Exception ex )
            {
                sb.AppendFormat( "Payload: Error: {0}", ex.Message );
                sb.AppendLine();
            }

            return sb.ToString();
        }

        void BuildPayload( StringBuilder sb, Type clientMsgType, IClientMsg clientMsg )
        {
            PropertyInfo pi = clientMsgType.GetProperty( "Payload", BindingFlags.Public | BindingFlags.Instance );
            object payloadObj = pi.GetValue( clientMsg, null );
            PropertyInfo lenInfo = pi.PropertyType.GetProperty( "Length", BindingFlags.Public | BindingFlags.Instance );

            long len = ( long )lenInfo.GetValue( payloadObj, null );

            if ( len == 0 )
                return;

            MethodInfo mi = pi.PropertyType.GetMethod( "ToArray", BindingFlags.Public | BindingFlags.Instance );
            byte[] data = ( byte[] )mi.Invoke( payloadObj, null );

            sb.AppendLine();
            sb.AppendLine( "Payload:");
            sb.AppendFormat( "\tBin: {0}", BitConverter.ToString( data ).Replace( "-", "" ) );
            sb.AppendLine();
            sb.AppendFormat( "\tASCII: {0}", Encoding.ASCII.GetString( data ).Replace( "\0", "\\0" ) );
            sb.AppendLine();
            sb.AppendFormat( "\tUTF8: {0}", Encoding.UTF8.GetString( data ).Replace( "\0", "\\0" ) );

        }

        object Deserialize( Type genericParam, Stream stream )
        {
            MethodInfo mi = typeof( Serializer ).GetMethod( "Deserialize", BindingFlags.Static | BindingFlags.Public );
            mi = mi.MakeGenericMethod( genericParam );
            return mi.Invoke( null, new object[] { stream } );
        }

        Type GetProtoTypeForEMsg( EMsg eMsg )
        {
            string name = string.Format( "CMsg{0}", eMsg );

            Type[] steamKitTypes = typeof( CMClient ).Assembly.GetTypes();

            foreach ( var type in steamKitTypes )
            {
                if ( type.Name == name )
                {
                    return type;
                }
            }

            return null;
        }

        Type GetClientMsgForEMsg( uint rawEmsg )
        {
            bool isProto = MsgUtil.IsProtoBuf( rawEmsg );
            EMsg eMsg = MsgUtil.GetMsg( rawEmsg );

            string name = string.Format( "Msg{0}", eMsg );

            Type innerMessage = null;

            Type[] steamKitTypes = typeof( CMClient ).Assembly.GetTypes();

            foreach ( var type in steamKitTypes )
            {
                if ( type.Name == name )
                {
                    innerMessage = type;
                    break;
                }
            }

            if ( innerMessage == null )
                return null;

            Type[] innerTypes = new Type[ 2 ];

            innerTypes[ 0 ] = innerMessage;
            innerTypes[ 1 ] = typeof( ExtendedClientMsgHdr );

            if ( isProto )
                innerTypes[ 1 ] = typeof( MsgHdrProtoBuf );

            Type clientMsg = typeof( ClientMsg<,> );
            clientMsg = clientMsg.MakeGenericType( innerTypes );

            return clientMsg;
            
        }

        void BuildString( StringBuilder sb, PropertyInfo prop, object data, int numTabs )
        {
            if ( data == null )
            {
                sb.Append( "".PadLeft( numTabs, '\t' ) );
                sb.AppendFormat( "{0}: null", prop.Name );
                sb.AppendLine();

                return;
            }

            if ( prop.PropertyType.IsValueType || prop.PropertyType == typeof( string ) )
            {
                sb.Append( "".PadLeft( numTabs, '\t' ) );
                sb.AppendFormat( "{0}: {1}", prop.Name, data.ToString() );
                sb.AppendLine();

                return;
            }

            if ( TypeIsEnumerable( prop.PropertyType ) )
            {
                BuildArray( sb, prop, data, numTabs );
                return;
            }

            // must be some container object, print it
            sb.Append( "".PadLeft( numTabs, '\t' ) );
            sb.AppendFormat( "{0}: {1}:", prop.Name, prop.PropertyType.Name );
            sb.AppendLine();


            foreach ( var subProp in prop.PropertyType.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                object subData = null;

                try
                {
                    subData = subProp.GetValue( data, null );
                }
                catch ( TargetInvocationException ex )
                {
                    sb.Append( "".PadLeft( numTabs, '\t' ) );
                    sb.AppendFormat( "{0}: Error: {1}", prop.Name, ex.InnerException.Message );
                    sb.AppendLine();
                    continue;
                }

                BuildString( sb, subProp, subData, numTabs + 1 );
            }

            sb.AppendLine();

        }
        void BuildArray( StringBuilder sb, PropertyInfo prop, object data, int numTabs )
        {
            if ( data == null )
            {
                sb.Append( "".PadLeft( numTabs, '\t' ) );
                sb.AppendFormat( "{0}: null", prop.Name );
                sb.AppendLine();
                return;
            }

            IEnumerable arr = ( IEnumerable )data;

            int count = 0;
            foreach ( var subData in arr )
                count++;

            sb.Append( "".PadLeft( numTabs, '\t' ) );
            sb.AppendFormat( "{0}: {1}[ {2} ]:", prop.Name, prop.PropertyType.Name, count );
            sb.AppendLine();

            int index = -1;

            foreach ( var subData in arr )
            {
                index++;

                Type innerType = subData.GetType();

                if ( innerType.IsValueType )
                {
                    sb.Append( "".PadLeft( numTabs + 1, '\t' ) );
                    sb.AppendFormat( "[ {0} ]: {1}", index, subData.ToString() );
                    sb.AppendLine();
                    continue;
                }

                if ( TypeIsEnumerable( innerType ) )
                {
                    BuildArray( sb, prop, subData, numTabs + 1 );
                    continue;
                }

                sb.Append( "".PadLeft( numTabs + 1, '\t' ) );
                sb.AppendFormat( "[ {0} ]: {1}", index, innerType.Name );
                sb.AppendLine();

                foreach ( var subProp in innerType.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
                {
                    object subArrData = subProp.GetValue( subData, null );
                    BuildString( sb, subProp, subArrData, numTabs + 1 );
                }
                sb.AppendLine();
            }

            return;
        }

        bool TypeIsEnumerable( Type type )
        {
            foreach ( var iface in type.GetInterfaces() )
            {
                if ( iface == typeof( IEnumerable ) )
                    return true;
            }
            return false;
        }

        
    }

    class Packet : ListViewItem
    {
        static Regex NameRegex = new Regex(
            @"(?<num>\d+)_(?<direction>in|out)_(?<emsg>\d+)_(?<name>[\w_]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public string FullPath { get; private set; }

        public int PacketNum { get; private set; }
        public string Direction { get; private set; }

        public int EMsg { get; private set; }
        public string EMsgName { get; private set; }



        public Packet( string fileName )
        {
            this.FullPath = fileName;

            Match m = NameRegex.Match( Path.GetFileNameWithoutExtension( fileName ) );

            this.PacketNum = Int32.Parse( m.Groups[ "num" ].Value );
            this.Direction = m.Groups[ "direction" ].Value;

            this.EMsg = Int32.Parse( m.Groups[ "emsg" ].Value );
            this.EMsgName = m.Groups[ "name" ].Value;

            this.SubItems.Add( this.Direction );
            this.SubItems.Add( this.EMsgName );

            this.Text = this.PacketNum.ToString();
        }
    }

    class PacketSorter : IComparer
    {

        public int Column { get; set; }
        public SortOrder Order { get; set; }


        public PacketSorter()
        {
            Order = SortOrder.Ascending;
            Column = 0;
        }


        public int Compare( object x, object y )
        {
            int outOrder = ( Order == SortOrder.Descending ) ? -1 : 1;

            Packet l = ( Packet )x;
            Packet r = ( Packet )y;

            switch ( Column )
            {
                case 0:
                    return outOrder * Comparer<int>.Default.Compare( l.PacketNum, r.PacketNum );
                    
                case 1:
                    return outOrder * StringComparer.OrdinalIgnoreCase.Compare( l.Direction, r.Direction );

                case 2:
                    return outOrder * StringComparer.OrdinalIgnoreCase.Compare( l.EMsgName, r.EMsgName );
            }

            return 0;
        }
    }
}
