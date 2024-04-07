using System;
using System.Drawing;
using System.IO;
using System.Linq;
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

#pragma warning disable IDE0069 // Disposable fields should be disposed

		IDisposable itemsListViewFirstColumnHiderDisposable;
		FileSystemWatcher folderWatcher;

#pragma warning restore IDE0069 // Disposable fields should be disposed

		readonly ISpecialization[] specializations;

		static ISpecialization[] LoadMessageObjectSpecializations()
		{
			return new ISpecialization[]
			{
				new ClientServiceMethodSpecialization(),
				new ClientServiceMethodResponseSpecialization(),
				new RemoteClientSteamToSteamSpecialization(),
				new RemoteClientSteamBroadcastSpecialization(),
				new GCGenericSpecialization()
				{
					GameCoordinatorSpecializations = new IGameCoordinatorSpecialization[]
					{
						new CSGOCacheSubscribedGCSpecialization(),
						new CSGOSOMultipleObjectsGCSpecialization(),
						new CSGOSOSingleObjectGCSpecialization(),
						new Dota2CacheSubscribedGCSpecialization(),
						new Dota2SOSingleObjectGCSpecialization(),
						new Dota2SOMultipleObjectsGCSpecialization(),
						new TF2CacheSubscribedGCSpecialization(),
						new TF2SOMultipleObjectsGCSpecialization(),
						new TF2SOSingleObjectGCSpecialization(),
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

		NetHookDump Dump;

		void RepopulateInterface()
		{
			RepopulateListBox();
			RepopulateTreeView();
		}

		Func<NetHookItem, bool> GetFilterPredicate()
		{

			var outAllowed = inOutRadioButton.Checked || outRadioButton.Checked;
			var inAllowed = inOutRadioButton.Checked || inRadioButton.Checked;
			bool directionPredicate( NetHookItem nhi ) => ( nhi.Direction == NetHookItem.PacketDirection.Out && outAllowed ) || ( nhi.Direction == NetHookItem.PacketDirection.In && inAllowed );

			var searchTerm = searchTextBox.Text;
			Predicate<NetHookItem> searchPredicate;
			if ( searchTerm == SearchTextBoxPlaceholderText || string.IsNullOrWhiteSpace( searchTerm ) )
			{
				searchPredicate = nhi => true;
			}
			else
			{
				searchPredicate = nhi => ( nhi.EMsg.ToString().IndexOf( searchTerm, StringComparison.InvariantCultureIgnoreCase ) >= 0 ) ||
					( nhi.InnerMessageName != null && nhi.InnerMessageName.IndexOf( searchTerm, StringComparison.InvariantCultureIgnoreCase ) >= 0 );
			}

			return nhi => directionPredicate( nhi ) && searchPredicate( nhi );
		}

		void RepopulateListBox()
		{
			var listViewItems = Dump.Items.Where(GetFilterPredicate()).Select(x => x.AsListViewItem());

			itemsListView.BeginUpdate();
			itemsListView.Items.Clear();
			itemsListView.Items.AddRange(listViewItems.ToArray());
			itemsListView.EndUpdate();
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

		void OnOpenToolStripMenuItemClick( object sender, EventArgs e )
		{
			string dumpDirectory;

			using ( var dialog = new FolderBrowserDialog() )
			{
				dialog.ShowNewFolderButton = false;

				var latestNethookDir = GetLatestNethookDumpDirectory();
				if ( latestNethookDir != null )
				{
					dialog.SelectedPath = latestNethookDir;
				}

				if ( dialog.ShowDialog() != WinForms.DialogResult.OK )
				{
					return;
				}

				dumpDirectory = dialog.SelectedPath;
			}

			OpenDirectory( dumpDirectory );

		}

		void OnOpenLatestFolderToolStripMenuItemClick( object sender, EventArgs e )
		{
            var steamDirectory = SteamUtils.GetSteamDirectory();
            if ( steamDirectory == null )
            {
                MessageBox.Show( "Failed to find Steam directory.", nameof( NetHookAnalyzer2 ), MessageBoxButtons.OK, MessageBoxIcon.Warning );
                return;
            }

            var nethookDirectory = Path.Combine( steamDirectory, "nethook" );
            if ( !Directory.Exists( nethookDirectory ) )
            {
                MessageBox.Show( $"Directory '{nethookDirectory}' does not exist.", nameof( NetHookAnalyzer2 ), MessageBoxButtons.OK, MessageBoxIcon.Warning );
                return;
            }

            var nethookDumpDirs = Directory.GetDirectories( nethookDirectory );
            var latestDump = nethookDumpDirs.LastOrDefault();
            if ( latestDump == null )
            {
                MessageBox.Show( $"There are no directories in '{nethookDirectory}'.", nameof( NetHookAnalyzer2 ), MessageBoxButtons.OK, MessageBoxIcon.Warning );
                return;
            }

            OpenDirectory( Path.Combine( nethookDirectory, latestDump ) );
        }

		void OpenDirectory(string dumpDirectory)
		{
			var dump = new NetHookDump();
			dump.LoadFromDirectory( dumpDirectory );
			Dump = dump;

			Text = string.Format( "NetHook2 Dump Analyzer - [{0}]", dumpDirectory );

			selectedListViewItem = null;
			RepopulateInterface();

			if (automaticallySelectNewItemsToolStripMenuItem.Checked)
			{
				SelectLastItem();
			}
			else if (itemsListView.Items.Count > 0)
			{
				itemsListView.Select();
				itemsListView.Items[ 0 ].Selected = true;
			}

			InitializeFileSystemWatcher(dumpDirectory);
		}

		void OnDirectionFilterCheckedChanged(object sender, EventArgs e)
		{
			RepopulateListBox();
		}

		void OnShowAllCheckedChanged(object sender, EventArgs e)
		{
			RepopulateTreeView();
		}

		void SearchTextBox_TextChanged(object sender, EventArgs e)
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

			folderWatcher.Changed += OnFolderWatcherChanged;
			folderWatcher.Created += OnFolderWatcherCreated;
			folderWatcher.Renamed += OnFolderWatcherRenamed;
			folderWatcher.Deleted += OnFolderWatcherDeleted;
			folderWatcher.EnableRaisingEvents = true;
			folderWatcher.IncludeSubdirectories = false;
			folderWatcher.SynchronizingObject = this;

			folderWatcher.EndInit();
		}

		void OnFolderWatcherChanged(object sender, FileSystemEventArgs e)
		{
			var item = Dump.Items.SingleOrDefault(x => x.FileInfo.FullName == e.FullPath);
			if (item == null)
			{
				return;
			}

			if (itemsListView.SelectedItems.Count == 0)
			{
				return;
			}

			foreach (var selectedItem in itemsListView.SelectedItems.Cast<ListViewItem>())
			{
				var tag = selectedItem.GetNetHookItem();
				if (tag == null)
				{
					continue;
				}

				if (tag != item)
				{
					continue;
				}

				RepopulateTreeView();
			}
		}

		void HandleFileCreated(string fullPath)
		{
			var item = Dump.AddItemFromFile(new FileInfo(fullPath));
			if (item == null)
			{
				return;
			}

			itemsListView.Invoke( ( MethodInvoker ) delegate ()
			{
				if (!GetFilterPredicate().Invoke(item))
				{
					return;
				}

				var listViewItem = item.AsListViewItem();

				itemsListView.BeginUpdate();
				itemsListView.Items.Add( listViewItem );

				if ( automaticallySelectNewItemsToolStripMenuItem.Checked )
				{
					SelectLastItem();
				}

				itemsListView.EndUpdate();
			} );
		}

		void OnFolderWatcherCreated(object sender, FileSystemEventArgs e)
		{
			HandleFileCreated(e.FullPath);
		}

		void OnFolderWatcherRenamed(object sender, RenamedEventArgs e)
		{
			HandleFileCreated(e.FullPath);
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

			itemExplorerTreeView.BeginUpdate();
			itemExplorerTreeView.Nodes.Clear();
			itemExplorerTreeView.Nodes.Add(BuildTree(item));
			itemExplorerTreeView.Nodes[0].EnsureVisible(); // Scroll to top
			itemExplorerTreeView.EndUpdate();
		}

		TreeNode BuildTree(NetHookItem item)
		{
			return NetHookItemTreeBuilder.BuildTree( item, specializations, showAllCheckBox.Checked );
		}

		void OnAutomaticallySelectNewItemsCheckedChanged(object sender, EventArgs e)
		{
			if (!automaticallySelectNewItemsToolStripMenuItem.Checked)
			{
				return;
			}

			SelectLastItem();
		}

		void SelectLastItem()
		{
			if (itemsListView.Items.Count == 0)
			{
				return;
			}

			var lastItem = itemsListView.Items[ ^1 ];
			if (!lastItem.Selected)
			{
				lastItem.Selected = true;
				lastItem.Focused = true;
				lastItem.EnsureVisible();
			}
		}

		private void OnItemExplorerTreeViewNodeMouseClick( object sender, TreeNodeMouseClickEventArgs e )
		{
			if ( e.Button == MouseButtons.Right && e.Node is TreeNodeObjectExplorer objectExplorer )
			{
				objectExplorer.CreateContextMenu();
			}
		}
	}
}
