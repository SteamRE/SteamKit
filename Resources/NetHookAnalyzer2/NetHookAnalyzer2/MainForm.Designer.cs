namespace NetHookAnalyzer2
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.ColumnHeader sequenceColumnHeader;
			System.Windows.Forms.ColumnHeader directionColumnHeader;
			System.Windows.Forms.ColumnHeader messageColumnHeader;
			System.Windows.Forms.ColumnHeader innerMessageColumnHeader;
			System.Windows.Forms.ColumnHeader itemNameColumnHeader;
			System.Windows.Forms.ColumnHeader timestampColumnHeader;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openLatestFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.automaticallySelectNewItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.filterContainerPanel = new System.Windows.Forms.Panel();
			this.filterGroupBox = new System.Windows.Forms.GroupBox();
			this.showAllCheckBox = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.searchTextBox = new System.Windows.Forms.TextBox();
			this.outRadioButton = new System.Windows.Forms.RadioButton();
			this.inRadioButton = new System.Windows.Forms.RadioButton();
			this.inOutRadioButton = new System.Windows.Forms.RadioButton();
			this.listViewContainerPanel = new System.Windows.Forms.Panel();
			this.itemsListView = new NetHookAnalyzer2.ListViewDoubleBuffered();
			this.itemExplorerTreeView = new System.Windows.Forms.TreeView();
			sequenceColumnHeader = new System.Windows.Forms.ColumnHeader();
			directionColumnHeader = new System.Windows.Forms.ColumnHeader();
			messageColumnHeader = new System.Windows.Forms.ColumnHeader();
			innerMessageColumnHeader = new System.Windows.Forms.ColumnHeader();
			itemNameColumnHeader = new System.Windows.Forms.ColumnHeader();
			timestampColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.menuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.filterContainerPanel.SuspendLayout();
			this.filterGroupBox.SuspendLayout();
			this.listViewContainerPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// sequenceColumnHeader
			// 
			sequenceColumnHeader.Text = "#";
			sequenceColumnHeader.Width = 39;
			// 
			// directionColumnHeader
			// 
			directionColumnHeader.Text = "Direction";
			directionColumnHeader.Width = 54;
			// 
			// messageColumnHeader
			// 
			messageColumnHeader.Text = "Message";
			messageColumnHeader.Width = 146;
			// 
			// innerMessageColumnHeader
			// 
			innerMessageColumnHeader.Text = "Inner Message";
			innerMessageColumnHeader.Width = 165;
			// 
			// itemNameColumnHeader
			// 
			itemNameColumnHeader.Text = "";
			itemNameColumnHeader.Width = 0;
			// 
			// timestampColumnHeader
			// 
			timestampColumnHeader.Text = "Timestamp";
			timestampColumnHeader.Width = 134;
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.fileToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
			this.menuStrip1.Size = new System.Drawing.Size(1030, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.openToolStripMenuItem,
			this.openLatestFolderToolStripMenuItem,
			this.automaticallySelectNewItemsToolStripMenuItem,
			this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
			this.openToolStripMenuItem.Text = "&Open...";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.OnOpenToolStripMenuItemClick);
			// 
			// openLatestFolderToolStripMenuItem
			// 
			this.openLatestFolderToolStripMenuItem.Name = "openLatestFolderToolStripMenuItem";
			this.openLatestFolderToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
			this.openLatestFolderToolStripMenuItem.Text = "Open latest folder";
			this.openLatestFolderToolStripMenuItem.Click += new System.EventHandler(this.OnOpenLatestFolderToolStripMenuItemClick);
			// 
			// automaticallySelectNewItemsToolStripMenuItem
			// 
			this.automaticallySelectNewItemsToolStripMenuItem.CheckOnClick = true;
			this.automaticallySelectNewItemsToolStripMenuItem.Name = "automaticallySelectNewItemsToolStripMenuItem";
			this.automaticallySelectNewItemsToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
			this.automaticallySelectNewItemsToolStripMenuItem.Text = "&Automatically select new items";
			this.automaticallySelectNewItemsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.OnAutomaticallySelectNewItemsCheckedChanged);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.OnExitToolStripMenuItemClick);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = new System.Drawing.Point(0, 24);
			this.splitContainer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.filterContainerPanel);
			this.splitContainer.Panel1.Controls.Add(this.listViewContainerPanel);
			this.splitContainer.Panel1MinSize = 200;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.itemExplorerTreeView);
			this.splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(5);
			this.splitContainer.Size = new System.Drawing.Size(1030, 504);
			this.splitContainer.SplitterDistance = 485;
			this.splitContainer.SplitterWidth = 5;
			this.splitContainer.TabIndex = 2;
			// 
			// filterContainerPanel
			// 
			this.filterContainerPanel.Controls.Add(this.filterGroupBox);
			this.filterContainerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.filterContainerPanel.Location = new System.Drawing.Point(0, 0);
			this.filterContainerPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.filterContainerPanel.Name = "filterContainerPanel";
			this.filterContainerPanel.Padding = new System.Windows.Forms.Padding(5, 5, 5, 0);
			this.filterContainerPanel.Size = new System.Drawing.Size(485, 81);
			this.filterContainerPanel.TabIndex = 3;
			// 
			// filterGroupBox
			// 
			this.filterGroupBox.Controls.Add(this.showAllCheckBox);
			this.filterGroupBox.Controls.Add(this.label1);
			this.filterGroupBox.Controls.Add(this.searchTextBox);
			this.filterGroupBox.Controls.Add(this.outRadioButton);
			this.filterGroupBox.Controls.Add(this.inRadioButton);
			this.filterGroupBox.Controls.Add(this.inOutRadioButton);
			this.filterGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filterGroupBox.Location = new System.Drawing.Point(5, 5);
			this.filterGroupBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.filterGroupBox.Name = "filterGroupBox";
			this.filterGroupBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.filterGroupBox.Size = new System.Drawing.Size(475, 76);
			this.filterGroupBox.TabIndex = 1;
			this.filterGroupBox.TabStop = false;
			this.filterGroupBox.Text = "Filter";
			// 
			// showAllCheckBox
			// 
			this.showAllCheckBox.AutoSize = true;
			this.showAllCheckBox.Checked = true;
			this.showAllCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.showAllCheckBox.Location = new System.Drawing.Point(323, 22);
			this.showAllCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.showAllCheckBox.Name = "showAllCheckBox";
			this.showAllCheckBox.Size = new System.Drawing.Size(134, 19);
			this.showAllCheckBox.TabIndex = 3;
			this.showAllCheckBox.Text = "Show All Msg Values";
			this.showAllCheckBox.UseVisualStyleBackColor = true;
			this.showAllCheckBox.CheckedChanged += new System.EventHandler(this.OnShowAllCheckedChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 22);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 15);
			this.label1.TabIndex = 2;
			this.label1.Text = "Direction:";
			// 
			// searchTextBox
			// 
			this.searchTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.searchTextBox.ForeColor = System.Drawing.Color.LightGray;
			this.searchTextBox.Location = new System.Drawing.Point(4, 50);
			this.searchTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.searchTextBox.Name = "searchTextBox";
			this.searchTextBox.Size = new System.Drawing.Size(467, 23);
			this.searchTextBox.TabIndex = 1;
			this.searchTextBox.Text = "Search...";
			this.searchTextBox.TextChanged += new System.EventHandler(this.SearchTextBox_TextChanged);
			this.searchTextBox.Enter += new System.EventHandler(this.OnSearchTextBoxEnter);
			this.searchTextBox.Leave += new System.EventHandler(this.OnSearchTextBoxLeave);
			// 
			// outRadioButton
			// 
			this.outRadioButton.AutoSize = true;
			this.outRadioButton.Location = new System.Drawing.Point(197, 20);
			this.outRadioButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.outRadioButton.Name = "outRadioButton";
			this.outRadioButton.Size = new System.Drawing.Size(45, 19);
			this.outRadioButton.TabIndex = 0;
			this.outRadioButton.Text = "Out";
			this.outRadioButton.UseVisualStyleBackColor = true;
			this.outRadioButton.CheckedChanged += new System.EventHandler(this.OnDirectionFilterCheckedChanged);
			// 
			// inRadioButton
			// 
			this.inRadioButton.AutoSize = true;
			this.inRadioButton.Location = new System.Drawing.Point(150, 20);
			this.inRadioButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.inRadioButton.Name = "inRadioButton";
			this.inRadioButton.Size = new System.Drawing.Size(35, 19);
			this.inRadioButton.TabIndex = 0;
			this.inRadioButton.Text = "In";
			this.inRadioButton.UseVisualStyleBackColor = true;
			this.inRadioButton.CheckedChanged += new System.EventHandler(this.OnDirectionFilterCheckedChanged);
			// 
			// inOutRadioButton
			// 
			this.inOutRadioButton.AutoSize = true;
			this.inOutRadioButton.Checked = true;
			this.inOutRadioButton.Location = new System.Drawing.Point(78, 20);
			this.inOutRadioButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.inOutRadioButton.Name = "inOutRadioButton";
			this.inOutRadioButton.Size = new System.Drawing.Size(60, 19);
			this.inOutRadioButton.TabIndex = 0;
			this.inOutRadioButton.TabStop = true;
			this.inOutRadioButton.Text = "In/Out";
			this.inOutRadioButton.UseVisualStyleBackColor = true;
			this.inOutRadioButton.CheckedChanged += new System.EventHandler(this.OnDirectionFilterCheckedChanged);
			// 
			// listViewContainerPanel
			// 
			this.listViewContainerPanel.Controls.Add(this.itemsListView);
			this.listViewContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewContainerPanel.Location = new System.Drawing.Point(0, 0);
			this.listViewContainerPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.listViewContainerPanel.Name = "listViewContainerPanel";
			this.listViewContainerPanel.Padding = new System.Windows.Forms.Padding(5, 85, 5, 5);
			this.listViewContainerPanel.Size = new System.Drawing.Size(485, 504);
			this.listViewContainerPanel.TabIndex = 2;
			// 
			// itemsListView
			// 
			this.itemsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			itemNameColumnHeader,
			sequenceColumnHeader,
			timestampColumnHeader,
			directionColumnHeader,
			messageColumnHeader,
			innerMessageColumnHeader});
			this.itemsListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.itemsListView.FullRowSelect = true;
			this.itemsListView.GridLines = true;
			this.itemsListView.Location = new System.Drawing.Point(5, 85);
			this.itemsListView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.itemsListView.MultiSelect = false;
			this.itemsListView.Name = "itemsListView";
			this.itemsListView.Size = new System.Drawing.Size(475, 414);
			this.itemsListView.TabIndex = 0;
			this.itemsListView.UseCompatibleStateImageBehavior = false;
			this.itemsListView.View = System.Windows.Forms.View.Details;
			this.itemsListView.SelectedIndexChanged += new System.EventHandler(this.OnItemsListViewSelectedIndexChanged);
			// 
			// itemExplorerTreeView
			// 
			this.itemExplorerTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.itemExplorerTreeView.Location = new System.Drawing.Point(5, 5);
			this.itemExplorerTreeView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.itemExplorerTreeView.Name = "itemExplorerTreeView";
			this.itemExplorerTreeView.ShowRootLines = false;
			this.itemExplorerTreeView.Size = new System.Drawing.Size(530, 494);
			this.itemExplorerTreeView.TabIndex = 0;
			this.itemExplorerTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnItemExplorerTreeViewNodeMouseClick);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1030, 528);
			this.Controls.Add(this.splitContainer);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "MainForm";
			this.Text = "NetHook2 Dump Analyzer";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.filterContainerPanel.ResumeLayout(false);
			this.filterGroupBox.ResumeLayout(false);
			this.filterGroupBox.PerformLayout();
			this.listViewContainerPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.TextBox searchTextBox;
		private ListViewDoubleBuffered itemsListView;
		private System.Windows.Forms.Panel listViewContainerPanel;
		private System.Windows.Forms.Panel filterContainerPanel;
		private System.Windows.Forms.TreeView itemExplorerTreeView;
		private System.Windows.Forms.GroupBox filterGroupBox;
		private System.Windows.Forms.RadioButton outRadioButton;
		private System.Windows.Forms.RadioButton inRadioButton;
		private System.Windows.Forms.RadioButton inOutRadioButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ToolStripMenuItem automaticallySelectNewItemsToolStripMenuItem;
		private System.Windows.Forms.CheckBox showAllCheckBox;
		private System.Windows.Forms.ToolStripMenuItem openLatestFolderToolStripMenuItem;
	}
}

