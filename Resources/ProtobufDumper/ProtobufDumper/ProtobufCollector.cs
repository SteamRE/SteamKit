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

        public enum CandidateResult
        {
            OK,
            Invalid
        }

        public ProtobufCollector()
        {
            Candidates = [];
        }

        public bool TryParseCandidate( Stream data, out CandidateResult result, out Exception error, out long bytesConsumed )
        {
            FileDescriptorProto candidate;
            bytesConsumed = 0;

            try
            {
                bytesConsumed = ConsumeOneMessage( data );

                if ( bytesConsumed == 0 )
                {
                    throw new InvalidDataException( "No data was consumed" );
                }

                data.Position = 0;
                var buffer = new byte[ bytesConsumed ];
                data.Read( buffer, 0, ( int )bytesConsumed );

                candidate = Serializer.Deserialize<FileDescriptorProto>( buffer.AsSpan() );
            }
            catch ( Exception ex )
            {
                result = CandidateResult.Invalid;
                error = ex;
                return true;
            }

            Candidates.Add( candidate );

            result = CandidateResult.OK;
            error = null;
            return true;
        }

        private static long ConsumeOneMessage( Stream data )
        {
            bool consumedFieldOne = false;

            using var state = ProtoReader.State.Create( data, null, null );

            while ( true )
            {
                long positionBeforeField = state.GetPosition();

                try
                {
                    var fieldNumber = state.ReadFieldHeader();

                    if ( fieldNumber == 0 )
                    {
                        return positionBeforeField;
                    }

                    // Field 1 is the "name" field in FileDescriptorProto
                    // If we see it twice, we've hit the next message
                    if ( fieldNumber == 1 )
                    {
                        if ( consumedFieldOne )
                        {
                            return positionBeforeField;
                        }

                        consumedFieldOne = true;
                    }

                    state.SkipField();
                }
                catch
                {
                    return positionBeforeField;
                }
            }
        }
    }
}
