using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

using SteamKit2;

//
// Sample 6: SteamGuard
//
// this sample goes into detail for how to handle steamguard protected accounts and how to login to them
//
// SteamGuard works by enforcing a two factor authentication scheme
// upon first logon to an account with SG enabled, the steam server will email an authcode to the validated address of the account
// this authcode token can be used as the second factor during logon, but the token has a limited time span in which it is valid
//
// after a client logs on using the authcode, the steam server will generate a blob of random data that the client stores called a "sentry file"
// this sentry file is then used in all subsequent logons as the second factor
// ownership of this file provides proof that the machine being used to logon is owned by the client in question
//
// the usual login flow is thus:
// 1. connect to the server
// 2. logon to account with only username and password
// at this point, if the account is steamguard protected, the LoggedOnCallback will have a result of AccountLogonDenied
// the server will disconnect the client and email the authcode
//
// the login flow must then be restarted:
// 1. connect to server
// 2. logon to account using username, password, and authcode
// at this point, login wil succeed and a UpdateMachineAuthCallback callback will be posted with the sentry file data from the steam server
// the client will save the file, and reply to the server informing that it has accepted the sentry file
// 
// all subsequent logons will use this flow:
// 1. connect to server
// 2. logon to account using username, password, and sha-1 hash of the sentry file


namespace Sample6_SteamGuard
{
    class Program
    {
        static SteamClient steamClient;
        static CallbackManager manager;

        static SteamUser steamUser;

        static bool isRunning;

        static string user, pass;
        static string authCode;


        static void Main( string[] args )
        {
            if ( args.Length < 2 )
            {
                Console.WriteLine( "Sample5: No username and password specified!" );
                return;
            }

            // save our logon details
            user = args[ 0 ];
            pass = args[ 1 ];

            // create our steamclient instance
            steamClient = new SteamClient();
            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager( steamClient );

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            new Callback<SteamClient.ConnectedCallback>( OnConnected, manager );
            new Callback<SteamClient.DisconnectedCallback>( OnDisconnected, manager );

            new Callback<SteamUser.LoggedOnCallback>( OnLoggedOn, manager );
            new Callback<SteamUser.LoggedOffCallback>( OnLoggedOff, manager );

            // this callback is triggered when the steam servers wish for the client to store the sentry file
            new JobCallback<SteamUser.UpdateMachineAuthCallback>( OnMachineAuth, manager );

            isRunning = true;

            Console.WriteLine( "Connecting to Steam..." );

            // initiate the connection
            steamClient.Connect();

            // create our callback handling loop
            while ( isRunning )
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks( TimeSpan.FromSeconds( 1 ) );
            }
        }

        static void OnConnected( SteamClient.ConnectedCallback callback )
        {
            if ( callback.Result != EResult.OK )
            {
                Console.WriteLine( "Unable to connect to Steam: {0}", callback.Result );

                isRunning = false;
                return;
            }

            Console.WriteLine( "Connected to Steam! Logging in '{0}'...", user );

            byte[] sentryHash = null;
            if ( File.Exists( "sentry.bin" ) )
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes( "sentry.bin" );
                sentryHash = CryptoHelper.SHAHash( sentryFile );
            }

            steamUser.LogOn( new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,
                
                // in this sample, we pass in an additional authcode
                // this value will be null (which is the default) for our first logon attempt
                AuthCode = authCode,

                // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                SentryFileHash = sentryHash,
            } );
        }

        static void OnDisconnected( SteamClient.DisconnectedCallback callback )
        {
            // after recieving an AccountLogonDenied, we'll be disconnected from steam
            // so after we read an authcode from the user, we need to reconnect to begin the logon flow again

            Console.WriteLine( "Disconnected from Steam, reconnecting in 5..." );

            Thread.Sleep( TimeSpan.FromSeconds( 5 ) );

            steamClient.Connect();
        }

        static void OnLoggedOn( SteamUser.LoggedOnCallback callback )
        {
            if ( callback.Result == EResult.AccountLogonDenied )
            {
                Console.WriteLine( "This account is SteamGuard protected!" );
                Console.Write( "Please enter the auth code sent to the email at {0}: ", callback.EmailDomain );

                authCode = Console.ReadLine();
                return;
            }

            if ( callback.Result != EResult.OK )
            {
                Console.WriteLine( "Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult );

                isRunning = false;
                return;
            }

            Console.WriteLine( "Successfully logged on!" );

            // at this point, we'd be able to perform actions on Steam
        }

        static void OnLoggedOff( SteamUser.LoggedOffCallback callback )
        {
            Console.WriteLine( "Logged off of Steam: {0}", callback.Result );
        }

        static void OnMachineAuth( SteamUser.UpdateMachineAuthCallback callback, JobID jobId )
        {
            Console.WriteLine( "Updating sentryfile..." );

            byte[] sentryHash = CryptoHelper.SHAHash( callback.Data );

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"
            File.WriteAllBytes( "sentry.bin", callback.Data );

            // inform the steam servers that we're accepting this sentry file
            steamUser.SendMachineAuthResponse( new SteamUser.MachineAuthDetails
            {
                JobID = jobId,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = callback.Data.Length,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            } );

            Console.WriteLine( "Done!" );
        }
    }
}
