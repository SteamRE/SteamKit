using System;

using SteamKit2;
using SteamKit2.Internal; // this namespace stores the generated protobuf message structures

namespace Sample2_Extending
{
    class MyHandler : ClientMsgHandler
    {
        // define our custom callback class
        // this will pass data back to the user of the handler
        public class MyCallback : CallbackMsg
        {
            public EResult Result { get; private set; }

            // generally we don't want user code to instantiate callback objects,
            // but rather only let handlers create them
            internal MyCallback( EResult res )
            {
                Result = res;
            }
        }


        // handlers can also define functions which can send data to the steam servers
        public void LogOff( string user, string pass )
        {
            var logOffMessage = new ClientMsgProtobuf<CMsgClientLogOff>( EMsg.ClientLogOff );

            Client.Send( logOffMessage );
        }

        // some other useful function
        public void DoSomething()
        {
            // this function could send some other message or perform some other logic

            // ...
            // Client.Send( somethingElse ); // etc
            // ...
        }

        public override void HandleMsg( IPacketMsg packetMsg )
        {
            // this function is called when a message arrives from the Steam network
            // the SteamClient class will pass the message along to every registered ClientMsgHandler

            // the MsgType exposes the EMsg (type) of the message
            switch ( packetMsg.MsgType )
            {

                // we want to custom handle this message, for the sake of an example
                case EMsg.ClientLogOnResponse:
                    HandleLogonResponse( packetMsg );
                    break;

            }
        }

        void HandleLogonResponse( IPacketMsg packetMsg )
        {
            // in order to get at the message contents, we need to wrap the packet message
            // in an object that gives us access to the message body
            var logonResponse = new ClientMsgProtobuf<CMsgClientLogonResponse>( packetMsg );

            // the raw body of the message often doesn't make use of useful types, so we need to
            // cast them to types that are prettier for the user to handle
            EResult result = ( EResult )logonResponse.Body.eresult;

            // our handler will simply display a message in the console, and then post our custom callback with the result of logon
            Console.WriteLine( "HandleLogonResponse: {0}", result );

            // post the callback to be consumed by user code
            Client.PostCallback( new MyCallback( result ) );
        }
    }
}
