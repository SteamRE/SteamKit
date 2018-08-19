using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NetHookAnalyzer2.Specializations;
using ProtoBuf;
using ProtoBuf.Meta;
using SteamKit2;
using SteamKit2.Internal;

namespace NetHookAnalyzer2
{
	partial class NetHookItemTreeBuilder
	{
		static ISteamSerializableHeader ReadHeader(uint rawEMsg, Stream stream)
		{
			ISteamSerializableHeader header;

			if (MsgUtil.IsProtoBuf(rawEMsg))
			{
				header = new MsgHdrProtoBuf();
			}
			else
			{
				switch (rawEMsg)
				{
					case (uint)EMsg.ChannelEncryptRequest:
					case (uint)EMsg.ChannelEncryptResponse:
					case (uint)EMsg.ChannelEncryptResult:
						header = new MsgHdr();
						break;

					default:
						header = new ExtendedClientMsgHdr();
						break;
				}
			}

			header.Deserialize(stream);
			return header;
		}

		static object ReadBody(uint rawEMsg, Stream stream, ISteamSerializableHeader header)
		{
			var eMsg = MsgUtil.GetMsg(rawEMsg);
			var isProto = MsgUtil.IsProtoBuf(rawEMsg);

			var targetJobName = new Lazy<string>(() => ((MsgHdrProtoBuf)header).Proto.target_job_name);

			object body;

			switch (eMsg)
			{
				case EMsg.ServiceMethod:
				case EMsg.ServiceMethodCallFromClient:
					body = UnifiedMessagingHelpers.ReadServiceMethodBody(targetJobName.Value, stream, x => x.GetParameters().First().ParameterType);
					break;

				case EMsg.ServiceMethodResponse:
					body = UnifiedMessagingHelpers.ReadServiceMethodBody(targetJobName.Value, stream, x => x.ReturnType);
					break;

				default:
					body = ReadMessageBody(rawEMsg, stream);
					break;
			}

			return body;
		}

		static byte[] ReadPayload(Stream stream)
		{
			var payloadLength = (int)(stream.Length - stream.Position);

			var payloadData = new byte[payloadLength];
			stream.Read(payloadData, 0, payloadData.Length);

			return payloadData;
		}

		static uint PeekUInt(Stream stream)
		{
			var data = new byte[sizeof(uint)];
			stream.Read(data, 0, data.Length);
			stream.Seek(-sizeof(uint), SeekOrigin.Current);
			return BitConverter.ToUInt32(data, 0);
		}

		static object ReadMessageBody(uint rawEMsg, Stream stream)
		{
			var eMsg = MsgUtil.GetMsg(rawEMsg);

			var protoMsgType = MessageTypeFinder.GetProtobufMessageBodyType(rawEMsg);
			if (protoMsgType != null)
			{
				return RuntimeTypeModel.Default.Deserialize(stream, null, protoMsgType);
			}

			// lets first find the type by checking all EMsgs we have
			var msgType = MessageTypeFinder.GetNonProtobufMessageBodyType(eMsg);

			var eMsgName = eMsg.ToString()
				.Replace("Econ", "")
				.Replace("AM", "");

			// check name
			if (msgType == null)
			{
				msgType = MessageTypeFinder.GetSteamKitType(string.Format("SteamKit2.Msg{0}", eMsgName));
			}

			if (msgType != null)
			{
				var body = Activator.CreateInstance(msgType) as ISteamSerializableMessage;
				body.Deserialize(stream);

				return body;
			}

			msgType = MessageTypeFinder.GetSteamKitType(string.Format("SteamKit2.CMsg{0}", eMsgName));
			if (msgType != null)
			{
				return Serializer.NonGeneric.Deserialize(msgType, stream);
			}

			// Last resort for protobufs
			if (MsgUtil.IsProtoBuf(rawEMsg))
			{
				var asFieldDictionary = ProtoBufFieldReader.ReadProtobuf(stream);
				return asFieldDictionary;
			}

			return null;
		}
	}
}
