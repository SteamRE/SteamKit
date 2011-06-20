using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public class AuthBlob
    {
        [BlobField(FieldKey = AuthFields.eFieldAccount)]
        public string AccountName { get; set; }

        [BlobField(FieldKey = AuthFields.eFieldEmail)]
        public string Email { get; set; }

        [BlobField(FieldKey = AuthFields.eFieldTimestampCreation)]
        public MicroTime CreationTime { get; set; }
    }
}
