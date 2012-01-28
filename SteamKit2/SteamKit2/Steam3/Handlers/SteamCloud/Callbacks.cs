/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
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


#if STATIC_CALLBACKS
            internal UGCDetailsCallback( SteamClient client, CMsgClientUFSGetUGCDetailsResponse msg )
                : base( client )
#else
            internal UGCDetailsCallback( CMsgClientUFSGetUGCDetailsResponse msg )
#endif
            {
                Result = ( EResult )msg.eresult;

                AppID = msg.app_id;
                Creator = msg.steamid_creator;

                URL = msg.url;

                FileName = msg.filename;
                FileSize = msg.file_size;
            }
        }
    }
}
