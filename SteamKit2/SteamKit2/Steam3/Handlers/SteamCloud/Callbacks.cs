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
        public sealed class UGCDetailsCallback : CallbackMsg
        {
            public EResult Result { get; private set; }

            public uint AppID { get; private set; }
            public SteamID Creator { get; private set; }

            public string URL { get; private set; }

            public string FileName { get; private set; }
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
