/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.ObjectModel;
using System.Linq;
using SteamKit2.Internal;

namespace SteamKit2
{
    public sealed partial class SteamWorkshop
    {
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
