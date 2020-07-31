using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for initializing Steam trades with other clients.
    /// </summary>
    public sealed partial class SteamScreenshots : ClientMsgHandler
    {
        /// <summary>
        /// Width of a screenshot thumnail
        /// </summary>
        public const uint ScreenshotThumbnailWidth = 200;

        /// <summary>
        /// Represents the details required to add a screenshot
        /// </summary>
        public sealed class ScreenshotDetails
        {
            /// <summary>
            /// Gets or sets the Steam game ID this screenshot belongs to
            /// </summary>
            /// <value>The game ID.</value>
            public GameID? GameID { get; set; }

            /// <summary>
            /// Gets or sets the UFS image filepath.
            /// </summary>
            /// <value>The UFS image filepath.</value>
            public string? UFSImageFilePath { get; set; }
            /// <summary>
            /// Gets or sets the UFS thumbnail filepath.
            /// </summary>
            /// <value>The UFS thumbnail filepath.</value>
            public string? UFSThumbnailFilePath { get; set; }

            /// <summary>
            /// Gets or sets the screenshot caption
            /// </summary>
            /// <value>The screenshot caption.</value>
            public string? Caption { get; set; }
            /// <summary>
            /// Gets or sets the screenshot privacy
            /// </summary>
            /// <value>The screenshot privacy.</value>
            public EUCMFilePrivacyState Privacy { get; set; }

            /// <summary>
            /// Gets or sets the screenshot width
            /// </summary>
            /// <value>The screenshot width.</value>
            public uint Width { get; set; }
            /// <summary>
            /// Gets or sets the screenshot height
            /// </summary>
            /// <value>The screenshot height.</value>
            public uint Height { get; set; }

            /// <summary>
            /// Gets or sets the creation time
            /// </summary>
            /// <value>The creation time.</value>
            public DateTime CreationTime { get; set; }
            /// <summary>
            /// Gets or sets whether or not the screenshot contains spoilers
            /// </summary>
            /// <value>Whether or not the screenshot contains spoilers.</value>
            public bool ContainsSpoilers { get; set; }


            /// <summary>
            /// Initializes a new instance of the <see cref="ScreenshotDetails"/> class.
            /// </summary>
            public ScreenshotDetails()
            {
            }
        }


        Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        internal SteamScreenshots()
        {
            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ClientUCMAddScreenshotResponse, HandleUCMAddScreenshot },
            };
        }


        /// <summary>
        /// Adds a screenshot to the user's screenshot library. The screenshot image and thumbnail must already exist on the UFS.
        /// Results are returned in a <see cref="ScreenshotAddedCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="details">The details of the screenshot.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="ScreenshotAddedCallback"/>.</returns>
        public AsyncJob<ScreenshotAddedCallback> AddScreenshot( ScreenshotDetails details )
        {
            if ( details == null )
            {
                throw new ArgumentNullException( nameof(details) );
            }

            var msg = new ClientMsgProtobuf<CMsgClientUCMAddScreenshot>( EMsg.ClientUCMAddScreenshot );
            msg.SourceJobID = Client.GetNextJobID();

            if ( details.GameID != null )
            {
                msg.Body.appid = details.GameID.AppID;
            }

            msg.Body.caption = details.Caption;
            msg.Body.filename = details.UFSImageFilePath;
            msg.Body.permissions = ( uint )details.Privacy;
            msg.Body.thumbname = details.UFSThumbnailFilePath;
            msg.Body.width = details.Width;
            msg.Body.height = details.Height;
            msg.Body.rtime32_created = ( uint )DateUtils.DateTimeToUnixTime( details.CreationTime );
            msg.Body.spoiler_tag = details.ContainsSpoilers;

            Client.Send( msg );

            return new AsyncJob<ScreenshotAddedCallback>( this.Client, msg.SourceJobID );
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            if ( packetMsg == null )
            {
                throw new ArgumentNullException( nameof(packetMsg) );
            }

            bool haveFunc = dispatchMap.TryGetValue( packetMsg.MsgType, out var handlerFunc );

            if ( !haveFunc )
            {
                // ignore messages that we don't have a handler function for
                return;
            }

            handlerFunc( packetMsg );
        }


        #region ClientMsg Handlers
        void HandleUCMAddScreenshot( IPacketMsg packetMsg )
        {
            var resp = new ClientMsgProtobuf<CMsgClientUCMAddScreenshotResponse>( packetMsg );

            var callback = new ScreenshotAddedCallback(resp.TargetJobID, resp.Body);
            Client.PostCallback( callback );
        }
        #endregion

    }
}
