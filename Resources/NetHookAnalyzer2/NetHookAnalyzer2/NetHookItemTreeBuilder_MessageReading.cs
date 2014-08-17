using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
				header = new ExtendedClientMsgHdr();
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

			// Unified Notifications
			if (isProto && eMsg == EMsg.ServiceMethod && !string.IsNullOrEmpty(targetJobName.Value))
			{
				body = ReadServiceMethodBody(targetJobName.Value, stream, x => x.GetParameters().First().ParameterType);
			}
			else
			{
				body = ReadMessageBody(rawEMsg, stream);
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

		static object ReadServiceMethodBody(string methodName, Stream stream, Func<MethodInfo, Type> typeSelector)
		{
			var methodInfo = FindMethodInfo(methodName);
			if (methodInfo != null)
			{
				var requestType = typeSelector(methodInfo);
				var request = RuntimeTypeModel.Default.Deserialize(stream, null, requestType);
				return request;
			}

			return null;
		}

		static MethodInfo FindMethodInfo(string serviceMethodName)
		{
			var splitByDot = serviceMethodName.Split('.');
			var interfaceName = "I" + splitByDot[0];
			var methodName = splitByDot[1].Split('#').First();

			var namespaces = new[]
			{
				"SteamKit2.Unified.Internal"
			};

			foreach (var ns in namespaces)
			{
				var interfaceType = Type.GetType(ns + "." + interfaceName + ", SteamKit2");
				if (interfaceType != null)
				{
					var method = interfaceType.GetMethod(methodName);
					if (method != null)
					{
						return method;
					}
				}
			}

			return null;
		}

		static uint PeekUInt(Stream stream)
		{
			var data = new byte[sizeof(uint)];
			stream.Read(data, 0, data.Length);
			stream.Seek(-sizeof(uint), SeekOrigin.Current);
			return BitConverter.ToUInt32(data, 0);
		}

		static object ReadMessageBody(uint rawEMsg, Stream stream, uint gcAppId = 0)
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

			if (gcAppId != 0)
			{
				foreach (var type in MessageTypeFinder.GetGCMessageBodyTypeCandidates(rawEMsg, gcAppId))
				{
					var streamPos = stream.Position;
					try
					{
						return Serializer.NonGeneric.Deserialize(type, stream);
					}
					catch (Exception)
					{
						stream.Position = streamPos;
					}
				}
			}

			// Last resort for protobufs
			if (MsgUtil.IsProtoBuf(rawEMsg))
			{
				var asFieldDictionary = ProtoBufFieldReader.ReadProtobuf(stream);
				return asFieldDictionary;
			}

			return null;
		}

		static IGCSerializableHeader ReadGameCoordinatorHeader(uint rawEMsg, Stream stream)
		{
			IGCSerializableHeader header = null;

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
	}
}
