/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SteamKit2.Internal;

namespace SteamKit2
{
    public sealed partial class SteamWorkshop
    {
        /// <summary>
        /// This callback is received in response to calling <see cref="RequestPublishedFileDetails"/>.
        /// </summary>
        public sealed class PublishedFileDetailsCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the file ID.
            /// </summary>
            public PublishedFileID FileID { get; private set; }

            /// <summary>
            /// Gets the SteamID of the creator of this file.
            /// </summary>
            public SteamID Creator { get; private set; }

            /// <summary>
            /// Gets the AppID used during creation.
            /// </summary>
            public uint CreatorAppID { get; private set; }
            /// <summary>
            /// Gets the AppID used during consumption.
            /// </summary>
            public uint ConsumerAppID { get; private set; }

            /// <summary>
            /// Gets the handle for the UGC file this published file represents.
            /// </summary>
            public UGCHandle FileUGC { get; private set; }
            /// <summary>
            /// Gets the handle for the UGC preview file this published file represents, normally an image or thumbnail.
            /// </summary>
            public UGCHandle PreviewFileUGC { get; private set; }

            /// <summary>
            /// Gets the title.
            /// </summary>
            public string Title { get; private set; }
            /// <summary>
            /// Gets the description.
            /// </summary>
            public string Description { get; private set; }

            /// <summary>
            /// Gets the creation time.
            /// </summary>
            public DateTime CreationTime { get; private set; }
            /// <summary>
            /// Gets the last update time.
            /// </summary>
            public DateTime UpdateTime { get; private set; }

            /// <summary>
            /// Gets the visiblity of this file.
            /// </summary>
            public EPublishedFileVisibility Visiblity { get; private set; }

            /// <summary>
            /// Gets a value indicating whether this instance is banned.
            /// </summary>
            public bool IsBanned { get; private set; }

            /// <summary>
            /// Gets the tags associated with this file.
            /// </summary>
            public ReadOnlyCollection<string> Tags { get; private set; }

            /// <summary>
            /// Gets the name of the file.
            /// </summary>
            public string FileName { get; private set; }

            /// <summary>
            /// Gets the size of the file.
            /// </summary>
            public uint FileSize { get; private set; }
            /// <summary>
            /// Gets the size of the preview file.
            /// </summary>
            public uint PreviewFileSize { get; private set; }

            /// <summary>
            /// Gets the URL.
            /// </summary>
            public string URL { get; private set; }


