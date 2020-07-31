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
            /// <summary>
            /// If true, the disconnection was initiated by calling <see cref="UFSClient.Disconnect"/>.
            /// If false, the disconnection was the cause of something not user-controlled, such as a network failure or
            /// a forcible disconnection by the remote server.
            /// </summary>
            public bool UserInitiated { get; private set; }

            internal DisconnectedCallback( bool userInitiated )
            {
                this.UserInitiated = userInitiated;
            }
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


            internal LoggedOnCallback( JobID jobID, CMsgClientUFSLoginResponse body )
            {
                JobID = jobID;

                Result = ( EResult )body.eresult;
            }


            internal LoggedOnCallback( JobID jobID, EResult result )
            {
                JobID = jobID;

                Result = result;
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
            /// Gets the JobID on the UFS server.
            /// </summary>
            public JobID RemoteJobID { get; private set; }


            internal UploadFileResponseCallback( JobID jobID, CMsgClientUFSUploadFileResponse body, JobID remoteJobID )
            {
                JobID = jobID;

                Result = ( EResult )body.eresult;
                UseHttp = body.use_http;
                UseHttps = body.use_https;
                EncryptFile = body.encrypt_file;
                ShaHash = body.sha_file;

                RemoteJobID = remoteJobID;
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

            internal UploadFileFinishedCallback( JobID jobID, CMsgClientUFSUploadFileFinished body )
            {
                JobID = jobID;

                Result = (EResult)body.eresult;
                ShaHash = body.sha_file;
            }
        }
    }
}
