/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;


namespace DepotDownloader2
{
    class Program
    {
        static void Main( string[] args )
        {
            ServerCache.Build();
            CDRManager.Update();


            if ( args.Length == 0 )
            {
                Options.ShowHelp();
                return;
            }

            Options.Parse( args );

            if ( string.IsNullOrEmpty( Options.Game ) )
            {
                Log.WriteLine( "Error: Missing `game` parameter. Try --help." );
                return;
            }

            if ( string.IsNullOrEmpty( Options.Directory ) )
            {
                Log.WriteLine( "Error: Missing `directory` parameter. Try --help." );
                return;
            }

            var gameList = CDRManager.GetDepotsForGame( Options.Game );

            foreach ( var depotId in gameList )
            {
                ContentDownloader.Install( depotId );
            }
        }
    }
}
