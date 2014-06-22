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


            internal PublishedFilesCallback( JobID jobID, CMsgCREEnumeratePublishedFilesResponse msg )
            {
                this.JobID = jobID;

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


            internal UserPublishedFilesCallback( JobID jobID, CMsgClientUCMEnumerateUserPublishedFilesResponse msg)
            {
                this.JobID = jobID;

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

                    this.TimeSubscribed = DateUtils.DateTimeFromUnixTime( file.rtime32_subscribed );
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


            internal UserSubscribedFilesCallback( JobID jobID, CMsgClientUCMEnumerateUserSubscribedFilesResponse msg )
            {
                this.JobID = jobID;

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

                    this.Timestamp = DateUtils.DateTimeFromUnixTime( file.rtime_time_stamp );
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


            internal UserActionPublishedFilesCallback( JobID jobID, CMsgClientUCMEnumeratePublishedFilesByUserActionResponse msg )
            {
                this.JobID = jobID;

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
