/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;

namespace SteamKit2
{
    public static class AuthFields
    {
        public const int eFieldIdent = 0; // byte[2] ident 04 00
        public const int eFieldAccount = 1; // string(x) account name
        public const int eFieldTimestampCreation = 2; // byte[8] account creation timestamp
        public const int eFieldRedHerring = 3; // string(9) "Egq-pe-y"

        public const int eFieldAccountBlob = 6; // Blob, containing 1 descriptor with account name

        public const int eFieldUnknownBlob7 = 7; // Blob, 1/2 fields, containing blobs (5 children each)
        public const int eFieldUnknownBlob8 = 8; // Blob, empty!

        public const int eFieldTimestamp9 = 9; // byte[8] timestamp 2

        public const int eFieldUnknown10 = 10; // byte[4] 4 nulls

        public const int eFieldEmail = 11; // string(x) email address

        public const int eFieldUnknown12 = 12; // byte[2] 0/1 (chrislimited: 0, yaka: 1)

        public const int eFieldTimestamp14 = 14; // byte[8] timestamp 3

        public const int eFieldUnknownBlob15 = 15; // Blob, 1/2 blob fields. First field, two children, one is empty. second field contained WON key
    }

    // flags for blob inside eFieldAccountBlob 6 inside account field
    public static class AuthAccountBlobFields
    {
        public const int eFieldUnknown1 = 1; // byte[8] timestamp?
        public const int eFieldUnknown2 = 2; // byte[2] always 01 00
        public const int eFieldUnknownBlob3 = 3; // Blob, empty
    }

    // flags for descriptor 7
    public static class AuthDescriptor7Fields
    {
        public const int eFieldBlob0 = 0; // Blob, first field
        public const int eFieldBlob1 = 1; // Blob, second (appears ??)
    }
    // flags for both desc 7 inner blobs
    public static class AuthDescriptor7InnerFields
    {
        public const int eFieldUnknown1 = 1; // byte[8] timestamp?
        public const int eFieldUnknown2 = 2; // byte[8] only set in second
        public const int eFieldUnknown3 = 3; // byte[2] 01 00 in first, 10 00 in second
        public const int eFieldUnknown5 = 5; // byte[1] always 0
        public const int eFieldUnknown6 = 6; // byte[2] 1F 00 in first, 00 00 in second
    }

    // first blob is constant (desc 0: empty blob, desc 1: 07) second blob is WON
    public static class AuthDescriptor15Fields
    {
        public const int eFieldBlob0 = 0; // Blob, constant
        public const int eFieldBlob1 = 1; // Blob, contains WON key
    }
    // fields for inner 15 blobs
    public static class AuthDescriptor15InnerFields
    {
        public const int eFieldType = 1; // byte[1] 07 (first blob) 06 (won)
        public const int eFieldBlobData = 2; // Blob, data for type (07 is empty, 06 contains key data blob)
    }
    // WON data blob
    public static class AuthKeyDataFields
    {
        public const int eFieldKeyName = 1; // string(x) "WONCDKey"
        public const int eFieldKeyValue = 2; // string(x) a key
    }
}
