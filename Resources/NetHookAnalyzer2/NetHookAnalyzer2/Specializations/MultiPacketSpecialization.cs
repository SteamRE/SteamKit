using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using SteamKit2;
using SteamKit2.Internal;

namespace NetHookAnalyzer2.Specializations
{
	class MultiPacketSpecialization : ISpecialization
	{
		public IEnumerable<KeyValuePair<string, object>> ReadExtraObjects( object messageObject )
		{
			var msgMulti = messageObject as CMsgMulti;
			if ( msgMulti == null )
			{
				yield break;
			}

			using var payloadStream = new MemoryStream( msgMulti.message_body );
			Stream stream = payloadStream;

			if ( msgMulti.size_unzipped > 0 )
			{
				stream = new GZipStream( payloadStream, CompressionMode.Decompress );
			}

			using ( stream )
			{
				var length = new byte[ sizeof( int ) ];

				while ( stream.ReadAtLeast( length, minimumBytes: length.Length, throwOnEndOfStream: false ) > 0 )
				{
					var subSize = BitConverter.ToInt32( length );
					var subData = new byte[ subSize ];

					stream.ReadAtLeast( subData, minimumBytes: subData.Length, throwOnEndOfStream: false );

					using var subDataStream = new MemoryStream( subData );
					var innerMessage = NetHookItem.ReadFile( subDataStream );

					var innerObject = new
					{
						innerMessage.Header,
						innerMessage.Body,
						innerMessage.Payload,
					};

					yield return new KeyValuePair<string, object>( NetHookItemTreeBuilder.EMsgToStringName( innerMessage.EMsg ), innerObject );
				}
			}
		}
	}
}
