using System;
using KellermanSoftware.CompareNetObjects;

namespace DotaBot
{
	public static class Diff 
	{
		public static ComparisonResult Compare<T>(T obj1, T obj2)
		{
			var logic = new CompareLogic(){Config = new ComparisonConfig(){Caching = false, MaxDifferences = 100}};
			return logic.Compare (obj1, obj2);
		}
	}
}