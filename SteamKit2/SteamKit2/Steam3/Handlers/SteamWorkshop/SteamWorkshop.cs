/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for requesting files published on the Steam Workshop.
    /// </summary>
    public sealed partial class SteamWorkshop : ClientMsgHandler
    {
        internal SteamWorkshop()
        {
        }


        /// <summary>
        /// Requests details for a given published workshop file.
        /// Results are returned in a <see cref="PublishedFileDetailsCallback"/> from a <see cref="SteamClient.JobCallback&lt;T&gt;"/>.
        /// </summary>
        /// <param name="publishedFileId">The file ID being requested.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID RequestPublishedFileDetails( PublishedFileID publishedFileId )
        {
            var request = new ClientMsgProtobuf<CMsgClientUCMGetPublishedFileDetails>( EMsg.ClientUCMGetPublishedFileDetails );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.published_file_id = publishedFileId;

            Client.Send( request );

            return request.SourceJobID;
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
        /// Results are returned in a <see cref="UserPublishedFilesCallback"/> from a <see cref="SteamClient.JobCallback&lt;T&gt;"/>.
        /// </summary>
        /// <param name="details">The specific details of the request.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
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
        /// Results are returned in a <see cref="UserSubscribedFilesCallback"/> from a <see cref="SteamClient.JobCallback&lt;T&gt;"/>.
        /// </summary>
        /// <param name="details">The specific details of the request.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
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
        /// Results are returned in a <see cref="UserActionPublishedFilesCallback"/> from a <see cref="SteamClient.JobCallback&lt;T&gt;"/>.
        /// </summary>
        /// <param name="details">The specific details of the request.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
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
        /// Results are returned in a <see cref="PublishedFilesCallback"/> from a <see cref="SteamClient.JobCallback&lt;T&gt;"/>.
        /// </summary>
        /// <param name="details">The specific details of the request.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
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
            switch ( packetMsg.MsgType )
            {
                case EMsg.CREEnumeratePublishedFilesResponse:
                    HandleEnumPublishedFiles( packetMsg );
                    break;

                case EMsg.ClientUCMEnumerateUserPublishedFilesResponse:
                    HandleEnumUserPublishedFiles( packetMsg );
                    break;

                case EMsg.ClientUCMEnumerateUserSubscribedFilesResponse:
                    HandleEnumUserSubscribedFiles( packetMsg );
                    break;

                case EMsg.ClientUCMEnumeratePublishedFilesByUserActionResponse:
                    HandleEnumPublishedFilesByAction( packetMsg );
                    break;

                case EMsg.ClientUCMGetPublishedFileDetailsResponse:
                    HandlePublishedFileDetails( packetMsg );
                    break;
            }
        }



        #region ClientMsg Handlers
        void HandleEnumPublishedFiles( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgCREEnumeratePublishedFilesResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new PublishedFilesCallback( Client, response.Body );
            var callback = new SteamClient.JobCallback<PublishedFilesCallback>( Client, response.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new PublishedFilesCallback( response.Body );
            var callback = new SteamClient.JobCallback<PublishedFilesCallback>( response.TargetJobID, innerCallback );
            Client.PostCallback( callback );
#endif
        }
        void HandleEnumUserPublishedFiles( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgClientUCMEnumerateUserPublishedFilesResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new UserPublishedFilesCallback( Client, response.Body );
            var callback = new SteamClient.JobCallback<UserPublishedFilesCallback>( Client, response.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new UserPublishedFilesCallback( response.Body );
            var callback = new SteamClient.JobCallback<UserPublishedFilesCallback>( response.TargetJobID, innerCallback );
            Client.PostCallback( callback );
#endif
        }
        void HandleEnumUserSubscribedFiles( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgClientUCMEnumerateUserSubscribedFilesResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new UserSubscribedFilesCallback( Client, response.Body );
            var callback = new SteamClient.JobCallback<UserSubscribedFilesCallback>( Client, response.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new UserSubscribedFilesCallback( response.Body );
            var callback = new SteamClient.JobCallback<UserSubscribedFilesCallback>( response.TargetJobID, innerCallback );
            Client.PostCallback( callback );
#endif
        }
        void HandleEnumPublishedFilesByAction( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgClientUCMEnumeratePublishedFilesByUserActionResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new UserActionPublishedFilesCallback( Client, response.Body );
            var callback = new SteamClient.JobCallback<UserActionPublishedFilesCallback>( Client, response.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new UserActionPublishedFilesCallback( response.Body );
            var callback = new SteamClient.JobCallback<UserActionPublishedFilesCallback>( response.TargetJobID, innerCallback );
            Client.PostCallback( callback );
#endif
        }
        void HandlePublishedFileDetails( IPacketMsg packetMsg )
        {
            var details = new ClientMsgProtobuf<CMsgClientUCMGetPublishedFileDetailsResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new PublishedFileDetailsCallback( Client, details.Body );
            var callback = new SteamClient.JobCallback<PublishedFileDetailsCallback>( Client, details.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new PublishedFileDetailsCallback( details.Body );
            var callback = new SteamClient.JobCallback<PublishedFileDetailsCallback>( packetMsg.TargetJobID, innerCallback );
            Client.PostCallback( callback );
#endif
        }
        #endregion

    }
}
