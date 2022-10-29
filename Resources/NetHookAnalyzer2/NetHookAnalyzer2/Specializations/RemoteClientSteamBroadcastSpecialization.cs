using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf.Meta;
using SteamKit2.Internal;

namespace NetHookAnalyzer2.Specializations
{
	class RemoteClientSteamBroadcastSpecialization : ISpecialization
	{
		public IEnumerable<KeyValuePair<string, object>> ReadExtraObjects( object messageObject )
		{
			var broadcast = messageObject as CRemoteClient_SteamBroadcast_Notification;
			if ( broadcast == null )
			{
				yield break;
			}

			using var broadcastStream = new MemoryStream( broadcast.payload );
			using var br = new BinaryReader( broadcastStream );

			if ( br.ReadUInt32() != 0xFFFFFFFF )
			{
				yield break;
			}

			if ( br.ReadUInt32() != 0xA05F4C21 )
			{
				yield break;
			}

			var headerLength = br.ReadInt32();
			using var msProto = new MemoryStream( br.ReadBytes( headerLength ) );
			var header = ProtoBuf.Serializer.Deserialize<CMsgRemoteClientBroadcastHeader>( msProto );

			yield return new KeyValuePair<string, object>( "Broadcast Header", header );

			var bodyLength = br.ReadInt32();
			var body = br.ReadBytes( bodyLength );

			var typeName = header.msg_type
				.ToString()
				.Replace( "k_E", "CMsg" )
				.Replace( "BroadcastMsg", "Broadcast" );

			var bodyType = Type.GetType( $"SteamKit2.Internal.{typeName}, SteamKit2" );

			if ( bodyType == null )
			{
				yield return new KeyValuePair<string, object>( "Broadcast Body", body );
				yield break;
			}

			using var msBody = new MemoryStream( body );
			var bodyProto = RuntimeTypeModel.Default.Deserialize( msBody, null, bodyType );

			yield return new KeyValuePair<string, object>( "Broadcast Body", bodyProto );
		}
	}
}
