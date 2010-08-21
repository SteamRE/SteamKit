using System;

namespace BlobLib
{
    public class AuthFields
    {
        public enum EFieldType : int
        {
            eFieldIdent = 0, // byte[2] ident 04 00
            eFieldAccount = 1, // string(x) account name
            eFieldTimestamp2 = 2, // byte[8] timestamp 1
            eFieldRedHerring = 3, // string(9) "Egq-pe-y"

            eFieldAccountBlob = 6, // Blob, containing 1 descriptor with account name

            eFieldUnknownBlob7 = 7, // Blob, 1/2 fields, containing blobs (5 children each)
            eFieldUnknownBlob8 = 8, // Blob, empty!

            eFieldTimestamp9 = 9, // byte[8] timestamp 2

            eFieldUnknown10 = 10, // byte[4] 4 nulls

            eFieldEmail = 11, // string(x) email address

            eFieldUnknown12 = 12, // byte[2] 0/1 (chrislimited: 0, yaka: 1)

            eFieldTimestamp14 = 14, // byte[8] timestamp 3
            
            eFieldUnknownBlob15 = 15, // Blob, 1/2 blob fields. First field, two children, one is empty. second field contained WON key
        };

        // flags for blob inside eFieldAccountBlob 6 inside account field
        public enum EAcccountBlobFields : int
        {
            eFieldUnknown1 = 1, // byte[8] timestamp?
            eFieldUnknown2 = 2, // byte[2] always 01 00
            eFieldUnknownBlob3 = 3, // Blob, empty
        }

        // flags for descriptor 7
        public enum EUnknown7Fields : int
        {
            eFieldBlob0 = 0, // Blob, first field
            eFieldBlob1 = 1, // Blob, second (appears ??)
        }
        // flags for both desc 7 inner blobs
        public enum EUnknown7InnerFields : int
        {
            eFieldUnknown1 = 1, // byte[8] timestamp?
            eFieldUnknown2 = 2, // byte[8] only set in second
            eFieldUnknown3 = 3, // byte[2] 01 00 in first, 10 00 in second
            eFieldUnknown5 = 5, // byte[1] always 0
            eFieldUnknown6 = 6, // byte[2] 1F 00 in first, 00 00 in second
        }

        // first blob is constant (desc 0: empty blob, desc 1: 07) second blob is WON
        public enum EUnknown15Flags : int
        {
            eFieldBlob0 = 0, // Blob, constant
            eFieldBlob1 = 1, // Blob, contains WON key
        }
        // fields for inner 15 blobs
        public enum EUnknown15InnerFlags : int
        {
            eFieldType = 1, // byte[1] 07 (first blob) 06 (won)
            eFieldBlobData = 2, // Blob, data for type (07 is empty, 06 contains key data blob)
        }
        // WON data blob
        public enum EKeyDataBlob : int
        {
            eFieldKeyName = 1, // string(x) "WONCDKey"
            eFieldKeyValue = 2, // string(x) a key
        }

    }
}