            internal PublishedFileDetailsCallback( CMsgClientUCMGetPublishedFileDetailsResponse msg )
            {
                this.Result = ( EResult )msg.eresult;

                this.FileID = msg.published_file_id;

                this.Creator = msg.creator_steam_id;

                this.CreatorAppID = msg.creator_app_id;
                this.ConsumerAppID = msg.consumer_app_id;

                this.FileUGC = msg.file_hcontent;
                this.PreviewFileUGC = msg.preview_hcontent;

                this.Title = msg.title;
                this.Description = msg.description;

                this.CreationTime = Utils.DateTimeFromUnixTime( msg.rtime32_created );
                this.UpdateTime = Utils.DateTimeFromUnixTime( msg.rtime32_updated );

                this.Visiblity = ( EPublishedFileVisibility )msg.visibility;

                this.IsBanned = msg.banned;

                this.Tags = new ReadOnlyCollection<string>( new List<string>( msg.tag ) );

                this.FileName = msg.filename;

                this.FileSize = msg.file_size;
                this.PreviewFileSize = msg.preview_file_size;

                this.URL = msg.url;
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="EnumeratePublishedFiles"/>.
        /// </summary>
        public sealed class PublishedFilesCallback : CallbackMsg
        {
            /// <summary>
            /// Represents the details of a single published file.
            /// </summary>
            public sealed class File
            {
                /// <summary>
                /// Gets the file ID.
                /// </summary>
                public PublishedFileID FileID { get; private set; }

                /// <summary>
                /// Gets the number of reports for this file.
                /// </summary>
                public int Reports { get; private set; }

                /// <summary>
                /// Gets the score of this file, based on up and down votes.
                /// </summary>
                public float Score { get; private set; }

                /// <summary>
                /// Gets the total count of up votes.
                /// </summary>
                public int UpVotes { get; private set; }
                /// <summary>
                /// Gets the total count of down votes.
                /// </summary>
                public int DownVotes { get; private set; }


                internal File( CMsgCREEnumeratePublishedFilesResponse.PublishedFileId file )
                {
                    this.FileID = file.published_file_id;

                    this.Reports = file.reports;

                    this.Score = file.score;

                    this.UpVotes = file.votes_for;
                    this.DownVotes = file.votes_against;
                }
            }


            /// <summary>
            /// Gets the result.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the list of enumerated files.
            /// </summary>
            public ReadOnlyCollection<File> Files { get; private set; }

            /// <summary>
            /// Gets the count of total results.
            /// </summary>
            public int TotalResults { get; private set; }


            internal PublishedFilesCallback( CMsgCREEnumeratePublishedFilesResponse msg )
            {
                this.Result = ( EResult )msg.eresult;

                var fileList = msg.published_files
                    .Select( f => new File( f ) )
                    .ToList();

                this.Files = new ReadOnlyCollection<File>( fileList );

                this.TotalResults = ( int )msg.total_results;
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="EnumerateUserPublishedFiles"/>.
        /// </summary>
        public sealed class UserPublishedFilesCallback : CallbackMsg
        {
            /// <summary>
            /// Represents the details of a single published file.
            /// </summary>
            public sealed class File
            {
                /// <summary>
                /// Gets the file ID.
                /// </summary>
                public PublishedFileID FileID { get; private set; }


                internal File( CMsgClientUCMEnumerateUserPublishedFilesResponse.PublishedFileId file )
                {
                    this.FileID = file.published_file_id;
                }
            }


            /// <summary>
            /// Gets the result.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the list of enumerated files.
            /// </summary>
            public ReadOnlyCollection<File> Files { get; private set; }

            /// <summary>
            /// Gets the count of total results.
            /// </summary>
            public int TotalResults { get; private set; }


            internal UserPublishedFilesCallback( CMsgClientUCMEnumerateUserPublishedFilesResponse msg )
            {
                this.Result = ( EResult )msg.eresult;

                var fileList = msg.published_files
                    .Select( f => new File( f ) )
                    .ToList();

                this.Files = new ReadOnlyCollection<File>( fileList );

                this.TotalResults = ( int )msg.total_results;
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="EnumerateUserPublishedFiles"/>.
        /// </summary>
        public sealed class UserSubscribedFilesCallback : CallbackMsg
        {
            /// <summary>
            /// Represents the details of a single published file.
            /// </summary>
            public sealed class File
            {
                /// <summary>
                /// Gets the file ID.
                /// </summary>
                public PublishedFileID FileID { get; private set; }

                /// <summary>
                /// Gets the time this file was subscribed to.
                /// </summary>
                public DateTime TimeSubscribed { get; private set; }


                internal File( CMsgClientUCMEnumerateUserSubscribedFilesResponse.PublishedFileId file )
                {
                    this.FileID = file.published_file_id;

                    this.TimeSubscribed = Utils.DateTimeFromUnixTime( file.rtime32_subscribed );
                }
            }


            /// <summary>
            /// Gets the result.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the list of enumerated files.
            /// </summary>
            public ReadOnlyCollection<File> Files { get; private set; }

            /// <summary>
            /// Gets the count of total results.
            /// </summary>
            public int TotalResults { get; private set; }


            internal UserSubscribedFilesCallback( CMsgClientUCMEnumerateUserSubscribedFilesResponse msg )
            {
                this.Result = ( EResult )msg.eresult;

                var fileList = msg.subscribed_files
                    .Select( f => new File( f ) )
                    .ToList();

                this.Files = new ReadOnlyCollection<File>( fileList );

                this.TotalResults = ( int )msg.total_results;
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="EnumeratePublishedFilesByUserAction"/>.
        /// </summary>
        public sealed class UserActionPublishedFilesCallback : CallbackMsg
        {
            /// <summary>
            /// Represents the details of a single published file.
            /// </summary>
            public sealed class File
            {
                /// <summary>
                /// Gets the file ID.
                /// </summary>
                public PublishedFileID FileID { get; private set; }

                /// <summary>
                /// Gets the timestamp of this file.
                /// </summary>
                public DateTime Timestamp { get; private set; }


                internal File( CMsgClientUCMEnumeratePublishedFilesByUserActionResponse.PublishedFileId file )
                {
                    this.FileID = file.published_file_id;

                    this.Timestamp = Utils.DateTimeFromUnixTime( file.rtime_time_stamp );
                }
            }


            /// <summary>
            /// Gets the result.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the list of enumerated files.
            /// </summary>
            public ReadOnlyCollection<File> Files { get; private set; }

            /// <summary>
            /// Gets the count of total results.
            /// </summary>
            public int TotalResults { get; private set; }


            internal UserActionPublishedFilesCallback( CMsgClientUCMEnumeratePublishedFilesByUserActionResponse msg )
            {
                this.Result = ( EResult )msg.eresult;

                var fileList = msg.published_files
                    .Select( f => new File( f ) )
                    .ToList();

                this.Files = new ReadOnlyCollection<File>( fileList );

                this.TotalResults = ( int )msg.total_results;
            }
        }
    }
}
