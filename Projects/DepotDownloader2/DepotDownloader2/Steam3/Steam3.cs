/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using System.Threading;

namespace DepotDownloader2
{
    enum Steam3Result
    {
        Finished,
        NoConnection,
        NoLogon,
        NoAppTicket,
    }

    class Steam3Info
    {
        public Steam3Result Result { get; set; }

        public EResult SteamResult { get; set; }

        public ulong SessionToken { get; set; }
        public byte[] AppTicket { get; set; }
        public byte[] Steam2Ticket { get; set; }
    }

    // TODO: use this?
    class Steam3Session
    {
        SteamClient client;

        SteamUser.LogOnDetails logonDetails;

        SteamUser user;
        SteamApps apps;

        Thread steam3Thread;

        ManualResetEvent finishedEvent;

        ManualResetEvent ticketEvent;
        ManualResetEvent tokenEvent;

        uint appId;


        public Steam3Info ResultInfo { get; private set; }


        public Steam3Session( string username, string password, uint appId )
        {
            this.appId = appId;

            finishedEvent = new ManualResetEvent( false );
            ticketEvent = new ManualResetEvent( false );
            tokenEvent = new ManualResetEvent( false );

            ResultInfo = new Steam3Info();

            steam3Thread = new Thread( RunCallbacks );
            steam3Thread.Start();

            client = new SteamClient();

            user = client.GetHandler<SteamUser>();
            apps = client.GetHandler<SteamApps>();

            logonDetails = new SteamUser.LogOnDetails()
            {
                Username = username,
                Password = password,
            };
        }


        public void Connect()
        {
            client.Connect();
        }

        public bool Wait( TimeSpan timeOut )
        {
            return finishedEvent.WaitOne( timeOut );
        }



        void RunCallbacks()
        {
            while ( true )
            {
                var callback = client.WaitForCallback( true, TimeSpan.FromMilliseconds( 100 ) );

                if ( tokenEvent.WaitOne( 0 ) && ticketEvent.WaitOne( 0 ) )
                    finishedEvent.Set();

                if ( finishedEvent.WaitOne( 0 ) )
                    break;

                callback.Handle<SteamClient.ConnectCallback>( OnConnect );

                callback.Handle<SteamUser.LogOnCallback>( OnLogon );
                callback.Handle<SteamUser.SessionTokenCallback>( OnSessionToken );

                callback.Handle<SteamApps.AppOwnershipTicketCallback>( OnAppTicket );
            }
        }

        void OnConnect( SteamClient.ConnectCallback call )
        {
            if ( call.Result != EResult.OK )
            {
                ResultInfo.Result = Steam3Result.NoConnection;
                ResultInfo.SteamResult = call.Result;

                finishedEvent.Set();
                return;
            }

            user.LogOn( logonDetails );
        }

        void OnLogon( SteamUser.LogOnCallback call )
        {
            if ( call.Result != EResult.OK )
            {
                ResultInfo.Result = Steam3Result.NoLogon;
                ResultInfo.SteamResult = call.Result;

                finishedEvent.Set();
                return;
            }

            ResultInfo.Steam2Ticket = call.Steam2Ticket;

            apps.GetAppOwnershipTicket( appId );
        }

        void OnSessionToken( SteamUser.SessionTokenCallback call )
        {
            ResultInfo.SessionToken = call.SessionToken;
            tokenEvent.Set();
        }

        void OnAppTicket( SteamApps.AppOwnershipTicketCallback call )
        {
            if ( call.Result != EResult.OK )
            {
                ResultInfo.Result = Steam3Result.NoAppTicket;
                ResultInfo.SteamResult = call.Result;

                ticketEvent.Set();
                return;
            }

            ResultInfo.AppTicket = call.Ticket;
            ticketEvent.Set();
        }

    }
}
