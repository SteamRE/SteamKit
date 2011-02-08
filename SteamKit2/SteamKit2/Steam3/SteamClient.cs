/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public abstract class CallbackMsg
    {
    }


    public abstract class ClientMsgHandler
    {
        public string Name { get; private set; }

        protected SteamClient Client { get; private set; }


        public ClientMsgHandler( string name )
        {
            this.Name = name;
        }

        internal void Setup( SteamClient client )
        {
            this.Client = client;
        }


        public abstract void HandleMsg( ClientMsgEventArgs e );
    }

    public class ConnectedCallback : CallbackMsg
    {
        public EResult Result { get; set; }

        internal ConnectedCallback( MsgChannelEncryptResult result )
        {
            this.Result = result.Result;
        }
    }

    public sealed class SteamClient : CMClient
    {
        public Dictionary<string, ClientMsgHandler> Handlers { get; private set; }

        object callbackLock = new object();
        Queue<CallbackMsg> callbackQueue;


        public SteamClient()
        {
            this.Handlers = new Dictionary<string, ClientMsgHandler>( StringComparer.OrdinalIgnoreCase );

            callbackQueue = new Queue<CallbackMsg>();

            // add this library's handlers
            this.AddHandler( new SteamUser() );
            this.AddHandler( new SteamFriends() );
        }

        // handlers
        public void AddHandler( ClientMsgHandler handler )
        {
            if ( Handlers.ContainsKey( handler.Name ) )
                return;

            Handlers[ handler.Name ] = handler;

            handler.Setup( this );
        }
        public void RemoveHandler( string handler )
        {
            if ( !Handlers.ContainsKey( handler ) )
                return;

            Handlers.Remove( handler );
        }
        public void RemoveHandler( ClientMsgHandler handler )
        {
            this.RemoveHandler( handler.Name );
        }
        public T GetHandler<T>( string name ) where T : ClientMsgHandler
        {
            if ( Handlers.ContainsKey( name ) )
                return ( T )Handlers[ name ];

            return null;
        }


        // callbacks
        public CallbackMsg GetCallback()
        {
            CallbackMsg msg = null;

            lock ( callbackLock )
            {
                if ( callbackQueue.Count > 0 )
                    msg = callbackQueue.Peek();
            }

            return msg;
        }
        public void FreeLastCallback()
        {
            lock ( callbackLock )
            {
                if ( callbackQueue.Count == 0 )
                    return;

                callbackQueue.Dequeue();
            }
        }
        public void PostCallback( CallbackMsg msg )
        {
            if ( msg == null )
                return;

            lock ( callbackLock )
            {
                callbackQueue.Enqueue( msg );
            }
        }



        protected override void OnClientMsgReceived( ClientMsgEventArgs e )
        {

            if ( e.EMsg == EMsg.ChannelEncryptResult )
                HandleEncryptResult( e );

            // pass along the clientmsg to all registered handlers
            foreach ( var kvp in Handlers )
            {
                kvp.Value.HandleMsg( e );
            }
        }


        void HandleEncryptResult( ClientMsgEventArgs e )
        {
            // if the EResult is OK, we've finished the crypto handshake and can send commands (such as LogOn)
            var encResult = new ClientMsg<MsgChannelEncryptResult, MsgHdr>( e.Data );

            if ( encResult.Msg.Result == EResult.OK )
            {
                //  we've connected
                PostCallback( new ConnectedCallback( encResult.Msg ) );
            }
        }

    }
}
