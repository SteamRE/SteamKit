using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ProtoBuf;
using ProtoBuf.Meta;
using SteamKit2;
using SteamKit2.Internal;

namespace NetHookAnalyzer2.Specializations
{
	class GCGenericSpecialization : ISpecialization
	{
		public IGameCoordinatorSpecialization[] GameCoordinatorSpecializations { get; set; }

		public IEnumerable<KeyValuePair<string, object>> ReadExtraObjects(object messageObject)
		{
			var gameCoordinatorMessage = messageObject as CMsgGCClient;
			if (gameCoordinatorMessage == null)
			{
				yield break;
			}

            using var ms = new MemoryStream( gameCoordinatorMessage.payload );
            var header = ReadHeader( gameCoordinatorMessage.msgtype, ms );
            var gcBody = ReadMessageBody( gameCoordinatorMessage.msgtype, ms, gameCoordinatorMessage.appid );
            if ( gcBody == null )
            {
                yield break;
            }

            var gc = new
            {
                emsg = EMsgExtensions.GetGCMessageName( gameCoordinatorMessage.msgtype, gameCoordinatorMessage.appid ),
                header = header,
                body = gcBody,
            };

            var specializations = new List<TreeNode>();

            yield return new KeyValuePair<string, object>( "Game Coordinator Message", gc );

            if ( GameCoordinatorSpecializations != null )
            {
                foreach ( var gameSpecificSpecialization in GameCoordinatorSpecializations )
                {
                    foreach ( var specializedObject in gameSpecificSpecialization.GetExtraObjects( gcBody, gameCoordinatorMessage.appid ) )
                    {
                        yield return specializedObject;
                    }
                }
            }
        }

		static IGCSerializableHeader ReadHeader(uint rawEMsg, Stream stream)
		{
			IGCSerializableHeader header;

			if (MsgUtil.IsProtoBuf(rawEMsg))
			{
				header = new MsgGCHdrProtoBuf();
			}
			else
			{
				header = new MsgGCHdr();
			}

			header.Deserialize(stream);
			return header;
		}

		static object ReadMessageBody(uint rawEMsg, Stream stream, uint gcAppId)
		{
			foreach (var type in MessageTypeFinder.GetGCMessageBodyTypeCandidates(rawEMsg, gcAppId))
			{
				var streamPos = stream.Position;
				try
				{
					return RuntimeTypeModel.Default.Deserialize(stream, null, type);
				}
				catch (Exception)
				{
					stream.Position = streamPos;
				}
			}

			// Last resort for protobufs
			if (MsgUtil.IsProtoBuf(rawEMsg))
			{
				try
				{
					var asFieldDictionary = ProtoBufFieldReader.ReadProtobuf(stream);
					return asFieldDictionary;
				}
				catch (ProtoException)
				{
					return "Invalid protobuf data.";
				}
				catch (EndOfStreamException ex)
				{
					return "Error parsing SO data: " + ex.Message;
				}
			}

			return null;
		}
	}
}
