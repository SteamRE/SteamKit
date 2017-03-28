using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using SteamKit2;
using SteamKit2.Internal;

namespace NetHookAnalyzer2
{
	class TreeNodeObjectExplorer
	{
		public TreeNodeObjectExplorer(string name, object value, TreeNodeObjectExplorerConfiguration configuration)
		{
			this.name = name;
			this.value = value;
			this.configuration = configuration;

			this.treeNode = new TreeNode();

			if (configuration.IsUnsetField)
			{
			    treeNode.ForeColor = System.Drawing.Color.DarkGray;
			}
			this.treeNode.ContextMenu = new ContextMenu();
			this.treeNode.ContextMenu.Popup += OnContextMenuPopup;

			Initialize();
		}

		readonly string name;
		readonly object value;
		TreeNodeObjectExplorerConfiguration configuration;
		readonly TreeNode treeNode;
		string clipboardCopyOverride;

		public TreeNode TreeNode
		{
			get { return treeNode; }
		}

		Menu.MenuItemCollection ContextMenuItems
		{
			get { return TreeNode.ContextMenu.MenuItems; }
		}

		string ValueForDisplay
		{
			get { return valueForDisplay; }
			set
			{
				valueForDisplay = value;
				UpdateDisplayText();
			}
		}
		string valueForDisplay;

		#region Context Menu Actions

		#region Copy to Clipboard

		void CopyNameToClipboard(object sender, EventArgs e)
		{
			Clipboard.SetText(name, TextDataFormat.Text);
		}

		void CopyValueToClipboard(object sender, EventArgs e)
		{
			var valueToCopy = clipboardCopyOverride ?? ValueForDisplay;
			Clipboard.SetText(valueToCopy, TextDataFormat.Text);
		}
		void CopyNameAndValueToClipboard(object sender, EventArgs e)
		{
			Clipboard.SetText(TreeNode.Text, TextDataFormat.Text);
		}

		#endregion

		#region Binary Data

		const int MaxDataLengthForDisplay = 400;

		void SaveDataToFile(object sender, EventArgs e)
		{
			var data = (byte[])value;

			var dialog = new SaveFileDialog { DefaultExt = "bin", SupportMultiDottedExtensions = true };
			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				File.WriteAllBytes(dialog.FileName, data);
			}
		}

		void DisplayDataAsAscii(object sender, EventArgs e)
		{
			SetAsRadioSelected(sender);

			var data = (byte[])value;
			SetValueForDisplay(Encoding.ASCII.GetString(data).Replace("\0", "\\0"));
		}

		void DisplayDataAsUTF8(object sender, EventArgs e)
		{
			SetAsRadioSelected(sender);

			var data = (byte[])value;
			SetValueForDisplay(Encoding.UTF8.GetString(data).Replace("\0", "\\0"));
		}

		void DisplayDataAsHexadecimal(object sender, EventArgs e)
		{
			SetAsRadioSelected(sender);

			var data = (byte[])value;
			var hexString = data.Aggregate(new StringBuilder(), (str, val) => str.Append(val.ToString("X2"))).ToString();
			SetValueForDisplay(hexString);

		}

		void DisplayDataAsBinaryKeyValues(object sender, EventArgs e)
		{
			SetAsRadioSelected(sender);

			var data = (byte[])value;
			var kv = new KeyValue();
			bool didRead;
			using (var ms = new MemoryStream(data))
			{
				didRead = kv.TryReadAsBinary(ms);
			}

			if (!didRead)
			{
				SetValueForDisplay("Not a valid KeyValues object!");
			}
			else
			{
				SetValueForDisplay(null, childNodes: new[] { new TreeNodeObjectExplorer(kv.Name, kv, configuration).TreeNode });
			}

			TreeNode.ExpandAll();
		}

		#endregion

		#region IP Address

		void DisplayAsIPAddress(object sender, EventArgs e)
		{
			SetAsRadioSelected(sender);

			var addressInteger = (long)Convert.ChangeType(value, typeof(long));
			var addressScratch = BitConverter.GetBytes(addressInteger);
			Array.Reverse(addressScratch);

			var addressBytes = new byte[sizeof(uint)];
			Array.Copy(addressScratch, sizeof(uint), addressBytes, 0, addressBytes.Length);

			var address = new IPAddress(addressBytes);
			SetValueForDisplay(address.ToString());
		}

		#endregion

		#region Steam ID

		void DisplayAsSteam2ID(object sender, EventArgs e)
		{
			SetAsRadioSelected(sender);

			var steamID = ConvertToSteamID(value);
			SetValueForDisplay(steamID.Render(steam3: false));
		}

		void DisplayAsSteam3ID(object sender, EventArgs e)
		{
			SetAsRadioSelected(sender);

			var steamID = ConvertToSteamID(value);
			SetValueForDisplay(steamID.Render(steam3: true));
		}

