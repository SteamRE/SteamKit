using System;

namespace SteamKit2.Blob
{
    public static class AuthFields
    {
        public const string eFieldIdent = "0"; // byte[2] ident 04 00
        public const string eFieldAccount = "1"; // string(x) account name
        public const string eFieldTimestampCreation = "2"; // byte[8] account creation timestamp
        public const string eFieldRedHerring = "3"; // string(9) "Egq-pe-y"

        public const string eFieldAccountBlob = "6"; // Blob, containing 1 descriptor with account name

        public const string eFieldUnknownBlob7 = "7"; // Blob, 1/2 fields, containing blobs (5 children each)
        public const string eFieldUnknownBlob8 = "8"; // Blob, empty!

        public const string eFieldTimestamp9 = "9"; // byte[8] timestamp 2

        public const string eFieldUnknown10 = "10"; // byte[4] 4 nulls

        public const string eFieldEmail = "11"; // string(x) email address

        public const string eFieldUnknown12 = "12"; // byte[2] 0/1 (chrislimited: 0, yaka: 1)

        public const string eFieldTimestamp14 = "14"; // byte[8] timestamp 3

        public const string eFieldUnknownBlob15 = "15"; // Blob, 1/2 blob fields. First field, two children, one is empty. second field contained WON key
    }

    // flags for blob inside eFieldAccountBlob 6 inside account field
    public static class AuthAccountBlobFields
    {
        public const string eFieldUnknown1 = "1"; // byte[8] timestamp?
        public const string eFieldUnknown2 = "2"; // byte[2] always 01 00
        public const string eFieldUnknownBlob3 = "3"; // Blob, empty
    }

    // flags for descriptor 7
    public static class AuthDescriptor7Fields
    {
        public const string eFieldBlob0 = "0"; // Blob, first field
        public const string eFieldBlob1 = "1"; // Blob, second (appears ??)
    }
    // flags for both desc 7 inner blobs
    public static class AuthDescriptor7InnerFields
    {
        public const string eFieldUnknown1 = "1"; // byte[8] timestamp?
        public const string eFieldUnknown2 = "2"; // byte[8] only set in second
        public const string eFieldUnknown3 = "3"; // byte[2] 01 00 in first, 10 00 in second
        public const string eFieldUnknown5 = "5"; // byte[1] always 0
        public const string eFieldUnknown6 = "6"; // byte[2] 1F 00 in first, 00 00 in second
    }

    // first blob is constant (desc 0: empty blob, desc 1: 07) second blob is WON
    public static class AuthDescriptor15Fields
    {
        public const string eFieldBlob0 = "0"; // Blob, constant
        public const string eFieldBlob1 = "1"; // Blob, contains WON key
    }
    // fields for inner 15 blobs
    public static class AuthDescriptor15InnerFields
    {
        public const string eFieldType = "1"; // byte[1] 07 (first blob) 06 (won)
        public const string eFieldBlobData = "2"; // Blob, data for type (07 is empty, 06 contains key data blob)
    }
    // WON data blob
    public static class AuthKeyDataFields
    {
        public const string eFieldKeyName = "1"; // string(x) "WONCDKey"
        public const string eFieldKeyValue = "2"; // string(x) a key
    }
}
