/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using SteamKit2.Internal;

namespace SteamKit2
{
    public sealed partial class SteamCloud
    {
        /// <summary>
        /// This callback is recieved in response to calling <see cref="RequestUGCDetails"/>.
        /// </summary>
        public sealed class UGCDetailsCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the App ID the UGC is for.
            /// </summary>
            public uint AppID { get; private set; }
            /// <summary>
            /// Gets the SteamID of the UGC's creator.
            /// </summary>
            public SteamID Creator { get; private set; }

            /// <summary>
            /// Gets the URL that the content is located at.
            /// </summary>
            public string URL { get; private set; }

            /// <summary>
            /// Gets the name of the file.
            /// </summary>
            public string FileName { get; private set; }
            /// <summary>
            /// Gets the size of the file.
            /// </summary>
            public uint FileSize { get; private set; }


            internal UGCDetailsCallback( JobID jobID, CMsgClientUFSGetUGCDetailsResponse msg )
            {
                JobID = jobID;

                Result = ( EResult )msg.eresult;

                AppID = msg.app_id;
                Creator = msg.steamid_creator;

                URL = msg.url;

                FileName = msg.filename;
                FileSize = msg.file_size;
            }
        }

        /// <summary>
        /// This callback is recieved in response to calling <see cref="GetSingleFileInfo"/>.
        /// </summary>
        public sealed class SingleFileInfoCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the App ID the file is for.
            /// </summary>
            public uint AppID { get; private set; }
            /// <summary>
            /// Gets the file name request.
            /// </summary>
            public string FileName { get; private set; }

            /// <summary>
            /// Gets the SHA hash of the file.
            /// </summary>
            public byte[] SHAHash { get; private set; }

            /// <summary>
            /// Gets the timestamp of the file.
            /// </summary>
            public DateTime Timestamp { get; private set; }
            /// <summary>
            /// Gets the size of the file.
            /// </summary>
            public uint FileSize { get; private set; }

            /// <summary>
            /// Gets if the file was explicity deleted by the user.
            /// </summary>
            public bool IsExplicitDelete { get; private set; }

            internal SingleFileInfoCallback(JobID jobID, CMsgClientUFSGetSingleFileInfoResponse msg)
            {
                JobID = jobID;

                Result = (EResult)msg.eresult;

                AppID = msg.app_id;
                FileName = msg.file_name;
                SHAHash = msg.sha_file;
                Timestamp = DateUtils.DateTimeFromUnixTime( msg.time_stamp );
                FileSize = msg.raw_file_size;
                IsExplicitDelete = msg.is_explicit_delete;
            }
        }

        /// <summary>
        /// This callback is recieved in response to calling <see cref="ShareFile"/>.
        /// </summary>
        public sealed class ShareFileCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the resulting UGC handle.
            /// </summary>
            public ulong UGCId { get; private set; }

            internal ShareFileCallback(JobID jobID, CMsgClientUFSShareFileResponse msg)
            {
                JobID = jobID;

                Result = (EResult)msg.eresult;

                UGCId = msg.hcontent;
            }
        }
    }
}
