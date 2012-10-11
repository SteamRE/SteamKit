using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SteamKit2;
using ProtoBuf;
using System.Reflection;
using System.Collections;
using SteamKit2.Internal;
using ProtoBuf.Meta;
using SteamKit2.GC;

namespace NetHookAnalyzer
{
    public partial class SessionForm : Form
    {
        FileInfo[] packetFiles;
        PacketComparer sorter;
        PacketItem lastPacket;


        public SessionForm( Form mdiParent, FileInfo[] fileList, string path )
        {
            InitializeComponent();

            viewPacket.ListViewItemSorter = sorter = new PacketComparer();

            MdiParent = mdiParent;
            Text = path;
            WindowState = FormWindowState.Maximized;

            packetFiles = fileList;

            PopulatePackets();
        }


        void PopulatePackets()
        {
            viewPacket.Items.Clear();

            foreach ( var file in packetFiles )
            {
                PacketItem packItem = new PacketItem( file.FullName );

                if ( !packItem.IsValid )
                    continue;

                if ( packItem.Direction == "out" && !chkOut.Checked )
                    continue;

                if ( packItem.Direction == "in" && !chkIn.Checked )
                    continue;

                viewPacket.Items.Add( packItem );
            }

            viewPacket.Sort();
        }

        void Dump( PacketItem packet )
        {
            treePacket.Nodes.Clear();

            using ( FileStream packetStream = File.OpenRead( packet.FileName ) )
            {
                uint realEMsg = PeekUInt32( packetStream );
                EMsg eMsg = MsgUtil.GetMsg( realEMsg );

                var info = new
                {
                    EMsg = eMsg,
                    IsProto = MsgUtil.IsProtoBuf( realEMsg ),
                };
                var header = BuildHeader( realEMsg, packetStream );
                var body = BuildBody( realEMsg, packetStream );
                object payload = null;
                if ( body == null )
                {
                    body = "Unable to find body message!";
                    payload = "Unable to get payload: Body message missing!";
                }
                else
                {
                    payload = BuildPayload( packetStream );
                }

                TreeNode infoNode = new TreeNode( "Info: " );
                TreeNode headerNode = new TreeNode( "Header: " );
                TreeNode bodyNode = new TreeNode( "Body: " );
                TreeNode gcBodyNode = new TreeNode( "GC Body: " );
                TreeNode payloadNode = new TreeNode( "Payload: " );

                DumpType( info, infoNode );
                DumpType( header, headerNode );
                DumpType( body, bodyNode );
                if ( body is CMsgGCClient )
                {
                    var gcBody = body as CMsgGCClient;

                    using ( var ms = new MemoryStream( gcBody.payload ) )
                    {
                        var gc = new
                        {
                            emsg = BuildEMsg( gcBody.msgtype ),
                            header = BuildGCHeader( gcBody.msgtype, ms ),
                            body = BuildBody( gcBody.msgtype, ms ),
                        };

                        DumpType( gc, gcBodyNode );
                    }
                }
                DumpType( payload, payloadNode );

                treePacket.Nodes.Add( infoNode );
                treePacket.Nodes.Add( headerNode );
                treePacket.Nodes.Add( bodyNode );
                treePacket.Nodes.Add( gcBodyNode );
                treePacket.Nodes.Add( payloadNode );
            }

            treePacket.ExpandAll();
        }

        string BuildEMsg( uint eMsg )
        {
            eMsg = MsgUtil.GetGCMsg( eMsg );

            // first lets try the enum'd emsgs
            Type[] eMsgEnums =
            {
                typeof( SteamKit2.GC.Dota.Internal.EDOTAGCMsg ),
                typeof( SteamKit2.GC.Internal.EGCBaseMsg ),
                typeof( SteamKit2.GC.Internal.EGCSharedMsg )
            };

            foreach ( var enumType in eMsgEnums )
            {
                if ( Enum.IsDefined( enumType, ( int )eMsg ) )
                    return Enum.GetName( enumType, ( int )eMsg );
            }

            // no dice on those, back to the classes
            List<FieldInfo> fields = new List<FieldInfo>();
            fields.AddRange( typeof( SteamKit2.GC.TF2.EGCMsg ).GetFields( BindingFlags.Public | BindingFlags.Static ) );
            fields.AddRange( typeof( SteamKit2.GC.CSGO.EGCMsg ).GetFields( BindingFlags.Public | BindingFlags.Static ) );

            var field = fields.SingleOrDefault( f =>
            {
                uint value = ( uint )f.GetValue( null );
                return value == eMsg;
            } );

            if ( field != null )
                return string.Format( "{0} ({1})", field.Name, field.DeclaringType.FullName );

            return eMsg.ToString();
        }


