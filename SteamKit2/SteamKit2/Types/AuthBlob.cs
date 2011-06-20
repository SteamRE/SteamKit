using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public class AuthBlob
    {
        [BlobField(FieldKey = AuthFields.eFieldAccount, Depth = 1)]
        public string AccountName { get; set; }

        [BlobField(FieldKey = AuthFields.eFieldEmail, Depth = 1)]
        public string Email { get; set; }

        [BlobField(FieldKey = AuthFields.eFieldTimestampCreation, Depth = 1)]
        public MicroTime CreationTime { get; set; }
    }
}
