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
        /// This callback is received in response to calling <see cref="RequestUGCDetails"/>.
        /// </summary>
        public sealed class UGCDetailsCallback
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
            /// Gets the UGC ID.
            /// </summary>
            public ulong UGCId { get; private set; }

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

            /// <summary>
            /// Gets the timestamp of the file.
            /// </summary>
            public DateTime Timestamp { get; private set; }

            /// <summary>
            /// Gets the SHA hash of the file.
            /// </summary>
            public string FileSHA { get; private set; }

            /// <summary>
            /// Gets the compressed size of the file.
            /// </summary>
            public uint CompressedFileSize { get; private set; }

            /// <summary>
            /// Gets the rangecheck host.
            /// </summary>
            public string RangecheckHost { get; private set; }


            internal UGCDetailsCallback( SteamUnifiedMessages.ServiceMethodResponse<CCloud_GetFileDetails_Response> response )
            {
                Result = response.Result;

                var msg = response.Body.details;

                AppID = msg.appid;
                UGCId = msg.ugcid;
                Creator = msg.steamid_creator;

                URL = msg.url;

                FileName = msg.filename;
                FileSize = msg.file_size;
                Timestamp = DateUtils.DateTimeFromUnixTime( msg.timestamp );
                FileSHA = msg.file_sha;
                CompressedFileSize = msg.compressed_file_size;

                RangecheckHost = response.Body.rangecheck_host;
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="GetSingleFileInfo"/>.
        /// </summary>
        public sealed class SingleFileInfoCallback
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

            internal SingleFileInfoCallback( SteamUnifiedMessages.ServiceMethodResponse<CCloud_GetSingleFileInfo_Response> response )
            {
                var msg = response.Body;

                Result = response.Result;

                AppID = msg.app_id;
                FileName = msg.file_name;
                SHAHash = msg.sha_file;
                Timestamp = DateUtils.DateTimeFromUnixTime( msg.time_stamp );
                FileSize = msg.raw_file_size;
                IsExplicitDelete = msg.is_explicit_delete;
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="ShareFile"/>.
        /// </summary>
        public sealed class ShareFileCallback
        {
            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the resulting UGC handle.
            /// </summary>
            public ulong UGCId { get; private set; }

            internal ShareFileCallback( SteamUnifiedMessages.ServiceMethodResponse<CCloud_ShareFile_Response> response )
            {
                var msg = response.Body;

                Result = response.Result;

                UGCId = msg.hcontent;
            }
        }
    }
}
