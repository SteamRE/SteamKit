namespace NetHookAnalyzer2
{
	static class StringExtensions
	{
		public static string TrimStart( this string baseString, string startToTrim )
		{
			if ( baseString.StartsWith( startToTrim, System.StringComparison.Ordinal ) )
			{
				return baseString[ startToTrim.Length.. ];
			}

			return baseString;
		}
	}
}
