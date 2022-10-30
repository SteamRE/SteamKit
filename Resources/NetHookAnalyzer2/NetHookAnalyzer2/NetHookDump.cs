using System.Collections.Generic;
using System.IO;
using System.Linq;
using SteamKit2.Internal;

namespace NetHookAnalyzer2
{
	internal class NetHookDump
	{
		public NetHookDump()
		{
			items = new List<NetHookItem>();
			readOnlyView = items.AsReadOnly();
		}

		List<NetHookItem> items;
		IReadOnlyList<NetHookItem> readOnlyView;
		static Dictionary<int, byte[]> accountAuthSecrets = new();

		public void LoadFromDirectory(string directory)
		{
			items.Clear();

			var directoryInfo = new DirectoryInfo(directory);
			var itemFiles = directoryInfo.EnumerateFiles("*.bin", SearchOption.TopDirectoryOnly);
			foreach (var itemFile in itemFiles)
			{
				AddItemFromFile(itemFile);
			}
		}

		public IEnumerable<NetHookItem> Items => readOnlyView;

		public static byte[] GetAccountAuthSecret(int secretId) => accountAuthSecrets.GetValueOrDefault(secretId);

		public NetHookItem AddItemFromFile(FileInfo fileInfo)
		{
			var item = new NetHookItem();
			if (!item.LoadFromFile(fileInfo))
			{
				return null;
			}

			items.Add(item);

			if (item.EMsg == SteamKit2.EMsg.ServiceMethodResponse && item.InnerMessageName == "Credentials.GetAccountAuthSecret#1")
			{
				var authSecretBody = item.ReadFile().Body as CCredentials_GetAccountAuthSecret_Response;

				if (authSecretBody != null)
				{
					accountAuthSecrets[ authSecretBody.secret_id ] = authSecretBody.secret;
				}
			}

			return item;
		}

		public NetHookItem RemoveItemWithPath(string path)
		{
			var item = items.SingleOrDefault(x => x.FileInfo.FullName == path);
			if (item == null)
			{
				return null;
			}

			items.Remove(item);
			return item;
		}
	}
}