        void DumpType( object obj, TreeNode node )
        {
            Type propType = ( obj != null ? obj.GetType() : null );

            if ( obj == null )
            {
                node.Text += "<null>";
                return;
            }

            if ( propType != null )
            {
                if ( propType.IsValueType )
                {
                    node.Text += obj.ToString();
                    return;
                }
                else if (propType == typeof( string ) )
                {
                    node.Text += string.Format( "\"{0}\"", obj );
                    return;
                }
                else if ( propType == typeof( SteamID ) )
                {
                    SteamID sId = obj as SteamID;
                    node.Text += string.Format( "{0} ({1}) ", sId.ConvertToUInt64(), sId.Render() );
                }
                else if ( obj is byte[] )
                {
                    byte[] data = obj as byte[];

                    node.Text += string.Format( "byte[ {0} ]", data.Length );

                    if ( data.Length == 0 )
                    {
                        return;
                    }

                    const int MaxBinLength = 400;
                    if ( data.Length > MaxBinLength )
                    {
                        node.Nodes.Add( string.Format( "Length exceeded {0} bytes!", MaxBinLength ) );
                        return;
                    }

                    string strAscii = Encoding.ASCII.GetString( data ).Replace( "\0", "\\0" );
                    string strUnicode = Encoding.UTF8.GetString( data ).Replace( "\0", "\\0" );
                    string hexString = data.Aggregate( new StringBuilder(), ( str, value ) => str.Append( value.ToString( "X2" ) ) ).ToString();

                    node.Nodes.Add( string.Format( "{0}: {1}", "ASCII", strAscii ) );
                    node.Nodes.Add( string.Format( "{0}: {1}", "UTF8", strUnicode ) );
                    node.Nodes.Add( string.Format( "{0}: {1}", "Binary", hexString ) );

                    return;
                }
                else if ( TypeIsDictionary( propType ) )
                {
                    IDictionary dict = obj as IDictionary;
                    foreach (DictionaryEntry pair in dict)
                    {
                        TreeNode subNode = new TreeNode(string.Format(
                            "[ {0} ]: ", pair.Key.ToString() ));
                        node.Nodes.Add(subNode);

                        DumpType(pair.Value, subNode);
                    }

                    return;
                }
                else if ( TypeIsArray( propType ) )
                {
                    Type innerType = null;
                    int count = 0;
                    bool truncated = false;


                    foreach ( var subObj in obj as IEnumerable )
                    {
                        innerType = subObj.GetType();

                        count++;

                        if ( count <= 100 )
                        {
                            TreeNode subNode = new TreeNode( string.Format(
                                "[ {0} ]: {1}",
                                count - 1,
                                ( innerType.IsValueType ? "" : innerType.Name )
                            ) );
                            node.Nodes.Add( subNode );

                            DumpType( subObj, subNode );
                        }
                        else
                        {
                            truncated = true;
                        }
                    }

                    if ( truncated )
                    {
                        TreeNode truncNode = new TreeNode( "Array truncated: more than 100 entries!" );
                        node.Nodes.Add( truncNode );
                    }

                    node.Text += string.Format(
                        "{0}[ {1} ]",
                        ( innerType == null ? propType.Name : innerType.Name ),
                        count
                    );

                    return;
                }

                node.Text += propType.Name;
            }

            foreach ( var property in obj.GetType().GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                var subObject = property.GetValue( obj, null );
                TreeNode subNode = new TreeNode( property.Name + ": " );
                node.Nodes.Add( subNode );

                DumpType( subObject, subNode );
            }
        }


        ISteamSerializableHeader BuildHeader( uint realEMsg, Stream str )
        {
            ISteamSerializableHeader hdr = null;

            if ( MsgUtil.IsProtoBuf( realEMsg ) )
            {
                hdr = new MsgHdrProtoBuf();
            }
            else
            {
                hdr = new ExtendedClientMsgHdr();
            }

            hdr.Deserialize( str );
            return hdr;
        }
        IGCSerializableHeader BuildGCHeader( uint realEMsg, Stream str )
        {
            IGCSerializableHeader hdr = null;

            if ( MsgUtil.IsProtoBuf( realEMsg ) )
            {
                hdr = new MsgGCHdrProtoBuf();
            }
            else
            {
                hdr = new MsgGCHdr();
            }

            hdr.Deserialize( str );
            return hdr;
        }

        object BuildBody( uint realEMsg, Stream str )
        {
            EMsg eMsg = MsgUtil.GetMsg( realEMsg );

            if ( eMsg == EMsg.ClientLogonGameServer )
                eMsg = EMsg.ClientLogon; // temp hack for now
            else if( eMsg == EMsg.ClientGamesPlayedWithDataBlob)
                eMsg = EMsg.ClientGamesPlayed;

            var protomsgType = typeof(CMClient).Assembly.GetTypes().ToList().Find(type =>
            {
                if (type.GetInterfaces().ToList().Find(inter => inter == typeof(IExtensible)) == null)
                    return false;

                if (type.Name.EndsWith(eMsg.ToString()))
                    return true;

                return false;
            });

