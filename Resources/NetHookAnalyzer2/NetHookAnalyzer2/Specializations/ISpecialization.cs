using System.Collections.Generic;
using System.Windows.Forms;
using SteamKitten.Internal;

namespace NetHookAnalyzer2
{
	interface ISpecialization
	{
		IEnumerable<KeyValuePair<string, object>> ReadExtraObjects(object messageObject);
	}
}
