using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit3;

namespace Tester
{
    class Program
    {
        static void Main( string[] args )
        {
            Console.Write( "Username: " );
            string username = Console.ReadLine();

            Console.Write( "Password: " );
            string password = Console.ReadLine();

            SteamClient sc = new SteamClient();
            SteamUser steamUser = sc.GetHandler<SteamUser>();

            sc.Connect();

            while ( true )
            {
                CallbackMsg msg = sc.WaitForCallback();


                Console.WriteLine( "Got callback: {0}", msg );

                if ( msg.GetType() == typeof( SteamClient.ConnectedCallback ) )
                {
                    steamUser.Logon( new SteamUser.LogOnDetails() { Username = username, Password = password } );
                }
            }
        }
    }
}