            if (protomsgType != null)
            {
                return RuntimeTypeModel.Default.Deserialize(str, null, protomsgType);
            }

            // lets first find the type by checking all EMsgs we have
            var msgType = typeof( CMClient ).Assembly.GetTypes().ToList().Find( type =>
            {
                if ( type.GetInterfaces().ToList().Find( inter => inter == typeof( ISteamSerializableMessage ) ) == null )
                    return false;

                var gcMsg = Activator.CreateInstance( type ) as ISteamSerializableMessage;

                return gcMsg.GetEMsg() == eMsg;
            } );

            string eMsgName = eMsg.ToString();

            eMsgName = eMsgName.Replace( "Econ", "" ).Replace( "AM", "" );

            // check name
            if ( msgType == null )
                msgType = GetSteamKitType( string.Format( "SteamKit2.Msg{0}", eMsgName ) );


            if ( msgType != null )
            {
                var body = Activator.CreateInstance( msgType ) as ISteamSerializableMessage;
                body.Deserialize( str );

                return body;
            }

            msgType = GetSteamKitType( string.Format( "SteamKit2.CMsg{0}", eMsgName ) );
            if ( msgType != null )
            {
                return Deserialize( msgType, str );
            }

            if ( eMsg == EMsg.ClientToGC || eMsg == EMsg.ClientFromGC )
            {
                return Serializer.Deserialize<CMsgGCClient>( str );
            }

            // try reading it as a protobuf
            using (ProtoReader reader = new ProtoReader(str, null, null))
            {
                var fields = new Dictionary<int, List<object>>();

                while(true)
                {
                    int field = reader.ReadFieldHeader();

                    if(field == 0)
                        break;

                    object fieldValue = null;

                    switch (reader.WireType)
                    {
                        case WireType.Variant:
                        case WireType.Fixed32:
                        case WireType.Fixed64:
                        case WireType.SignedVariant:
                            {
                                try
                                {
                                    fieldValue = reader.ReadInt64();
                                }
                                catch (Exception)
                                {
                                    fieldValue = "Unable to read Variant (debugme)";
                                }

                                break;
                            }
                        case WireType.String:
                            {
                                try
                                {
                                    fieldValue = reader.ReadString();
                                }
                                catch (Exception)
                                {
                                    fieldValue = "Unable to read String (debugme)";
                                }

                                break;
                            }
                        default:
                            {
                                fieldValue = string.Format( "{0} is not implemented", reader.WireType );
                                break;
                            }
                    }

                    if ( !fields.ContainsKey( field ) )
                    {
                        fields[ field ] = new List<object>();
                    }

                    fields[ field ].Add( fieldValue );
                }

                if (fields.Count > 0)
                    return fields;
            }

            return null;
        }

        byte[] BuildPayload( Stream str )
        {
            int payloadLen = ( int )( str.Length - str.Position );

            byte[] data = new byte[ payloadLen ];
            str.Read( data, 0, data.Length );

            return data;
        }

        Type GetSteamKitType( string name )
        {
            return typeof( CMClient ).Assembly.GetTypes().ToList().Find( type => type.FullName == name );
        }
        object Deserialize( Type msgType, Stream stream )
        {
            MethodInfo mi = typeof( Serializer ).GetMethod( "Deserialize", BindingFlags.Static | BindingFlags.Public );
            mi = mi.MakeGenericMethod( msgType );
            return mi.Invoke( null, new object[] { stream } );
        }
        bool TypeIsArray( Type type )
        {
            foreach ( var iface in type.GetInterfaces() )
            {
                if ( iface == typeof( IEnumerable ) )
                    return true;
            }
            return false;
        }
        bool TypeIsDictionary(Type type)
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (iface == typeof(IDictionary))
                    return true;
            }
            return false;
        }

        uint PeekUInt32( Stream str )
        {
            byte[] eMsgData = new byte[ 4 ];
            str.Read( eMsgData, 0, eMsgData.Length );
            str.Seek( -4, SeekOrigin.Current );
            return BitConverter.ToUInt32( eMsgData, 0 );
        }


        private void chkOut_CheckedChanged( object sender, EventArgs e )
        {
            // repopulate the list once the filter changes
            PopulatePackets();
        }
        private void viewPacket_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( viewPacket.SelectedItems.Count == 0 )
                return;

            var packet = viewPacket.SelectedItems[ 0 ] as PacketItem;

            if ( packet == lastPacket )
                return;

            lastPacket = packet;

            Dump( packet );
        }

        private void viewPacket_ColumnClick( object sender, ColumnClickEventArgs e )
        {
            if ( sorter.Column == e.Column )
            {
                sorter.Order = -sorter.Order;
            }

            sorter.Column = e.Column;
            viewPacket.Sort();
        }
    }

}
