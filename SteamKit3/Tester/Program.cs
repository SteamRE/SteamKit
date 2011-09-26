using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit3;
using System.Threading;

namespace Tester
{
    class Client
    {
        string username, password;

        SteamClient client;

        SteamUser user;

        Callback<SteamClient.ConnectedCallback> connectedCallback;
        Callback<SteamUser.LoggedOnCallback> loggedOnCallback;


        public Client()
        {
            Console.Write( "Username: " );
            username = Console.ReadLine();

            Console.Write( "Password: " );
            password = Console.ReadLine();

            client = new SteamClient();

            user = client.GetHandler<SteamUser>();

            connectedCallback = new Callback<SteamClient.ConnectedCallback>( OnConnected, client );
            loggedOnCallback = new Callback<SteamUser.LoggedOnCallback>( OnLoggedOn, client );

            client.Connect();
        }

        public void RunFrame()
        {
            client.RunWaitCallbacks( TimeSpan.FromMilliseconds( 100 ) );
        }

        void OnConnected( SteamClient.ConnectedCallback msg )
        {
            Console.WriteLine( "Connect: {0}", msg.Result );

            user.Logon( new SteamUser.LogOnDetails() { Username = username, Password = password } );
        }
        void OnLoggedOn( SteamUser.LoggedOnCallback msg )
        {
            Console.WriteLine( "LogOn: {0}", msg.Result );
        }

    }
    class Program
    {
        static void Main( string[] args )
        {
            Client cli = new Client();

            while ( true )
            {
                cli.RunFrame();
            }
        }
    }
}
