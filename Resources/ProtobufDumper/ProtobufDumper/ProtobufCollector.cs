using System;
using System.Collections.Generic;
using System.IO;
using google.protobuf;
using ProtoBuf;

namespace ProtobufDumper
{
    class ProtobufCollector
    {
        public List<FileDescriptorProto> Candidates { get; private set; }

        public ProtobufCollector()
        {
            Candidates = new List<FileDescriptorProto>();
        }


        public bool CollectCandidate( string name, byte[] data )
        {
            FileDescriptorProto candidate;

            Console.Write( "{0}... ", name );

            try
            {
                using ( var ms = new MemoryStream( data ) )
                    candidate = Serializer.Deserialize<FileDescriptorProto>( ms );
            }
            catch ( EndOfStreamException ex )
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine( "needs rescan: {0}", ex.Message );
                Console.ResetColor();
                return false;
            }
            catch ( Exception ex )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( "is invalid: {0}", ex.Message );
                Console.ResetColor();
                return true;
            }

            Candidates.Add( candidate );

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine( "OK!" );
            Console.ResetColor();

            return true;
        }

    }
}
