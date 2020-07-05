using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetHookAnalyzer2
{
	class NetHookDump
	{
		public NetHookDump()
		{
			items = new List<NetHookItem>();
			readOnlyView = items.AsReadOnly();
		}

		List<NetHookItem> items;
		IReadOnlyList<NetHookItem> readOnlyView;

		public void LoadFromDirectory(string directory)
		{
			items.Clear();

			var directoryInfo = new DirectoryInfo(directory);
			var itemFiles = directoryInfo.EnumerateFiles("*.bin", SearchOption.TopDirectoryOnly);
			foreach (var itemFile in itemFiles)
			{
				var item = new NetHookItem();
				if (item.LoadFromFile(itemFile))
				{
					items.Add(item);
				}
			}
		}

		public IEnumerable<NetHookItem> Items => readOnlyView;

		public NetHookItem AddItemFromPath(string path)
		{
			var fileInfo = new FileInfo(path);
			var item = new NetHookItem();
			if (!item.LoadFromFile(fileInfo))
			{
				return null;
			}

			items.Add(item);
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
