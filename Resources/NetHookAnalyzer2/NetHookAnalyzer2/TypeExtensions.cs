using System;
using System.Collections;

namespace NetHookAnalyzer2
{
	static class TypeExtensions
	{
		public static bool IsDictionaryType(this Type type)
		{
			foreach (var @interface in type.GetInterfaces())
			{
				if (@interface == typeof(IDictionary))
					return true;
			}

			return false;
		}

		public static bool IsEnumerableType(this Type type)
		{
			foreach (var @interface in type.GetInterfaces())
			{
				if (@interface == typeof(IEnumerable))
					return true;
			}

			return false;
		}
	}
}
