using System;
using System.Linq;
using System.Reflection;

using SteamKit2.GC.Dota.Internal; // for CMsgDOTAMatch

namespace DotaMatchRequest
{
    class Program
    {
        static void Main( string[] args )
        {
            if ( args.Length < 3 )
            {
                PrintUsage();
                return;
            }

            string username = args[ 0 ];
            string password = args[ 1 ];

            uint matchId;
            if ( !uint.TryParse( args[ 2 ], out matchId ) )
            {
                Console.WriteLine( "Invalid Match ID!" );
                return;
            }

            DotaClient client = new DotaClient( username, password, matchId );

            // connect
            client.Connect();

            // wait for results of the match request
            client.Wait();

            // print off what steam gave us
            PrintMatchDetails( client.Match );
        }


        static void PrintMatchDetails( CMsgDOTAMatch match )
        {
            if ( match == null )
            {
                Console.WriteLine( "No match details to display" );
                return;
            }

            // use some lazy reflection to print out details
            var fields = typeof( CMsgDOTAMatch ).GetProperties( BindingFlags.Public | BindingFlags.Instance );

            foreach ( var field in fields.OrderBy( f => f.Name ) )
            {
                var value = field.GetValue( match, null );

                Console.WriteLine( "{0}: {1}", field.Name, value );
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine( "Usage:" );
            Console.WriteLine( "DotaMatchRequest <username> <password> <matchid>" );
        }
    }
}
