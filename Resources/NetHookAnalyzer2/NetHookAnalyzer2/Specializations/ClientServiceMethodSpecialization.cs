using System.Collections.Generic;
using System.IO;
using System.Linq;
using SteamKit2.Internal;

namespace NetHookAnalyzer2.Specializations
{
	class ClientServiceMethodSpecialization : ISpecialization
	{
		public IEnumerable<KeyValuePair<string, object>> ReadExtraObjects(object messageObject)
		{
			var serviceMethodBody = messageObject as CMsgClientServiceMethodLegacy;
			if (serviceMethodBody == null)
			{
				yield break;
			}

			var name = serviceMethodBody.method_name;
			object innerBody;

			using (var ms = new MemoryStream(serviceMethodBody.serialized_method))
			{
				innerBody = UnifiedMessagingHelpers.ReadServiceMethodBody(name, ms, x => x.GetParameters().First().ParameterType);
			}

			yield return new KeyValuePair<string, object>("Service Method", innerBody);
		}
	}
}
