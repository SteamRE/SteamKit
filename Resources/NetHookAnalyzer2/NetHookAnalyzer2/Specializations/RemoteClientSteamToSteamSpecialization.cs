using System;
using System.Collections.Generic;
using System.IO;
using SteamKit2;
using SteamKit2.Internal;

namespace NetHookAnalyzer2.Specializations
{
	class RemoteClientSteamToSteamSpecialization : ISpecialization
	{
		public IEnumerable<KeyValuePair<string, object>> ReadExtraObjects( object messageObject )
		{
			var notification = messageObject as CRemoteClient_SteamToSteam_Notification;
			if ( notification == null )
			{
				yield break;
			}

			var authSecret = NetHookDump.GetAccountAuthSecret( ( int )notification.secretid );

			if ( authSecret == null )
			{
				yield break;
			}

			var decrypted = CryptoHelper.SymmetricDecrypt( notification.encrypted_payload, authSecret );
			using var decryptedStream = new MemoryStream( decrypted );
			var innerMessage = NetHookItem.ReadFile( decryptedStream );

			yield return new KeyValuePair<string, object>( "Decrypted Header", innerMessage.Header );
			yield return new KeyValuePair<string, object>( "Decrypted Body", innerMessage.Body );

			if ( innerMessage.Payload != null && innerMessage.Payload.Length > 0 )
			{
				yield return new KeyValuePair<string, object>( "Decrypted Payload", innerMessage.Payload );
			}
		}
	}
}
