using System;
using Microsoft.Win32;

namespace NetHookAnalyzer2
{
	class SteamUtils
	{
		public static string GetSteamDirectory()
		{
			string installPath = "";

			try
			{
				installPath = (string)Registry.GetValue(
					 RegistryPathToSteam,
					 "InstallPath",
					 null);
			}
			catch
			{
			}

			return installPath;
		}

		static string RegistryPathToSteam
		{
			get { return Environment.Is64BitProcess ? @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Valve\Steam" : @"HKEY_LOCAL_MACHINE\Software\Valve\Steam"; }
		}
	}
}
