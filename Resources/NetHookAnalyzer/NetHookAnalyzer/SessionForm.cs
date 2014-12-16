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
using SteamKit2.GC.Internal;

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

        ClientMsgProtobuf<T> GetProtoMsgFromFile<T>( EMsg eMsg, string fileName )
            where T : IExtensible, new()
        {
            var fileData = File.ReadAllBytes( fileName );
            var msg = new SteamKit2.PacketClientMsgProtobuf( eMsg, fileData );
            var proto = new ClientMsgProtobuf<T>( msg );
            return proto;
        }

        string PacketItemNameEnhance( EMsg eMsg, string nameFromEMsg, string fileName )
        {
            if ( eMsg == EMsg.ClientToGC || eMsg == EMsg.ClientFromGC )
            {
                var proto = GetProtoMsgFromFile<CMsgGCClient>( eMsg, fileName );
                var gcEMsg = proto.Body.msgtype;
                var gcName = BuildEMsg( MsgUtil.GetGCMsg( gcEMsg ) );

                var headerToTrim = "k_EMsg";
                if ( gcName.StartsWith( headerToTrim ) )
                {
                    gcName = gcName.Substring( headerToTrim.Length );
                }

                return string.Format( "{0} ({1})", nameFromEMsg, gcName );
            }
            else if ( eMsg == EMsg.ServiceMethod )
            {
                var fileData = File.ReadAllBytes( fileName );
                var hdr = new MsgHdrProtoBuf();
                using ( var ms = new MemoryStream( fileData ) )
                {
                    hdr.Deserialize(ms);
                }

                return string.Format( "{0} ({1})", nameFromEMsg, hdr.Proto.target_job_name );
            }
            else if ( eMsg == EMsg.ClientServiceMethod )
            {
                var proto = GetProtoMsgFromFile<CMsgClientServiceMethod>( eMsg, fileName );
                return string.Format( "{0} ({1})", nameFromEMsg, proto.Body.method_name );
            }
            else if ( eMsg == EMsg.ClientServiceMethodResponse )
            {
                var proto = GetProtoMsgFromFile<CMsgClientServiceMethodResponse>( eMsg, fileName );
                return string.Format( "{0} ({1})", nameFromEMsg, proto.Body.method_name );
            }

            return nameFromEMsg;
        }

        void PopulatePackets()
        {
            viewPacket.Items.Clear();

            foreach ( var file in packetFiles )
            {
                PacketItem.PacketItemNameEnhance nameEnhance = chkEnhanceMsgNames.Checked ? PacketItemNameEnhance : (PacketItem.PacketItemNameEnhance)null;
                PacketItem packItem = new PacketItem( file.FullName, nameEnhance );

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
                object body = null;
                
                if ( MsgUtil.IsProtoBuf( realEMsg ) && eMsg == EMsg.ServiceMethod && !string.IsNullOrEmpty( ((MsgHdrProtoBuf)header).Proto.target_job_name ) )
                {
                    body = BuildServiceMethodBody( ((MsgHdrProtoBuf)header).Proto.target_job_name,packetStream, x => x.GetParameters().First().ParameterType );
                }
                else if ( body == null )
                {
                    body = BuildBody( realEMsg, packetStream );
                }

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
                TreeNode serviceMethodNode = new TreeNode( "Service Method Body: " );

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
                            body = BuildBody( gcBody.msgtype, ms, gcBody.appid),
                        };

                        DumpType( gc, gcBodyNode );
                    }
                }
                else if ( body is CMsgClientServiceMethod )
                {
                    var request = body as CMsgClientServiceMethod;
                    var name = request.method_name;

                    var serviceBody = BuildServiceMethodBody( request.method_name, request.serialized_method, x => x.GetParameters().First().ParameterType );
                    DumpType( serviceBody, serviceMethodNode );
                }
                else if ( body is CMsgClientServiceMethodResponse )
                {
                    var response = body as CMsgClientServiceMethodResponse;
                    var name = response.method_name;

                    var serviceBody = BuildServiceMethodBody( response.method_name, response.serialized_method_response, x => x.ReturnType );
                    DumpType( serviceBody, serviceMethodNode );
                }

                DumpType( payload, payloadNode );

                treePacket.Nodes.Add( infoNode );
                treePacket.Nodes.Add( headerNode );
                treePacket.Nodes.Add( bodyNode );
                treePacket.Nodes.Add( gcBodyNode );
                treePacket.Nodes.Add( payloadNode );
                treePacket.Nodes.Add( serviceMethodNode );
            }

            treePacket.ExpandAll();
        }

        static object BuildServiceMethodBody( string methodName, byte[] methodData, Func<MethodInfo, Type> typeSelector )
        {
            using ( var ms = new MemoryStream( methodData ) )
            {
                return BuildServiceMethodBody( methodName, ms, typeSelector );
            }
        }


        static object BuildServiceMethodBody( string methodName, Stream methodStream, Func<MethodInfo, Type> typeSelector )
        {
            var methodInfo = FindMethodInfo( methodName );
            if ( methodInfo != null )
            {
                var requestType = typeSelector( methodInfo );
                var request = Serializer.NonGeneric.Deserialize( requestType, methodStream );
                return request;
            }

            return null;
        }

        static MethodInfo FindMethodInfo( string serviceMethodName )
        {
            var interfaceName = "I" + serviceMethodName.Split( '.' ).First();
            var methodName = serviceMethodName.Split( '.' )[1].Split( '#' ).First();

            var namespaces = new[]
            {
                "SteamKit2.Unified.Internal",
                "SteamKit2.Unified.Internal.Steamworks"
            };

            foreach ( var ns in namespaces )
            {
                var interfaceType = Type.GetType( ns + "." + interfaceName + ", SteamKit2" );
                if ( interfaceType != null )
                {
                    var method = interfaceType.GetMethod( methodName );
                    if ( method != null )
                    {
                        return method;
                    }
                }
            }

            return null;
        }

        string BuildEMsg( uint eMsg )
        {
            eMsg = MsgUtil.GetGCMsg( eMsg );

            // first lets try the enum'd emsgs
            Type[] eMsgEnums =
            {
                typeof( SteamKit2.GC.Dota.Internal.EDOTAGCMsg ),
                typeof( SteamKit2.GC.CSGO.Internal.ECsgoGCMsg ),
                typeof( SteamKit2.GC.Internal.EGCBaseMsg ),
                typeof( SteamKit2.GC.Internal.ESOMsg ),
                typeof( SteamKit2.GC.Internal.EGCSystemMsg ),
                typeof( SteamKit2.GC.Internal.EGCItemMsg ),
                typeof( SteamKit2.GC.Internal.EGCBaseClientMsg ),
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

                    node.ContextMenu = new ContextMenu( new[] {
                        new MenuItem( "Save to File...", delegate( object sender, EventArgs e )
                        {
                            var dialog = new SaveFileDialog { DefaultExt = "bin", SupportMultiDottedExtensions = true };
                            var result = dialog.ShowDialog( this );
                            if ( result == DialogResult.OK )
                            {
                                File.WriteAllBytes( dialog.FileName, data );
                            }
                        }),
                    });

                    const int MaxBinLength = 400;
                    if ( data.Length > MaxBinLength )
                    {
                        node.Nodes.Add( string.Format( "Length exceeded {0} bytes!", MaxBinLength ) );
                        return;
                    }

                    Action<object> setAsRadioSelected = delegate( object sender )
                    {
                        var senderItem = (MenuItem)sender;
                        foreach ( MenuItem item in senderItem.Parent.MenuItems )
                        {
                            item.Checked = false;
                        }
                        senderItem.Checked = true;
                    };

                    node.ContextMenu.MenuItems.Add( new MenuItem( "-" )); // Separator

                    MenuItem intialMenuItem;

                    node.ContextMenu.MenuItems.Add( new MenuItem( "Display as ASCII", delegate( object sender, EventArgs e )
                    {
                        setAsRadioSelected( sender );
                        node.Nodes.Clear();

                        var strAscii = Encoding.ASCII.GetString( data ).Replace( "\0", "\\0" );
                        node.Nodes.Add( strAscii );
                        node.Expand();

                    }) { RadioCheck = true } );

                   node.ContextMenu.MenuItems.Add( new MenuItem( "Display as UTF-8", delegate( object sender, EventArgs e )
                    {
                        setAsRadioSelected( sender );
                        node.Nodes.Clear();

                        var strUnicode = Encoding.UTF8.GetString( data ).Replace( "\0", "\\0" );
                        node.Nodes.Add( strUnicode );
                        node.Expand();

                    }) { RadioCheck = true } );

                   node.ContextMenu.MenuItems.Add( ( intialMenuItem = new MenuItem( "Display as Hexadecimal", delegate( object sender, EventArgs e )
                    {
                        setAsRadioSelected( sender );
                        node.Nodes.Clear();

                        var hexString = data.Aggregate( new StringBuilder(), ( str, value ) => str.Append( value.ToString( "X2" ) ) ).ToString();
                        node.Nodes.Add( hexString );
                        node.Expand();

                    }) { RadioCheck = true, Checked = true } ) );

                   node.ContextMenu.MenuItems.Add(new MenuItem("Display as Binary KeyValues", delegate(object sender, EventArgs e)
                   {
                       setAsRadioSelected(sender);
                       node.Nodes.Clear();

                       var kv = new KeyValue();
                       bool didRead;
                       using ( var ms = new MemoryStream( data ) )
                       {
                           try
                           {
                               didRead = kv.ReadAsBinary( ms );
                           }
                           catch (InvalidDataException)
                           {
                               didRead = false;
                           }
                       }

                       if ( !didRead )
                       {
                           node.Nodes.Add( "Not a valid KeyValues object!" );
                       }
                       else
                       {
                           node.Nodes.Add( BuildKeyValuesNode( kv.Children[0] ) );
                       }

                       node.ExpandAll();

                   }) { RadioCheck = true });

                    intialMenuItem.PerformClick();

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

        Type GetMessageBodyType( uint realEMsg )
        {
            EMsg eMsg = MsgUtil.GetMsg(realEMsg);

            if ( MessageTypeOverrides.BodyMap.ContainsKey( eMsg ) )
            {
                return MessageTypeOverrides.BodyMap[eMsg];
            }

            var protomsgType = typeof(CMClient).Assembly.GetTypes().ToList().Find(type =>
            {
                if (type.GetInterfaces().ToList().Find(inter => inter == typeof(IExtensible)) == null)
                    return false;

                if (type.Name.EndsWith(eMsg.ToString(), StringComparison.OrdinalIgnoreCase))
                    return true;

                return false;
            });

            return protomsgType;
        }

        IEnumerable<Type> GetGCMessageBodyTypeCandidates( uint realEMsg, uint gcAppid = 0 )
        {
            uint eMsg = MsgUtil.GetGCMsg( realEMsg );

            if ( MessageTypeOverrides.GCBodyMap.ContainsKey( eMsg ) )
            {
                return Enumerable.Repeat( MessageTypeOverrides.GCBodyMap[eMsg], 1 );
            }

            var gcMsgName = BuildEMsg( realEMsg );

            var typeMsgName = gcMsgName
                .Replace( "k_", string.Empty )
                .Replace( "ESOMsg", string.Empty )
                .TrimStart( '_' )
                .Replace( "EMsg", string.Empty )
                .TrimStart( "GC" );
            
            var possibleTypes = from type in typeof(CMClient).Assembly.GetTypes()
                                from typePrefix in GetPossibleGCTypePrefixes( gcAppid )
                                where type.GetInterfaces().Contains( typeof(IExtensible) )
                                where type.FullName.StartsWith( typePrefix ) && type.FullName.EndsWith( typeMsgName )
                                select type;

            return possibleTypes;
        }

        object BuildBody( uint realEMsg, Stream str , uint gcAppid = 0)
        {
            EMsg eMsg = MsgUtil.GetMsg( realEMsg );

            if ( eMsg == EMsg.ClientLogonGameServer )
                eMsg = EMsg.ClientLogon; // temp hack for now
            else if( eMsg == EMsg.ClientGamesPlayedWithDataBlob)
                eMsg = EMsg.ClientGamesPlayed;

            var protomsgType = GetMessageBodyType( realEMsg );

            if (protomsgType != null)
            {
                return Serializer.NonGeneric.Deserialize( protomsgType, str );
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

            foreach ( var type in GetGCMessageBodyTypeCandidates( realEMsg, gcAppid ) )
            {
                var streamPos = str.Position;
                try
                {
                    return Deserialize( type, str );
                }
                catch ( Exception )
                {
                    str.Position = streamPos;
                }
            }

            if (!MsgUtil.IsProtoBuf(realEMsg))
                return null;

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

        IEnumerable<string> GetPossibleGCTypePrefixes(uint appid)
        {
            yield return "SteamKit2.GC.Internal.CMsg";

            switch (appid)
            {
                case 440:
                    yield return "SteamKit2.GC.TF.Internal.CMsg";
                    break;

                case 570:
                    yield return "SteamKit2.GC.Dota.Internal.CMsg";
                    break;

                case 730:
                    yield return "SteamKit2.GC.CSGO.Internal.CMsg";
                    break;
            }
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

        static TreeNode BuildKeyValuesNode( KeyValue kv )
        {
            var node = new TreeNode();
            if ( kv.Children.Count > 0 )
            {
                node.Text = kv.Name;
                foreach (var child in kv.Children)
                {
                    node.Nodes.Add( BuildKeyValuesNode( child ) );
                }
            }
            else
            {
                node.Text = string.Format( "{0}: {1}", kv.Name, kv.Value );
            }

            return node;
        }
    }

}
