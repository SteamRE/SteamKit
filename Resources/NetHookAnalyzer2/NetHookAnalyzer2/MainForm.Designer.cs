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
			components = new System.ComponentModel.Container();
			System.Windows.Forms.ColumnHeader sequenceColumnHeader;
			System.Windows.Forms.ColumnHeader directionColumnHeader;
			System.Windows.Forms.ColumnHeader messageColumnHeader;
			System.Windows.Forms.ColumnHeader innerMessageColumnHeader;
			System.Windows.Forms.ColumnHeader itemNameColumnHeader;
			System.Windows.Forms.ColumnHeader timestampColumnHeader;
			var resources = new System.ComponentModel.ComponentResourceManager( typeof( MainForm ) );
			menuStrip1 = new System.Windows.Forms.MenuStrip();
			fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			openLatestFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			automaticallySelectNewItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip( components );
			splitContainer = new System.Windows.Forms.SplitContainer();
			filterContainerPanel = new System.Windows.Forms.Panel();
			filterGroupBox = new System.Windows.Forms.GroupBox();
			showAllCheckBox = new System.Windows.Forms.CheckBox();
			label1 = new System.Windows.Forms.Label();
			searchTextBox = new System.Windows.Forms.TextBox();
			outRadioButton = new System.Windows.Forms.RadioButton();
			inRadioButton = new System.Windows.Forms.RadioButton();
			inOutRadioButton = new System.Windows.Forms.RadioButton();
			listViewContainerPanel = new System.Windows.Forms.Panel();
			itemsListView = new ListViewDoubleBuffered();
			itemExplorerTreeView = new System.Windows.Forms.TreeView();
			sequenceColumnHeader = new System.Windows.Forms.ColumnHeader();
			directionColumnHeader = new System.Windows.Forms.ColumnHeader();
			messageColumnHeader = new System.Windows.Forms.ColumnHeader();
			innerMessageColumnHeader = new System.Windows.Forms.ColumnHeader();
			itemNameColumnHeader = new System.Windows.Forms.ColumnHeader();
			timestampColumnHeader = new System.Windows.Forms.ColumnHeader();
			menuStrip1.SuspendLayout();
			( ( System.ComponentModel.ISupportInitialize )splitContainer ).BeginInit();
			splitContainer.Panel1.SuspendLayout();
			splitContainer.Panel2.SuspendLayout();
			splitContainer.SuspendLayout();
			filterContainerPanel.SuspendLayout();
			filterGroupBox.SuspendLayout();
			listViewContainerPanel.SuspendLayout();
			SuspendLayout();
			// 
			// sequenceColumnHeader
			// 
			sequenceColumnHeader.Text = "#";
			sequenceColumnHeader.Width = 39;
			// 
			// directionColumnHeader
			// 
			directionColumnHeader.Text = "Dir.";
			directionColumnHeader.Width = 40;
			// 
			// messageColumnHeader
			// 
			messageColumnHeader.Text = "Message";
			messageColumnHeader.Width = 170;
			// 
			// innerMessageColumnHeader
			// 
			innerMessageColumnHeader.Text = "Inner Message";
			innerMessageColumnHeader.Width = 170;
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
			menuStrip1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem } );
			menuStrip1.Location = new System.Drawing.Point( 0, 0 );
			menuStrip1.Name = "menuStrip1";
			menuStrip1.Padding = new System.Windows.Forms.Padding( 7, 2, 0, 2 );
			menuStrip1.Size = new System.Drawing.Size( 1184, 24 );
			menuStrip1.TabIndex = 0;
			menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] { openToolStripMenuItem, openLatestFolderToolStripMenuItem, automaticallySelectNewItemsToolStripMenuItem, exitToolStripMenuItem } );
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new System.Drawing.Size( 37, 20 );
			fileToolStripMenuItem.Text = "&File";
			// 
			// openToolStripMenuItem
			// 
			openToolStripMenuItem.Image = ( System.Drawing.Image )resources.GetObject( "openToolStripMenuItem.Image" );
			openToolStripMenuItem.Name = "openToolStripMenuItem";
			openToolStripMenuItem.ShortcutKeys =    System.Windows.Forms.Keys.Control  |  System.Windows.Forms.Keys.O ;
			openToolStripMenuItem.Size = new System.Drawing.Size( 238, 22 );
			openToolStripMenuItem.Text = "&Open...";
			openToolStripMenuItem.Click +=  OnOpenToolStripMenuItemClick ;
			// 
			// openLatestFolderToolStripMenuItem
			// 
			openLatestFolderToolStripMenuItem.Name = "openLatestFolderToolStripMenuItem";
			openLatestFolderToolStripMenuItem.Size = new System.Drawing.Size( 238, 22 );
			openLatestFolderToolStripMenuItem.Text = "Open latest folder";
			openLatestFolderToolStripMenuItem.Click +=  OnOpenLatestFolderToolStripMenuItemClick ;
			// 
			// automaticallySelectNewItemsToolStripMenuItem
			// 
			automaticallySelectNewItemsToolStripMenuItem.CheckOnClick = true;
			automaticallySelectNewItemsToolStripMenuItem.Name = "automaticallySelectNewItemsToolStripMenuItem";
			automaticallySelectNewItemsToolStripMenuItem.Size = new System.Drawing.Size( 238, 22 );
			automaticallySelectNewItemsToolStripMenuItem.Text = "&Automatically select new items";
			automaticallySelectNewItemsToolStripMenuItem.CheckedChanged +=  OnAutomaticallySelectNewItemsCheckedChanged ;
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.ShortcutKeys =    System.Windows.Forms.Keys.Alt  |  System.Windows.Forms.Keys.F4 ;
			exitToolStripMenuItem.Size = new System.Drawing.Size( 238, 22 );
			exitToolStripMenuItem.Text = "E&xit";
			exitToolStripMenuItem.Click +=  OnExitToolStripMenuItemClick ;
			// 
			// contextMenuStrip1
			// 
			contextMenuStrip1.Name = "contextMenuStrip1";
			contextMenuStrip1.Size = new System.Drawing.Size( 61, 4 );
			// 
			// splitContainer
			// 
			splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer.Location = new System.Drawing.Point( 0, 24 );
			splitContainer.Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			splitContainer.Panel1.Controls.Add( filterContainerPanel );
			splitContainer.Panel1.Controls.Add( listViewContainerPanel );
			splitContainer.Panel1MinSize = 200;
			// 
			// splitContainer.Panel2
			// 
			splitContainer.Panel2.Controls.Add( itemExplorerTreeView );
			splitContainer.Panel2.Padding = new System.Windows.Forms.Padding( 5 );
			splitContainer.Size = new System.Drawing.Size( 1184, 737 );
			splitContainer.SplitterDistance = 600;
			splitContainer.SplitterWidth = 5;
			splitContainer.TabIndex = 2;
			// 
			// filterContainerPanel
			// 
			filterContainerPanel.Controls.Add( filterGroupBox );
			filterContainerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			filterContainerPanel.Location = new System.Drawing.Point( 0, 0 );
			filterContainerPanel.Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			filterContainerPanel.Name = "filterContainerPanel";
			filterContainerPanel.Padding = new System.Windows.Forms.Padding( 5, 5, 5, 0 );
			filterContainerPanel.Size = new System.Drawing.Size( 600, 81 );
			filterContainerPanel.TabIndex = 3;
			// 
			// filterGroupBox
			// 
			filterGroupBox.Controls.Add( showAllCheckBox );
			filterGroupBox.Controls.Add( label1 );
			filterGroupBox.Controls.Add( searchTextBox );
			filterGroupBox.Controls.Add( outRadioButton );
			filterGroupBox.Controls.Add( inRadioButton );
			filterGroupBox.Controls.Add( inOutRadioButton );
			filterGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			filterGroupBox.Location = new System.Drawing.Point( 5, 5 );
			filterGroupBox.Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			filterGroupBox.Name = "filterGroupBox";
			filterGroupBox.Padding = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			filterGroupBox.Size = new System.Drawing.Size( 590, 76 );
			filterGroupBox.TabIndex = 1;
			filterGroupBox.TabStop = false;
			filterGroupBox.Text = "Filter";
			// 
			// showAllCheckBox
			// 
			showAllCheckBox.AutoSize = true;
			showAllCheckBox.Checked = true;
			showAllCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			showAllCheckBox.Location = new System.Drawing.Point( 323, 22 );
			showAllCheckBox.Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			showAllCheckBox.Name = "showAllCheckBox";
			showAllCheckBox.Size = new System.Drawing.Size( 132, 19 );
			showAllCheckBox.TabIndex = 3;
			showAllCheckBox.Text = "Show Default Values";
			showAllCheckBox.UseVisualStyleBackColor = true;
			showAllCheckBox.CheckedChanged +=  OnShowAllCheckedChanged ;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point( 10, 22 );
			label1.Margin = new System.Windows.Forms.Padding( 4, 0, 4, 0 );
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size( 58, 15 );
			label1.TabIndex = 2;
			label1.Text = "Direction:";
			// 
			// searchTextBox
			// 
			searchTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			searchTextBox.ForeColor = System.Drawing.Color.LightGray;
			searchTextBox.Location = new System.Drawing.Point( 4, 50 );
			searchTextBox.Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			searchTextBox.Name = "searchTextBox";
			searchTextBox.Size = new System.Drawing.Size( 582, 23 );
			searchTextBox.TabIndex = 1;
			searchTextBox.Text = "Search...";
			searchTextBox.TextChanged +=  SearchTextBox_TextChanged ;
			searchTextBox.Enter +=  OnSearchTextBoxEnter ;
			searchTextBox.Leave +=  OnSearchTextBoxLeave ;
			// 
			// outRadioButton
			// 
			outRadioButton.AutoSize = true;
			outRadioButton.Location = new System.Drawing.Point( 197, 20 );
			outRadioButton.Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			outRadioButton.Name = "outRadioButton";
			outRadioButton.Size = new System.Drawing.Size( 45, 19 );
			outRadioButton.TabIndex = 0;
			outRadioButton.Text = "Out";
			outRadioButton.UseVisualStyleBackColor = true;
			outRadioButton.CheckedChanged +=  OnDirectionFilterCheckedChanged ;
			// 
			// inRadioButton
			// 
			inRadioButton.AutoSize = true;
			inRadioButton.Location = new System.Drawing.Point( 150, 20 );
			inRadioButton.Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			inRadioButton.Name = "inRadioButton";
			inRadioButton.Size = new System.Drawing.Size( 35, 19 );
			inRadioButton.TabIndex = 0;
			inRadioButton.Text = "In";
			inRadioButton.UseVisualStyleBackColor = true;
			inRadioButton.CheckedChanged +=  OnDirectionFilterCheckedChanged ;
			// 
			// inOutRadioButton
			// 
			inOutRadioButton.AutoSize = true;
			inOutRadioButton.Checked = true;
			inOutRadioButton.Location = new System.Drawing.Point( 78, 20 );
			inOutRadioButton.Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			inOutRadioButton.Name = "inOutRadioButton";
			inOutRadioButton.Size = new System.Drawing.Size( 60, 19 );
			inOutRadioButton.TabIndex = 0;
			inOutRadioButton.TabStop = true;
			inOutRadioButton.Text = "In/Out";
			inOutRadioButton.UseVisualStyleBackColor = true;
			inOutRadioButton.CheckedChanged +=  OnDirectionFilterCheckedChanged ;
			// 
			// listViewContainerPanel
			// 
			listViewContainerPanel.Controls.Add( itemsListView );
			listViewContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			listViewContainerPanel.Location = new System.Drawing.Point( 0, 0 );
			listViewContainerPanel.Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			listViewContainerPanel.Name = "listViewContainerPanel";
			listViewContainerPanel.Padding = new System.Windows.Forms.Padding( 5, 85, 5, 5 );
			listViewContainerPanel.Size = new System.Drawing.Size( 600, 737 );
			listViewContainerPanel.TabIndex = 2;
			// 
			// itemsListView
			// 
			itemsListView.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] { itemNameColumnHeader, sequenceColumnHeader, timestampColumnHeader, directionColumnHeader, messageColumnHeader, innerMessageColumnHeader } );
			itemsListView.Dock = System.Windows.Forms.DockStyle.Fill;
			itemsListView.FullRowSelect = true;
			itemsListView.GridLines = true;
			itemsListView.Location = new System.Drawing.Point( 5, 85 );
			itemsListView.Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			itemsListView.MultiSelect = false;
			itemsListView.Name = "itemsListView";
			itemsListView.Size = new System.Drawing.Size( 590, 647 );
			itemsListView.TabIndex = 0;
			itemsListView.UseCompatibleStateImageBehavior = false;
			itemsListView.View = System.Windows.Forms.View.Details;
			itemsListView.SelectedIndexChanged +=  OnItemsListViewSelectedIndexChanged ;
			// 
			// itemExplorerTreeView
			// 
			itemExplorerTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			itemExplorerTreeView.Location = new System.Drawing.Point( 5, 5 );
			itemExplorerTreeView.Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			itemExplorerTreeView.Name = "itemExplorerTreeView";
			itemExplorerTreeView.ShowRootLines = false;
			itemExplorerTreeView.Size = new System.Drawing.Size( 569, 727 );
			itemExplorerTreeView.TabIndex = 0;
			itemExplorerTreeView.NodeMouseClick +=  OnItemExplorerTreeViewNodeMouseClick ;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF( 7F, 15F );
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size( 1184, 761 );
			Controls.Add( splitContainer );
			Controls.Add( menuStrip1 );
			Icon = ( System.Drawing.Icon )resources.GetObject( "$this.Icon" );
			Margin = new System.Windows.Forms.Padding( 4, 3, 4, 3 );
			Name = "MainForm";
			Text = "NetHook2 Dump Analyzer";
			Load +=  OnFormLoad ;
			menuStrip1.ResumeLayout( false );
			menuStrip1.PerformLayout();
			splitContainer.Panel1.ResumeLayout( false );
			splitContainer.Panel2.ResumeLayout( false );
			( ( System.ComponentModel.ISupportInitialize )splitContainer ).EndInit();
			splitContainer.ResumeLayout( false );
			filterContainerPanel.ResumeLayout( false );
			filterGroupBox.ResumeLayout( false );
			filterGroupBox.PerformLayout();
			listViewContainerPanel.ResumeLayout( false );
			ResumeLayout( false );
			PerformLayout();
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

