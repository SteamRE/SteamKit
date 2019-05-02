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
            Rescan,
            Invalid
        }

        public ProtobufCollector()
        {
            Candidates = new List<FileDescriptorProto>();
        }

        public bool TryParseCandidate( string name, Stream data, out CandidateResult result, out Exception error )
        {
            FileDescriptorProto candidate;

            try
            {
                candidate = Serializer.Deserialize<FileDescriptorProto>( data );
            }
            catch ( EndOfStreamException ex )
            {
                result = CandidateResult.Rescan;
                error = ex;
                return false;
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
    }
}
