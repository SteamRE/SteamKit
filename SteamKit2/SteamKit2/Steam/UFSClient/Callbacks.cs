/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2.Internal;

namespace SteamKit2
{
    public partial class UFSClient
    {
        /// <summary>
        /// This callback is received after attempting to connect to the UFS server.
        /// </summary>
        public sealed class ConnectedCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the connection attempt.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }


            internal ConnectedCallback( MsgChannelEncryptResult result )
                : this( result.Result )
            {
            }

            internal ConnectedCallback( EResult result )
            {
                this.Result = result;
            }
        }

        /// <summary>
        /// This callback is received when the client is physically disconnected from the UFS server.
        /// </summary>
        public sealed class DisconnectedCallback : CallbackMsg
        {
        }

        /// <summary>
        /// This callback is returned in response to an attempt to log on to the UFS server through <see cref="UFSClient"/>.
        /// </summary>
        public sealed class LoggedOnCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the logon
            /// </summary>
            public EResult Result { get; private set; }


            internal LoggedOnCallback( CMsgClientUFSLoginResponse body )
            {
                Result = ( EResult )body.eresult;
            }
        }

        /// <summary>
        /// This callback is returned in response to a request to upload a file through <see cref="UFSClient"/>.
        /// </summary>
        public sealed class UploadFileResponseCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the upload request
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets whether or not the file upload should proceed over HTTP
            /// </summary>
            public bool UseHttp { get; private set; }

            /// <summary>
            /// Gets whether or not the file upload should proceed over HTTPS
            /// </summary>
            public bool UseHttps { get; private set; }

            /// <summary>
            /// Gets whether or not the file should be encrypted during upload
            /// </summary>
            public bool EncryptFile { get; private set; }

            /// <summary>
            /// Gets the SHA hash of the file to be uploaded
            /// </summary>
            public byte[] ShaHash { get; private set; }

            /// <summary>
            /// Gets the JobID for this upload session. This is used for <see cref="UploadDetails.JobID"/>.
            /// </summary>
            public JobID JobID { get; private set; }


            internal UploadFileResponseCallback( CMsgClientUFSUploadFileResponse body, JobID remoteJobID )
            {
                Result = ( EResult )body.eresult;
                UseHttp = body.use_http;
                UseHttps = body.use_https;
                EncryptFile = body.encrypt_file;
                ShaHash = body.sha_file;

                JobID = remoteJobID;
            }
        }

        /// <summary>
        /// This callback is returned when a file upload through <see cref="UFSClient"/> is completed.
        /// </summary>
        public sealed class UploadFileFinishedCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the file upload
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the SHA hash of the file that was uploaded
            /// </summary>
            public byte[] ShaHash { get; private set; }

            internal UploadFileFinishedCallback( CMsgClientUFSUploadFileFinished body )
            {
                Result = (EResult)body.eresult;
                ShaHash = body.sha_file;
            }
        }
    }
}
