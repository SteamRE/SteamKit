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


        public abstract void HandleMsg( EMsg eMsg, byte[] data );
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
            lock ( callbackLock )
            {
                if ( callbackQueue.Count == 0 )
                    return null;

                return callbackQueue.Peek();
            }
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
            lock ( callbackLock )
            {
                if ( msg == null )
                    return;

                callbackQueue.Enqueue( msg );
            }
        }



        protected override void OnClientMsgReceived( ClientMsgEventArgs e )
        {
            base.OnClientMsgReceived( e );

            foreach ( var kvp in Handlers )
            {
                kvp.Value.HandleMsg( e.EMsg, e.Data );
            }
        }

    }
}
