using System.Collections.Generic;

namespace NetHookAnalyzer2.Specializations
{
	interface IGameCoordinatorSpecialization
	{
		IEnumerable<KeyValuePair<string, object>> GetExtraObjects(object body, uint appID);
	}
}
