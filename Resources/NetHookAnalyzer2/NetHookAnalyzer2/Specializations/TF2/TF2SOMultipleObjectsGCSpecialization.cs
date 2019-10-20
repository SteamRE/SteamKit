using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
using SteamKit2.GC.TF2.Internal;

namespace NetHookAnalyzer2.Specializations
{
	class TF2SOMultipleObjectsGCSpecialization : IGameCoordinatorSpecialization
	{
		public IEnumerable<KeyValuePair<string, object>> GetExtraObjects(object body, uint appID)
		{
			if (appID != WellKnownAppIDs.TeamFortress2)
			{
				yield break;
			}

			var updateMultiple = body as CMsgSOMultipleObjects;
			if (updateMultiple == null)
			{
				yield break;
			}

			foreach(var singleObject in updateMultiple.objects)
			{
				var extraNode = ReadExtraObject(singleObject);
				if (extraNode != null)
				{
					yield return new KeyValuePair<string, object>(string.Format("SO ({0})", extraNode.GetType().Name), extraNode);
				}
			}
		}

		object ReadExtraObject(CMsgSOMultipleObjects.SingleObject sharedObject)
		{
			try
			{
                using var ms = new MemoryStream( sharedObject.object_data );
                if ( TF2SOHelper.SOTypes.TryGetValue( sharedObject.type_id, out var t ) )
                {
                    return RuntimeTypeModel.Default.Deserialize( ms, null, t );
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
