using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using SteamKit2;
using SteamKit2.Internal;

namespace NetHookAnalyzer2.Specializations
{
	class MMSLobbyMessageSpecialization : ISpecialization
	{
		public IEnumerable<KeyValuePair<string, object>> ReadExtraObjects( object messageObject )
		{
			byte[] data = null;

			if ( messageObject is CMsgClientMMSSetLobbyData setLobbyData )
			{
				data = setLobbyData.metadata;
			}
			else if ( messageObject is CMsgClientMMSLobbyData lobbyData )
			{
				data = lobbyData.metadata;
			}
			else if ( messageObject is CMsgClientMMSJoinLobbyResponse joinLobbyResponse )
			{
				data = joinLobbyResponse.metadata;
			}
			else if ( messageObject is CMsgClientMMSCreateLobby createLobby )
			{
				data = createLobby.metadata;
			}
			else if ( messageObject is CMsgClientMMSSendLobbyChatMsg or CMsgClientMMSLobbyChatMsg )
			{
				uint appid = 0;

				if ( messageObject is CMsgClientMMSSendLobbyChatMsg sendLobbyChatMsg )
				{
					appid = sendLobbyChatMsg.app_id;
					data = sendLobbyChatMsg.lobby_message;
				}
				else if ( messageObject is CMsgClientMMSLobbyChatMsg lobbyChatMsg )
				{
					appid = lobbyChatMsg.app_id;
					data = lobbyChatMsg.lobby_message;
				}

				// they prepend keyvalues with engine build version and then keyvalues follow
				if ( appid == WellKnownAppIDs.CounterStrike2 || appid == WellKnownAppIDs.Dota2 )
				{
					data = data[ 4.. ]; // remove the version
				}
				else
				{
					data = null; // We likely don't know how to handle this game data
				}
			}
			// TODO: CMsgClientMMSGetLobbyListResponse

			if ( data != null )
			{
				var kv = SetBinaryKeyValues( data );

				if ( kv.HasValue )
				{
					yield return kv.Value;
				}
			}

			yield break;
		}

		private static KeyValuePair<string, object>? SetBinaryKeyValues( byte[] data )
		{
			var kv = new KeyValue();
			using var ms = new MemoryStream( data );

			// Special VBKV header, appears in Source 2 LobbyChatMsg
			if ( BinaryPrimitives.ReadUInt32LittleEndian( data ) == 0x564B4256 )
			{
				ms.Position += 4; // skip the magic
				ms.Position += 4; // skip the crc32
			}

			if ( kv.TryReadAsBinary( ms ) )
			{
				return new KeyValuePair<string, object>( "Lobby Metadata", kv );
			}

			return null;
		}
	}
}