		SteamID ConvertToSteamID(object value)
		{
			// first check if the given value is already a steamid
			SteamID steamID = value as SteamID;

			if (steamID == null)
			{
				// if not, try converting
				steamID = new SteamID((ulong)Convert.ChangeType(value, typeof(ulong)));
			}

			return steamID;
		}

		#endregion

		#region GlobalID

		void DisplayAsGlobalID(object sender, EventArgs e)
		{
			SetAsRadioSelected(sender);

			var gid = new GlobalID((ulong)Convert.ChangeType(value, typeof(ulong)));
			var children = new[]
			{
				new TreeNodeObjectExplorer("Box", gid.BoxID, configuration).TreeNode,
				new TreeNodeObjectExplorer("Process ID", gid.ProcessID, configuration).TreeNode,
				new TreeNodeObjectExplorer("Sequential Count", gid.SequentialCount, configuration).TreeNode,
				new TreeNodeObjectExplorer("StartTime", gid.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), configuration).TreeNode
			};

			SetValueForDisplay(null, childNodes: children);
		}

		#endregion

		#region Date/Time

		void DisplayAsPosixTimestamp(object sender, EventArgs e)
		{
			SetAsRadioSelected(sender);

			var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			try
			{
				var dateTimeValue = unixEpoch.AddSeconds((double)Convert.ChangeType(value, typeof(double))).ToString("yyyy-MM-dd HH:mm:ss");
				SetValueForDisplay(dateTimeValue);
			}
			catch (ArgumentOutOfRangeException)
			{
				SetValueForDisplay("Out of range!");
			}
			catch (InvalidCastException)
			{
				SetValueForDisplay("Out of range!");
			}
		}

		#endregion

		#region Enum

		void DisplayAsEnumMember(Type enumType)
		{
			object enumValue;
			try
			{
				enumValue = Convert.ChangeType(value, enumType.GetEnumUnderlyingType());
			}
			catch (OverflowException)
			{
				SetValueForDisplay(string.Format("{0} (not convertible to '{1}')", value, enumType.Name), value.ToString());
				return;
			}

			if (enumType.GetCustomAttribute<FlagsAttribute>() != null)
			{
				var allEnumFlags = Enum.GetValues(enumType)
					.Cast<object>().Select(x => (long)Convert.ChangeType(x, typeof(long))) // .Cast<long> fails to unbox int-backed enums
					.Aggregate(0L, (acc, val) => acc |= val);

				if ((~allEnumFlags & (long)Convert.ChangeType(enumValue, typeof(long))) > 0)
				{
					SetValueForDisplay(string.Format("{0} (not a valid combination of '{1}')", value, enumType.Name), value.ToString());
				}
				else
				{
					SetValueForDisplay(string.Format("{0:D} = {0:G}", Enum.ToObject(enumType, enumValue)));
				}
			}
			else
			{
				if (Enum.IsDefined(enumType, enumValue))
				{
					SetValueForDisplay(Enum.GetName(enumType, enumValue));
				}
				else
				{
					SetValueForDisplay(string.Format("{0} (not in '{1}')", value, enumType.Name), value.ToString());
				}
			}
		}

		static int MenuItemComparisonByText(MenuItem x, MenuItem y)
		{
			return string.Compare(x.Text, y.Text);
		}

		#endregion

		#endregion

		void OnContextMenuPopup(object sender, EventArgs e)
		{
			if (ContextMenuItems.Count > 0)
			{
				return;
			}

			if (!string.IsNullOrEmpty(ValueForDisplay) || clipboardCopyOverride != null)
			{
				ContextMenuItems.Add(
					new MenuItem(
						"&Copy",
						new[]
						{
							new MenuItem("Copy &Name", CopyNameToClipboard),
							new MenuItem("Copy &Value", CopyValueToClipboard),
							new MenuItem("Copy Name &and Value", CopyNameAndValueToClipboard)
						}));
			}
			else
			{
				ContextMenuItems.Add(
					new MenuItem(
						"&Copy",
						new[]
						{
							new MenuItem("Copy &Name", CopyNameToClipboard),
						}));
			}

			if (value != null)
			{
				var objectType = value.GetType();

				if ((objectType.IsValueType || objectType == typeof(SteamID)) && !objectType.IsEnum && objectType != typeof(bool))
				{
					ContextMenuItems.Add(new MenuItem("-"));

					ContextMenuItems.Add(new MenuItem("Display &Raw Value", (s, _) =>
					{
						SetAsRadioSelected(s);
						Initialize();
					}) { RadioCheck = true, Checked = true });

					if (objectType != typeof(SteamID))
					{
						// only allow displaying as an enum value if we're not a steamid

						var enumMenuItem = new MenuItem("Display as &Enum Value");

						var enumTypesByNamespace = typeof(CMClient).Assembly.ExportedTypes
							.Where(x => x.IsEnum)
							.GroupBy(x => x.Namespace)
							.OrderBy(x => x.Key)
							.ToArray();

						if (enumTypesByNamespace.Length > 0)
						{
							foreach (var enumTypes in enumTypesByNamespace)
							{
								var enumNamespaceMenuItem = new MenuItem(enumTypes.Key);
								enumMenuItem.MenuItems.Add(enumNamespaceMenuItem);

								var menuItems = new List<MenuItem>();

								foreach (var enumType in enumTypes)
								{
									var enumName = enumType.FullName.Substring(enumType.Namespace.Length + 1);
									var item = new MenuItem(enumName, (s, _) =>
									{
										SetAsRadioSelected(s);
										DisplayAsEnumMember(enumType);
									});
									menuItems.Add(item);
								}

								menuItems.Sort(MenuItemComparisonByText);
								enumNamespaceMenuItem.MenuItems.AddRange(menuItems.ToArray());
							}
							ContextMenuItems.Add(enumMenuItem);
						}
					}

					if (objectType == typeof(long) || objectType == typeof(ulong) || objectType == typeof(SteamID))
					{
						ContextMenuItems.Add(
							new MenuItem(
								"SteamID",
								new[]
								{
									new MenuItem("Steam2", DisplayAsSteam2ID) { RadioCheck = true },
									new MenuItem("Steam3", DisplayAsSteam3ID) { RadioCheck = true }
								}));

					}

					if (objectType == typeof(long) || objectType == typeof(ulong) || objectType == typeof(int) || objectType == typeof(uint))
					{
						ContextMenuItems.Add(new MenuItem("GlobalID (GID)", DisplayAsGlobalID) { RadioCheck = true });
						ContextMenuItems.Add(new MenuItem("Date/Time", DisplayAsPosixTimestamp) { RadioCheck = true });
					}

					if (objectType == typeof(int) || objectType == typeof(uint))
					{
						ContextMenuItems.Add(new MenuItem("IPv4 Address", DisplayAsIPAddress) { RadioCheck = true });
					}
				}

				if (objectType == typeof(byte[]))
				{
					ContextMenuItems.Add(new MenuItem("&Save to file...", SaveDataToFile));

					var data = (byte[])value;
					if (data.Length > 0 && data.Length <= MaxDataLengthForDisplay)
					{
						ContextMenuItems.Add(new MenuItem("&ASCII", DisplayDataAsAscii) { RadioCheck = true });
						ContextMenuItems.Add(new MenuItem("&UTF-8", DisplayDataAsUTF8) { RadioCheck = true });
						ContextMenuItems.Add(new MenuItem("&Hexadecimal", DisplayDataAsHexadecimal) { RadioCheck = true, Checked = true });
						ContextMenuItems.Add(new MenuItem("&Binary KeyValues (VDF)", DisplayDataAsBinaryKeyValues) { RadioCheck = true });
					}
				}
			}
		}

		void SetValueForDisplay(string valueForDisplay, string clipboardOverrideValue = null, TreeNode[] childNodes = null)
		{
			this.ValueForDisplay = valueForDisplay;
			this.clipboardCopyOverride = clipboardOverrideValue;

			TreeNode.Nodes.Clear();
			if (childNodes != null)
			{
				TreeNode.Nodes.AddRange(childNodes);
			}

			if (childNodes != null && childNodes.Length > 100)
			{
				TreeNode.Collapse(ignoreChildren: true);
			}
			else
			{
				TreeNode.Expand();
			}
		}

		static void SetAsRadioSelected(object sender)
		{
			var senderItem = sender as MenuItem;
			if (senderItem != null)
			{
				var contextMenu = senderItem;
				ContextMenu rootContextMenu;
				for (
					rootContextMenu = contextMenu.GetContextMenu();
					rootContextMenu.GetContextMenu() != rootContextMenu;
					rootContextMenu = rootContextMenu.GetContextMenu())
				{
				}

				RecursiveClearMenuChecked(rootContextMenu);
				senderItem.Checked = true;
			}
		}

		static void RecursiveClearMenuChecked(Menu menu)
		{
			foreach (MenuItem child in menu.MenuItems)
			{
				child.Checked = false;

				RecursiveClearMenuChecked(child);
			}
		}

		void UpdateDisplayText()
		{
			string textToDisplay;
			if (string.IsNullOrEmpty(ValueForDisplay))
			{
				textToDisplay = name;
			}
			else
			{
				textToDisplay = string.Format("{0}: {1}", name, ValueForDisplay);
			}

			TreeNode.Text = textToDisplay;
		}

		void Initialize()
		{
			if (value == null)
			{
				SetValueForDisplay("<null>");
				return;
			}

			var objectType = value.GetType();
			if (objectType.IsEnum)
			{
				SetValueForDisplay(string.Format("{0:G} ({0:D})", value));
			}
			else if (objectType.IsValueType)
			{
				SetValueForDisplay(value.ToString());
			}
			else if (value is string)
			{
				SetValueForDisplay(string.Format("\"{0}\"", value), (string)value);
			}
			else if (value is SteamID)
			{
				var steamID = (SteamID)value;
				SetValueForDisplay(string.Format("{0} ({1})", steamID.Render(steam3: true), steamID.ConvertToUInt64()));
			}
			else if (value is byte[])
			{
				var data = (byte[])value;
				if (data.Length == 0)
				{
					SetValueForDisplay("byte[ 0 ]");
				}
				else if (data.Length > MaxDataLengthForDisplay)
				{
					SetValueForDisplay(string.Format("byte[ {0} ]: Length exceeded {1} bytes! Value not shown - right-click to save.", data.Length, MaxDataLengthForDisplay));
				}
				else
				{
					var hexadecimalValue = data.Aggregate(new StringBuilder(), (str, val) => str.Append(val.ToString("X2"))).ToString();
					SetValueForDisplay(hexadecimalValue);
				}
			}
			else if (value is KeyValue)
			{
				var kv = (KeyValue)value;
				if (kv.Children.Count > 0)
				{
					var children = new List<TreeNode>();
					foreach (var child in kv.Children)
					{
						children.Add(new TreeNodeObjectExplorer(child.Name, child, configuration).TreeNode);
					}

					SetValueForDisplay(null, childNodes: children.ToArray());
				}
				else
				{
					SetValueForDisplay(string.Format("\"{0}\"", kv.Value), kv.Value);
				}
			}
			else if (objectType.IsDictionaryType())
			{
				var childNodes = new List<TreeNode>();

				var dictionary = value as IDictionary;
				foreach (DictionaryEntry entry in dictionary)
				{
					var childName = string.Format("[ {0} ]", entry.Key.ToString());
					var childObjectExplorer = new TreeNodeObjectExplorer(childName, entry.Value, configuration);
					childNodes.Add(childObjectExplorer.TreeNode);
				}

				SetValueForDisplay(null, childNodes: childNodes.ToArray());
			}
			else if (objectType.IsEnumerableType())
			{
				Type innerType = null;
				var index = 0;

				var childNodes = new List<TreeNode>();

				foreach (var childObject in value as IEnumerable)
				{
					if (innerType == null)
					{
						innerType = childObject.GetType();
					}

					var childName = string.Format("[ {0} ]", index);
					var childObjectExplorer = new TreeNodeObjectExplorer(childName, childObject, configuration);
					childNodes.Add(childObjectExplorer.TreeNode);

					index++;
				}

				SetValueForDisplay(string.Format("{0}[ {1} ]", innerType == null ? objectType.Name : innerType.Name, index), childNodes: childNodes.ToArray());
			}
			else
			{
				var childNodes = new List<TreeNode>();

				var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
				bool valueIsProtobufMsg = value is ProtoBuf.IExtensible;

				if (valueIsProtobufMsg)
				{
					// For proto msgs, we want to skip vars where name is "<blah>Specified", unless there's no var named "<blah>"
					properties = properties.Where(x => {
				        return !x.Name.EndsWith("Specified") || properties.FirstOrDefault(y => {
				            return y.Name == x.Name.Remove(x.Name.Length - 9);
				        }) == null;
				    }).ToList();
				}

				foreach (var property in properties)
				{
					var childName = property.Name;
					var childObject = property.GetValue(value, null);
					bool valueIsSet = true;
					if (valueIsProtobufMsg)
					{
						if (childObject is IList)
						{
							// Repeated fields are marshalled as Lists, but aren't "set"/sent if they have no values added.
							valueIsSet = (property.GetValue(value) as IList).Count != 0;
						}
						else
						{
							// For non-repeated fields, look for the "<blah>Specfied" field existing and being set to false;
							var propSpecified = value.GetType().GetProperty(property.Name + "Specified");
							valueIsSet = propSpecified == null || (bool)propSpecified.GetValue(value);
						}
					}
					
					if (valueIsSet || configuration.ShowUnsetFields)
					{
						var childConfiguration = configuration;
						childConfiguration.IsUnsetField = !valueIsSet;

					    var childObjectExplorer = new TreeNodeObjectExplorer(childName, childObject, childConfiguration);
					    childNodes.Add(childObjectExplorer.TreeNode);
					}
				}

				SetValueForDisplay(null, childNodes: childNodes.ToArray());
			}
		}
	}
}
