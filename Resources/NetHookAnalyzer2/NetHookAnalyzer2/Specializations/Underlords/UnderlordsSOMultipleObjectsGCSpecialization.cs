using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
using SteamKit2.GC.Underlords.Internal;

namespace NetHookAnalyzer2.Specializations
{
	class UnderlordsSOMultipleObjectsGCSpecialization : IGameCoordinatorSpecialization
	{
		public IEnumerable<KeyValuePair<string, object>> GetExtraObjects(object body, uint appID)
		{
			if (appID != WellKnownAppIDs.Underlords)
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
					yield return new KeyValuePair<string, object>(string.Format("New SO ({0})", extraNode.GetType().Name), extraNode);
				}
			}

			foreach (var singleObject in updateMultiple.objects_modified)
			{
				var extraNode = ReadExtraObject(singleObject);
				if (extraNode != null)
				{
					yield return new KeyValuePair<string, object>(string.Format("Modified SO ({0})", extraNode.GetType().Name), extraNode);
				}
			}

			foreach (var singleObject in updateMultiple.objects_removed)
			{
				var extraNode = ReadExtraObject(singleObject);
				if (extraNode != null)
				{
					yield return new KeyValuePair<string, object>(string.Format("Removed SO ({0})", extraNode.GetType().Name), extraNode);
				}
			}
		}

		object ReadExtraObject(CMsgSOMultipleObjects.SingleObject sharedObject)
		{
			try
			{
				using var ms = new MemoryStream(sharedObject.object_data);
				if (UnderlordsSOHelper.SOTypes.TryGetValue(sharedObject.type_id, out var t))
				{
					return RuntimeTypeModel.Default.Deserialize(ms, null, t);
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
