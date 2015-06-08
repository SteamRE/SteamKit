using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using SteamKit2;
using SteamKit2.Internal;

namespace NetHookAnalyzer2
{
	partial class NetHookItemTreeBuilder
	{
		static TreeNode BuildInfoNode(uint rawEMsg)
		{
			var eMsg = MsgUtil.GetMsg(rawEMsg);

			var eMsgExplorer = new TreeNodeObjectExplorer("EMsg", eMsg);
			
			return new TreeNode("Info", new[] 
			{
				eMsgExplorer.TreeNode,
				new TreeNodeObjectExplorer("Is Protobuf", MsgUtil.IsProtoBuf(rawEMsg)).TreeNode
			});
		}

		static TreeNode BuildHeaderNode(ISteamSerializableHeader header)
		{
			return new TreeNodeObjectExplorer("Header", header).TreeNode;
		}

		static TreeNode BuildBodyNode(object body)
		{
			return new TreeNodeObjectExplorer("Body", body).TreeNode;
		}

		static TreeNode BuildPayloadNode(byte[] data)
		{
			return new TreeNodeObjectExplorer("Payload", data).TreeNode;
		}

		static TreeNode BuildGCBodyNode(CMsgGCClient body)
		{
			using (var ms = new MemoryStream(body.payload))
			{
				var gc = new
				{
					emsg = EMsgExtensions.GetGCMessageName(body.msgtype),
					header = ReadGameCoordinatorHeader(body.msgtype, ms),
					body = ReadMessageBody(body.msgtype, ms, body.appid),
				};

				return new TreeNodeObjectExplorer("Game Coordinator Message", gc).TreeNode;
			}
		}

		static TreeNode BuildServiceMethodBodyNode(CMsgClientServiceMethod body)
		{
			var name = body.method_name;
			object innerBody;

			using (var ms = new MemoryStream(body.serialized_method))
			{
				innerBody = ReadServiceMethodBody(body.method_name, ms, x => x.GetParameters().First().ParameterType);
			}

			return new TreeNodeObjectExplorer("Service Method", innerBody).TreeNode;
		}

		static TreeNode BuildServiceMethodResponseBodyNode(CMsgClientServiceMethodResponse body)
		{
			var name = body.method_name;
			object innerBody;

			using (var ms = new MemoryStream(body.serialized_method_response))
			{
				innerBody = ReadServiceMethodBody(body.method_name, ms, x => x.ReturnType);
			}

			return new TreeNodeObjectExplorer("Service Method Response", innerBody).TreeNode;
		}

		class TreeNodeObjectExplorer
		{
			public TreeNodeObjectExplorer(string name, object value)
			{
				this.name = name;
				this.value = value;
				this.treeNode = new TreeNode();
				this.treeNode.ContextMenu = new ContextMenu();
				this.treeNode.ContextMenu.Popup += OnContextMenuPopup;

				Initialize();
			}

			readonly string name;
			readonly object value;
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
					try
					{
						didRead = kv.ReadAsBinary(ms);
					}
					catch (InvalidDataException)
					{
						didRead = false;
					}
				}

				if (!didRead)
				{
					SetValueForDisplay("Not a valid KeyValues object!");
				}
				else
				{
					var firstChild = kv.Children[0]; // Due to bug in KeyValues parser.
					SetValueForDisplay(null, childNodes: new[] { new TreeNodeObjectExplorer(firstChild.Name, firstChild).TreeNode });
				}

				TreeNode.ExpandAll();
			}

			#endregion

			#region Steam ID

			void DisplayAsSteam2ID(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);

				var steamID = new SteamID((ulong)value);
				SetValueForDisplay(steamID.Render(steam3: false));
			}

			void DisplayAsSteam3ID(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);

				var steamID = new SteamID((ulong)value);
				SetValueForDisplay(steamID.Render(steam3: true));
			}

			#endregion

			#region GlobalID

			void DisplayAsGlobalID(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);

				var gid = new GlobalID((ulong)value);
				var children = new[]
			{
				new TreeNodeObjectExplorer("Box", gid.BoxID).TreeNode,
				new TreeNodeObjectExplorer("Process ID", gid.ProcessID).TreeNode,
				new TreeNodeObjectExplorer("Sequential Count", gid.SequentialCount).TreeNode,
				new TreeNodeObjectExplorer("StartTime", gid.StartTime.ToString("yyyy-MM-dd HH:mm:ss")).TreeNode
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
					var dateTimeValue = unixEpoch.AddSeconds((double)value).ToString("yyyy-MM-dd HH:mm:ss");
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

				if (Enum.IsDefined(enumType, enumValue))
				{
					SetValueForDisplay(Enum.GetName(enumType, enumValue));
				}
				else
				{
					SetValueForDisplay(string.Format("{0} (not in '{1}')", value, enumType.Name), value.ToString());
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

					if (objectType.IsValueType && !objectType.IsEnum && objectType != typeof(bool))
					{
						ContextMenuItems.Add(new MenuItem("-"));

						ContextMenuItems.Add(new MenuItem("Display &Raw Value", (s, _) =>
						{
							SetAsRadioSelected(s);
							Initialize();
						}) { RadioCheck = true, Checked = true });

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

						if (objectType == typeof(long) || objectType == typeof(ulong))
						{
							ContextMenuItems.Add(
								new MenuItem(
									"SteamID",
									new[]
								{
									new MenuItem("Steam2", DisplayAsSteam2ID) { RadioCheck = true },
									new MenuItem("Steam3", DisplayAsSteam3ID) { RadioCheck = true }
								}));

							ContextMenuItems.Add(new MenuItem("GlobalID (GID)", DisplayAsGlobalID) { RadioCheck = true });
							ContextMenuItems.Add(new MenuItem("Date/Time", DisplayAsPosixTimestamp) { RadioCheck = true });
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
					SetValueForDisplay(string.Format("{0} ({1}", steamID.Render(steam3: true), steamID.ConvertToUInt64()));
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
							children.Add(new TreeNodeObjectExplorer(child.Name, child).TreeNode);
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
						var childObjectExplorer = new TreeNodeObjectExplorer(childName, entry.Value);
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
						var childObjectExplorer = new TreeNodeObjectExplorer(childName, childObject);
						childNodes.Add(childObjectExplorer.TreeNode);

						index++;
					}

					SetValueForDisplay(string.Format("{0}[ {1} ]", innerType == null ? objectType.Name : innerType.Name, index), childNodes: childNodes.ToArray());
				}
				else
				{
					var childNodes = new List<TreeNode>();

					foreach (var property in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
					{
						var childName = property.Name;
						var childObject = property.GetValue(value, null);

						var childObjectExplorer = new TreeNodeObjectExplorer(childName, childObject);
						childNodes.Add(childObjectExplorer.TreeNode);
					}

					SetValueForDisplay(null, childNodes: childNodes.ToArray());
				}
			}
		}
	}
}
