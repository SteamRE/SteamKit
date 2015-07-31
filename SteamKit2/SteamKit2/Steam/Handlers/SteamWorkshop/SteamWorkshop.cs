/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for requesting files published on the Steam Workshop.
    /// </summary>
    public sealed partial class SteamWorkshop : ClientMsgHandler
    {
        Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        internal SteamWorkshop()
        {
            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.CREEnumeratePublishedFilesResponse, HandleEnumPublishedFiles },
                { EMsg.ClientUCMEnumerateUserPublishedFilesResponse, HandleEnumUserPublishedFiles },
                { EMsg.ClientUCMEnumerateUserSubscribedFilesResponse, HandleEnumUserSubscribedFiles },
                { EMsg.ClientUCMEnumeratePublishedFilesByUserActionResponse, HandleEnumPublishedFilesByAction },
            };
        }


        /// <summary>
        /// Represents the details of an enumeration request used for the local user's files.
        /// </summary>
        public sealed class EnumerationUserDetails
        {
            /// <summary>
            /// Gets or sets the AppID of the workshop to enumerate.
            /// </summary>
            /// <value>
            /// The AppID.
            /// </value>
            public uint AppID { get; set; }

            /// <summary>
            /// Gets or sets the sort order.
            /// This value is only used by <see cref="SteamWorkshop.EnumerateUserPublishedFiles"/>.
            /// </summary>
            /// <value>
            /// The sort order.
            /// </value>
            public uint SortOrder { get; set; }

            /// <summary>
            /// Gets or sets the start index.
            /// </summary>
            /// <value>
            /// The start index.
            /// </value>
            public uint StartIndex { get; set; }

            /// <summary>
            /// Gets or sets the user action to filter by.
            /// This value is only used by <see cref="SteamWorkshop.EnumeratePublishedFilesByUserAction"/>.
            /// </summary>
            /// <value>
            /// The user action.
            /// </value>
            public EWorkshopFileAction UserAction { get; set; }
        }

        /// <summary>
        /// Enumerates the list of published files for the current logged in user.
        /// Results are returned in a <see cref="UserPublishedFilesCallback"/>.
        /// </summary>
        /// <param name="details">The specific details of the request.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="UserPublishedFilesCallback"/>.</returns>
        public JobID EnumerateUserPublishedFiles( EnumerationUserDetails details )
        {
            var enumRequest = new ClientMsgProtobuf<CMsgClientUCMEnumerateUserPublishedFiles>( EMsg.ClientUCMEnumerateUserPublishedFiles );
            enumRequest.SourceJobID = Client.GetNextJobID();

            enumRequest.Body.app_id = details.AppID;
            enumRequest.Body.sort_order = details.SortOrder;
            enumRequest.Body.start_index = details.StartIndex;

            Client.Send( enumRequest );

            return enumRequest.SourceJobID;
        }
        /// <summary>
        /// Enumerates the list of subscribed files for the current logged in user.
        /// Results are returned in a <see cref="UserSubscribedFilesCallback"/>.
        /// </summary>
        /// <param name="details">The specific details of the request.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="UserSubscribedFilesCallback"/>.</returns>
        public JobID EnumerateUserSubscribedFiles( EnumerationUserDetails details )
        {
            var enumRequest = new ClientMsgProtobuf<CMsgClientUCMEnumerateUserSubscribedFiles>( EMsg.ClientUCMEnumerateUserSubscribedFiles );
            enumRequest.SourceJobID = Client.GetNextJobID();

            enumRequest.Body.app_id = details.AppID;
            enumRequest.Body.start_index = details.StartIndex;

            Client.Send( enumRequest );

            return enumRequest.SourceJobID;
        }

        /// <summary>
        /// Enumerates the list of published files for the current logged in user based on user action.
        /// Results are returned in a <see cref="UserActionPublishedFilesCallback"/>.
        /// </summary>
        /// <param name="details">The specific details of the request.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="UserActionPublishedFilesCallback"/>.</returns>
        public JobID EnumeratePublishedFilesByUserAction( EnumerationUserDetails details )
        {
            var enumRequest = new ClientMsgProtobuf<CMsgClientUCMEnumeratePublishedFilesByUserAction>( EMsg.ClientUCMEnumeratePublishedFilesByUserAction );
            enumRequest.SourceJobID = Client.GetNextJobID();

            enumRequest.Body.action = ( int )details.UserAction;
            enumRequest.Body.app_id = details.AppID;
            enumRequest.Body.start_index = details.StartIndex;

            Client.Send( enumRequest );

            return enumRequest.SourceJobID;
        }

        /// <summary>
        /// Represents the details of an enumeration request for all published files.
        /// </summary>
        public sealed class EnumerationDetails
        {
            /// <summary>
            /// Gets or sets the AppID of the workshop to enumerate.
            /// </summary>
            /// <value>
            /// The AppID.
            /// </value>
            public uint AppID { get; set; }

            /// <summary>
            /// Gets or sets the type of the enumeration.
            /// </summary>
            /// <value>
            /// The type.
            /// </value>
            public EWorkshopEnumerationType Type { get; set; }

            /// <summary>
            /// Gets or sets the start index.
            /// </summary>
            /// <value>
            /// The start index.
            /// </value>
            public uint StartIndex { get; set; }

            /// <summary>
            /// Gets or sets the days.
            /// </summary>
            /// <value>
            /// The days.
            /// </value>
            public uint Days { get; set; }
            /// <summary>
            /// Gets or sets the number of results to return.
            /// </summary>
            /// <value>
            /// The number of results.
            /// </value>
            public uint Count { get; set; }

            /// <summary>
            /// Gets the list of tags to enumerate.
            /// </summary>
            public List<string> Tags { get; private set; }
            /// <summary>
            /// Gets the list of user tags to enumerate.
            /// </summary>
            public List<string> UserTags { get; private set; }


            /// <summary>
            /// Initializes a new instance of the <see cref="EnumerationDetails"/> class.
            /// </summary>
            public EnumerationDetails()
            {
                Tags = new List<string>();
                UserTags = new List<string>();
            }
        }

        /// <summary>
        /// Enumerates the list of all published files on the Steam workshop.
        /// Results are returned in a <see cref="PublishedFilesCallback"/>.
        /// </summary>
        /// <param name="details">The specific details of the request.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="PublishedFilesCallback"/>.</returns>
        public JobID EnumeratePublishedFiles( EnumerationDetails details )
        {
            var enumRequest = new ClientMsgProtobuf<CMsgCREEnumeratePublishedFiles>( EMsg.CREEnumeratePublishedFiles );
            enumRequest.SourceJobID = Client.GetNextJobID();

            enumRequest.Body.app_id = details.AppID;

            enumRequest.Body.query_type = ( int )details.Type;

            enumRequest.Body.start_index = details.StartIndex;

            enumRequest.Body.days = details.Days;
            enumRequest.Body.count = details.Count;

            enumRequest.Body.tags.AddRange( details.Tags );
            enumRequest.Body.user_tags.AddRange( details.UserTags );

            Client.Send( enumRequest );

            return enumRequest.SourceJobID;
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            Action<IPacketMsg> handlerFunc;
            bool haveFunc = dispatchMap.TryGetValue( packetMsg.MsgType, out handlerFunc );

            if ( !haveFunc )
            {
                // ignore messages that we don't have a handler function for
                return;
            }

            handlerFunc( packetMsg );
        }


        #region ClientMsg Handlers
        void HandleEnumPublishedFiles( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgCREEnumeratePublishedFilesResponse>( packetMsg );

            var callback = new PublishedFilesCallback(response.TargetJobID, response.Body);
            Client.PostCallback( callback );
        }
        void HandleEnumUserPublishedFiles( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgClientUCMEnumerateUserPublishedFilesResponse>( packetMsg );

            var callback = new UserPublishedFilesCallback(response.TargetJobID, response.Body);
            Client.PostCallback( callback );
        }
        void HandleEnumUserSubscribedFiles( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgClientUCMEnumerateUserSubscribedFilesResponse>( packetMsg );

            var callback = new UserSubscribedFilesCallback( response.TargetJobID, response.Body );
            Client.PostCallback( callback );
        }
        void HandleEnumPublishedFilesByAction( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgClientUCMEnumeratePublishedFilesByUserActionResponse>( packetMsg );

            var callback = new UserActionPublishedFilesCallback(response.TargetJobID, response.Body);
            Client.PostCallback( callback );
        }
        #endregion

    }
}
