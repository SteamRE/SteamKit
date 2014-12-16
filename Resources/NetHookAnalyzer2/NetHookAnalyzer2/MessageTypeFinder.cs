using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProtoBuf;
using SteamKit2;
using SteamKit2.Internal;

namespace NetHookAnalyzer2
{
	class MessageTypeFinder
	{
		public static Type GetProtobufMessageBodyType(uint realEMsg)
		{
			EMsg eMsg = MsgUtil.GetMsg(realEMsg);

			if (MessageTypeOverrides.BodyMap.ContainsKey(eMsg))
			{
				return MessageTypeOverrides.BodyMap[eMsg];
			}

			var protomsgType = SteamKit2Assembly.GetTypes().ToList().Find(t => FilterProtobufMessageBodyType(t, eMsg));
			return protomsgType;
		}

		public static Type GetSteamKitType(string name)
		{
			return SteamKit2Assembly.GetTypes().ToList().Find(type => type.FullName == name);
		}

		public static IEnumerable<Type> GetGCMessageBodyTypeCandidates(uint rawEMsg, uint gcAppid = 0)
		{
			var gcMsg = MsgUtil.GetGCMsg(rawEMsg);

			if (MessageTypeOverrides.GCBodyMap.ContainsKey(gcMsg))
			{
				return Enumerable.Repeat(MessageTypeOverrides.GCBodyMap[gcMsg], 1);
			}

			var gcMsgName = EMsgExtensions.GetGCMessageName(rawEMsg);

			var typeMsgName = gcMsgName
				.Replace("k_", string.Empty)
				.Replace("ESOMsg", string.Empty)
				.TrimStart('_')
				.Replace("EMsg", string.Empty)
				.TrimStart("GC");

			var possibleTypes = from type in typeof(CMClient).Assembly.GetTypes()
								from typePrefix in GetPossibleGCTypePrefixes(gcAppid)
								where type.GetInterfaces().Contains(typeof(IExtensible))
								where type.FullName.StartsWith(typePrefix) && type.FullName.EndsWith(typeMsgName)
								select type;

			return possibleTypes;
		}

		public static Type GetNonProtobufMessageBodyType(EMsg eMsg)
		{
			// lets first find the type by checking all EMsgs we have
			var msgType = SteamKit2Assembly.GetTypes().ToList().Find(type =>
			{
				if (type.GetInterfaces().ToList().Find(@interface => @interface == typeof(ISteamSerializableMessage)) == null)
					return false;

				var gcMsg = Activator.CreateInstance(type) as ISteamSerializableMessage;

				return gcMsg.GetEMsg() == eMsg;
			});

			return msgType;
		}

		#region How to find SteamKit2

		static Type SteamKit2MarkerType
		{
			get { return typeof(CMClient); }
		}

		static Assembly SteamKit2Assembly
		{
			get { return SteamKit2MarkerType.Assembly; }
		}

		#endregion

		#region Filters

		static bool FilterProtobufMessageBodyType(Type type, EMsg eMsg)
		{
			if (type.GetInterfaces().ToList().Find(inter => inter == typeof(IExtensible)) == null)
			{
				return false;
			}

			if (type.Namespace != "SteamKit2.Internal")
			{
				return false;
			}

			if (!type.Name.EndsWith(eMsg.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return true;
		}

		static bool FilterNonProtobufMessageBodyTypes(Type type, EMsg eMsg)
		{
			if (type.GetInterfaces().ToList().Find(@interface => @interface == typeof(ISteamSerializableMessage)) == null)
				return false;

			var gcMsg = Activator.CreateInstance(type) as ISteamSerializableMessage;

			return gcMsg.GetEMsg() == eMsg;
		}

		#endregion

		static IEnumerable<string> GetPossibleGCTypePrefixes(uint appid)
		{
			yield return "SteamKit2.GC.Internal.CMsg";

			switch (appid)
			{
				case 440:
					yield return "SteamKit2.GC.TF.Internal.CMsg";
					break;

				case 570:
					yield return "SteamKit2.GC.Dota.Internal.CMsg";
					break;

				case 730:
					yield return "SteamKit2.GC.CSGO.Internal.CMsg";
					break;
			}
		}
	}
}
