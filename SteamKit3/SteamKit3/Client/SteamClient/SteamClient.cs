/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace SteamKit3
{
    public sealed partial class SteamClient : CMClient
    {
        static List<Type> registeredHandlers = new List<Type>();

        Dictionary<Type, ClientHandler> handlers;
        Queue<CallbackMsg> callbackQueue;


        public JobMgr JobMgr { get; private set; }


        public SteamClient()
        {
            handlers = new Dictionary<Type, ClientHandler>();
            callbackQueue = new Queue<CallbackMsg>();

            JobMgr = new JobMgr( this );

            SetupHandlers();
        }

        static SteamClient()
        {
            registeredHandlers = new List<Type>();

            RegisterHandlers( Assembly.GetExecutingAssembly() );
        }


        protected override void OnClientConnected()
        {
            // todo: when connected on the non-encrypted port (27014?) this should post a ConnectedCallback, since the remote server
            // will never attempt a channel handshake, and will wait on the client to do something
        }
        protected override void OnClientDisconnected()
        {
            this.PostCallback( new DisconnectedCallback() );
        }
        protected override void OnClientMsgReceived( NetMsgEventArgs e )
        {
            var packetMsg = GetPacketMsg( e.Data );

            JobMgr.RouteMsgToJob( packetMsg );
        }

        internal void OnMulti( byte[] data )
        {
            OnClientMsgReceived( new NetMsgEventArgs( data, null ) );
        }

        IPacketMsg GetPacketMsg( byte[] data )
        {
            uint rawEMsg = BitConverter.ToUInt32( data, 0 );
            EMsg eMsg = MsgUtil.GetMsg( rawEMsg );

            switch ( eMsg )
            {
                case EMsg.ChannelEncryptRequest:
                case EMsg.ChannelEncryptResponse:
                case EMsg.ChannelEncryptResult:
                    return new PacketMsg( eMsg, data );
            }

            if ( MsgUtil.IsProtoBuf( rawEMsg ) )
            {
                return new PacketClientMsgProtobuf( eMsg, data );
            }
            else
            {
                return new PacketClientMsg( eMsg, data );
            }
        }


        #region Callbacks
        public void PostCallback( CallbackMsg msg )
        {
            lock ( callbackQueue )
            {
                callbackQueue.Enqueue( msg );
                Monitor.Pulse( callbackQueue );
            }
        }

        public CallbackMsg GetCallback()
        {
            lock ( callbackQueue )
            {
                if ( callbackQueue.Count > 0 )
                    return callbackQueue.Dequeue();

                return null;
            }
        }
        public CallbackMsg WaitForCallback()
        {
            lock ( callbackQueue )
            {
                while ( callbackQueue.Count == 0 )
                    Monitor.Wait( callbackQueue );

                return callbackQueue.Dequeue();
            }
        }
        public CallbackMsg WaitForCallback( TimeSpan timeout )
        {
            lock ( callbackQueue )
            {
                while ( callbackQueue.Count == 0 )
                {
                    if ( !Monitor.Wait( callbackQueue, timeout ) )
                        return null;
                }

                return callbackQueue.Dequeue();
            }
        }
        #endregion

        #region Handlers
        public static void RegisterHandlers( Assembly assembly )
        {
            foreach ( var type in assembly.GetTypes() )
            {
                var attribs = type.GetCustomAttributes( typeof( HandlerAttribute ), false ) as HandlerAttribute[];

                if ( attribs == null || attribs.Length == 0 )
                    continue;

                registeredHandlers.Add( type );
            }
        }

        public T GetHandler<T>()
            where T : ClientHandler
        {
            var type = typeof( T );

            if ( !handlers.ContainsKey( type ) )
                return null;

            return handlers[ type ] as T;
        }


        void SetupHandlers()
        {
            foreach ( var type in registeredHandlers )
            {
                ClientHandler handler = Activator.CreateInstance( type, true ) as ClientHandler;

                handler.Setup( this );

                handlers.Add( type, handler );
            }
        }
        #endregion
    }
}
