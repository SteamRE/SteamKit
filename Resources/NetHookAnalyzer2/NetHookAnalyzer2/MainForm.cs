using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;
using NetHookAnalyzer2.Specializations;
using WinForms = System.Windows.Forms;

namespace NetHookAnalyzer2
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			Dump = new NetHookDump();

			selectedListViewItem = null;
			RepopulateInterface();

			itemsListView.ListViewItemSorter = new NetHookListViewItemSequentialComparer();
			specializations = LoadMessageObjectSpecializations();
		}

		IDisposable itemsListViewFirstColumnHiderDisposable;
		FileSystemWatcher folderWatcher;
		readonly ISpecialization[] specializations;

		static ISpecialization[] LoadMessageObjectSpecializations()
		{
			return new ISpecialization[]
			{
				new ClientServiceMethodSpecialization(),
				new ClientServiceMethodResponseSpecialization(),
				new GCGenericSpecialization()
				{
					GameCoordinatorSpecializations = new[]
					{
						new Dota2SOMultipleObjectsGCSpecialization(),
					}
				}
			};
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			if (itemsListViewFirstColumnHiderDisposable != null)
			{
				itemsListViewFirstColumnHiderDisposable.Dispose();
				itemsListViewFirstColumnHiderDisposable = null;
			}

			if (folderWatcher != null)
			{
				folderWatcher.Dispose();
				folderWatcher = null;
			}

			base.OnFormClosed(e);
		}

		#region

		string GetLatestNethookDumpDirectory()
		{
			var steamDirectory = SteamUtils.GetSteamDirectory();
			if (steamDirectory == null)
			{
				return null;
			}

			var nethookDirectory = Path.Combine(steamDirectory, "nethook");

			if (!Directory.Exists(nethookDirectory))
			{
				return steamDirectory;
			}

			var nethookDumpDirs = Directory.GetDirectories(nethookDirectory);
			var latestDump = nethookDumpDirs.LastOrDefault();
			if (latestDump == null)
			{
				return nethookDirectory;
			}

			return Path.Combine(nethookDirectory, latestDump);
		}

		NetHookDump Dump { get; set; }

		void RepopulateInterface()
		{
			RepopulateListBox();
			RepopulateTreeView();
		}

		void RepopulateListBox()
		{
			var searchTerm = searchTextBox.Text;
			Expression<Func<NetHookItem, bool>> predicate;
			if (searchTerm == SearchTextBoxPlaceholderText || string.IsNullOrWhiteSpace(searchTerm))
			{
				predicate = nhi => true;
			}
			else
			{
				predicate = nhi => (nhi.Name.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
					(nhi.InnerMessageName != null && nhi.InnerMessageName.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) >= 0);
			}

			var outAllowed = inOutRadioButton.Checked || outRadioButton.Checked;
			var inAllowed = inOutRadioButton.Checked || inRadioButton.Checked;
			Expression<Func<NetHookItem, bool>> directionPredicate = nhi => (nhi.Direction == NetHookItem.PacketDirection.Out && outAllowed) || (nhi.Direction == NetHookItem.PacketDirection.In && inAllowed);

			var listViewItems = Dump.Items.Where(directionPredicate).Where(predicate).Select(x => x.AsListViewItem());

			itemsListView.Items.Clear();
			itemsListView.Items.AddRange(listViewItems.ToArray());
		}

		#endregion

		#region UI Events

		void OnFormLoad(object sender, EventArgs e)
		{
			itemsListViewFirstColumnHiderDisposable = new ListViewColumnHider(itemsListView, 0);
		}

		void OnExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Application.Exit();
		}

		void OnOpenToolStripMenuItemClick(object sender, EventArgs e)
		{
			var dialog = new FolderBrowserDialog { ShowNewFolderButton = false };
			var latestNethookDir = GetLatestNethookDumpDirectory();
			if (latestNethookDir != null)
			{
				dialog.SelectedPath = GetLatestNethookDumpDirectory();
			}

			if (dialog.ShowDialog() != WinForms.DialogResult.OK)
			{
				return;
			}

			var dumpDirectory = dialog.SelectedPath;

			var dump = new NetHookDump();
			dump.LoadFromDirectory(dumpDirectory);
			Dump = dump;

			Text = string.Format("NetHook2 Dump Analyzer - [{0}]", dumpDirectory);

			selectedListViewItem = null;
			RepopulateInterface();

			if (itemsListView.Items.Count > 0)
			{
				itemsListView.Select();
				itemsListView.Items[0].Selected = true;
			}

			InitializeFileSystemWatcher(dumpDirectory);
		}

		void OnDirectionFilterCheckedChanged(object sender, EventArgs e)
		{
			RepopulateListBox();
		}

		void searchTextBox_TextChanged(object sender, EventArgs e)
		{
			RepopulateListBox();
		}

		#region SearchTextBox Placeholder Text

		const string SearchTextBoxPlaceholderText = "Search...";
		Color SearchTextBoxPlaceholderColor = Color.LightGray;
		Color SearchTextBoxUserTextColor = Color.Black;

		void OnSearchTextBoxEnter(object sender, EventArgs e)
		{
			if (searchTextBox.Text == SearchTextBoxPlaceholderText)
			{
				searchTextBox.Text = string.Empty;
				searchTextBox.ForeColor = SearchTextBoxUserTextColor;
			}
			else
			{
				searchTextBox.SelectAll();
			}
		}

		void OnSearchTextBoxLeave(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(searchTextBox.Text))
			{
				searchTextBox.Text = SearchTextBoxPlaceholderText;
				searchTextBox.ForeColor = SearchTextBoxPlaceholderColor;
			}
		}

		#endregion


		#endregion

		#region FileSystem Watcher

		void InitializeFileSystemWatcher(string path)
		{
			if (folderWatcher != null)
			{
				folderWatcher.Dispose();
			}

			folderWatcher = new FileSystemWatcher(path, "*.bin");
			folderWatcher.BeginInit();

			folderWatcher.Created += OnFolderWatcherCreated;
			folderWatcher.Deleted += OnFolderWatcherDeleted;
			folderWatcher.EnableRaisingEvents = true;
			folderWatcher.IncludeSubdirectories = false;
			folderWatcher.SynchronizingObject = this;

			folderWatcher.EndInit();
		}

		void OnFolderWatcherCreated(object sender, FileSystemEventArgs e)
		{
			var item = Dump.AddItemFromPath(e.FullPath);
			if (item == null)
			{
				return;
			}

			var listViewItem = item.AsListViewItem();
			itemsListView.Items.Add(listViewItem);
		}

		void OnFolderWatcherDeleted(object sender, FileSystemEventArgs e)
		{
			var item = Dump.RemoveItemWithPath(e.FullPath);
			if (item == null)
			{
				return;
			}

			var listViewItem = itemsListView.Items.Cast<ListViewItem>().SingleOrDefault(x => x.Tag == item);
			if (listViewItem == null)
			{
				return;
			}

			itemsListView.Items.Remove(listViewItem);
		}

		#endregion

		ListViewItem selectedListViewItem;

		void OnItemsListViewSelectedIndexChanged(object sender, EventArgs e)
		{
			if (itemsListView.SelectedItems.Count != 1)
			{
				return;
			}

			var selectedItem = itemsListView.SelectedItems[0];
			if (selectedItem != selectedListViewItem)
			{
				selectedListViewItem = selectedItem;
				RepopulateTreeView();
			}
		}

		void RepopulateTreeView()
		{
			if (selectedListViewItem == null)
			{
				return;
			}

			var item = selectedListViewItem.GetNetHookItem();
			if (item == null)
			{
				return;
			}

			itemExplorerTreeView.Nodes.Clear();
			itemExplorerTreeView.Nodes.AddRange(BuildTree(item).Nodes.Cast<TreeNode>().ToArray());
			itemExplorerTreeView.Nodes[0].EnsureVisible(); // Scroll to top
		}

		TreeNode BuildTree(NetHookItem item)
		{
			return new NetHookItemTreeBuilder(item) { Specializations = specializations }.BuildTree();
		}
	}
}
