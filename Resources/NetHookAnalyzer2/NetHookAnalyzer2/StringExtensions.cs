namespace NetHookAnalyzer2
{
	static class StringExtensions
	{
		public static string TrimStart(this string baseString, string startToTrim)
		{
			if (baseString.IndexOf(startToTrim) == 0)
			{
				return baseString.Substring(startToTrim.Length);
			}

			return baseString;
		}
	}
}
