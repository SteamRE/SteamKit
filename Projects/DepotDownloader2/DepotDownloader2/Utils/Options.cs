/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;

namespace DepotDownloader2
{
    static class Options
    {
        static OptionSet optionSet = new OptionSet
        {
            { "v|verbose", "Displays verbose logging information.", v => Verbose = true },
            { "h|?|help", "Displays help information.", v => ShowHelp() },
            { "list|list_games", "List possible options for the `game` option.", v => ShowGames() },

            { "g=|game=", "The game to download. (required)", v => SetGame( v ) },
            { "d=|dir=|directory=", "The directory to download the game to. (required)", v => Directory = v },

            { "c=|cell=|cellid=", "Force content downloading through a specific steam2 cell. (optional)", ( uint v ) => CellID = v },
            { "ver|verify", "Force downloading of all updated files. (optional)", v => Verify = true },

            { "u=|user=|username=", "Username to use for restricted content. (optional)", v => Username = v },
            { "p=|pass=|password=", "Password to use for restricted content. (optional)", v => Password = v },
        };


        public static bool Verbose { get; private set; }

        public static string Game { get; private set; }
        public static string Directory { get; private set; }

        public static uint CellID { get; private set; }
        public static bool Verify { get; private set; }

        public static string Username { get; private set; }
        public static string Password { get; private set; }

        public static bool DidAction { get; private set; }


        public static List<string> Parse( IEnumerable<string> args )
        {
            var result = optionSet.Parse( args );

            if ( result.Count > 0 )
            {
                Log.WriteLine( "Warning: Unhandled arguments: {0}", string.Join( ", ", result.ToArray() ) );
                Log.WriteLine();
            }

            return result;
        }

        public static void ShowHelp()
        {
            DidAction = true;

            Log.WriteLine( "Usage:" );
            Log.WriteLine( "  dd2 <options>" );
            Log.WriteLine();
            Log.WriteLine( "Options: " );

            optionSet.WriteOptionDescriptions( Console.Out );

            Log.WriteLine();
        }

        static void SetGame( string game )
        {
            var games = CDRManager.GetGamesInRange( 0, int.MaxValue );

            if ( !games.Contains( game ) )
            {
                Log.WriteLine( "Error: Unknown `game` argument. Try --list." );
                return;
            }

            Game = game;
        }


        static void ShowGames()
        {
            DidAction = true;

            App serverInfo = CDRManager.GetApp( 4 );

            var sourceGames = CDRManager.GetGamesInRange( 200, 999 ).ToList(); // source range
            var thirdPartyGames = CDRManager.GetGamesInRange( 1000, int.MaxValue ).ToList();

            sourceGames.Sort( StringComparer.OrdinalIgnoreCase );
            thirdPartyGames.Sort( StringComparer.OrdinalIgnoreCase );

            Log.WriteLine( "`game` options for Source DS install:\n" );
            foreach ( var game in sourceGames )
                Log.WriteLine( "\t\"{0}\"", game );

            Log.WriteLine( "\n`game` options for Third-Party game servers:\n" );
            foreach ( var game in thirdPartyGames )
                Log.WriteLine( "\t\"{0}\"", game );
        }
    }
}
