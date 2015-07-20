using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetHookAnalyzer2
{
	class NetHookDump
	{
		public NetHookDump()
		{
			items = new List<NetHookItem>();
		}

		List<NetHookItem> items;

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

		public IQueryable<NetHookItem> Items
		{
			get { return items.AsQueryable(); }
		}

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
