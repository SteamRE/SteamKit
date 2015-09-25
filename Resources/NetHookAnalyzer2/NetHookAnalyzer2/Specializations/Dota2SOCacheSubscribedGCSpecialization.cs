using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.GC.Internal;

namespace NetHookAnalyzer2.Specializations
{
    class Dota2CacheSubscribedGCSpecialization : IGameCoordinatorSpecialization
    {
        const uint Dota2AppID = 570;

        public IEnumerable<KeyValuePair<string, object>> GetExtraObjects(object body, uint appID)
        {
            if (appID != Dota2AppID)
            {
                yield break;
            }

            var cacheSubscribed = body as CMsgSOCacheSubscribed;
            if (cacheSubscribed == null)
            {
                yield break;
            }

            foreach (var bucket in cacheSubscribed.objects)
            {
                int typeId = bucket.type_id;
                foreach (var singleObject in bucket.object_data)
                {
                    var extraNode = ReadExtraObject(singleObject, typeId);
                    if (extraNode != null)
                    {
                        yield return new KeyValuePair<string, object>("SO", extraNode);
                    }
                }
            }
        }

        object ReadExtraObject(byte[] sharedObject, int typeId)
        {
            try
            {
                using (var ms = new MemoryStream(sharedObject))
                {
                    Type t;
                    if (Dota2SOHelper.SOTypes.TryGetValue(typeId, out t))
                    {
                        return RuntimeTypeModel.Default.Deserialize(ms, null, t);
                    }
                }
            }
            catch (ProtoException ex)
            {
                return "Error parsing SO data: " + ex.Message;
            }

            return null;
        }
    }
}
