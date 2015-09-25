using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
using SteamKit2.GC.Internal;

namespace NetHookAnalyzer2.Specializations
{
	class Dota2SOMultipleObjectsGCSpecialization : IGameCoordinatorSpecialization
	{
		const uint Dota2AppID = 570;

		public IEnumerable<KeyValuePair<string, object>> GetExtraObjects(object body, uint appID)
		{
			if (appID != Dota2AppID)
			{
				yield break;
			}

			var updateMultiple = body as CMsgSOMultipleObjects;
			if (updateMultiple == null)
			{
				yield break;
			}

			foreach(var singleObject in updateMultiple.objects_added)
			{
				var extraNode = ReadExtraObject(singleObject);
				if (extraNode != null)
				{
					yield return new KeyValuePair<string, object>("New SO", extraNode);
				}
			}

			foreach (var singleObject in updateMultiple.objects_modified)
			{
				var extraNode = ReadExtraObject(singleObject);
				if (extraNode != null)
				{
					yield return new KeyValuePair<string, object>("Modified SO", extraNode);
				}
			}

			foreach (var singleObject in updateMultiple.objects_removed)
			{
				var extraNode = ReadExtraObject(singleObject);
				if (extraNode != null)
				{
					yield return new KeyValuePair<string, object>("Removed SO", extraNode);
				}
			}
		}

		object ReadExtraObject(CMsgSOMultipleObjects.SingleObject sharedObject)
		{
			try
			{
				using (var ms = new MemoryStream(sharedObject.object_data))
				{
					Type t;
					if (Dota2SOHelper.SOTypes.TryGetValue(sharedObject.type_id, out t))
					{
						return RuntimeTypeModel.Default.Deserialize(ms, null, t);
					}
				}
			}
			catch (ProtoException ex)
			{
				return "Error parsing SO data: " + ex.Message;
			}
			catch (EndOfStreamException ex)
			{
				return "Error parsing SO data: " + ex.Message;
			}

			return null;
		}
	}
}
