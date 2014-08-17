using System;
using System.Collections;
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
		public class NodeInfo
		{
			public bool ShouldExpandByDefault {get;set;}
			public string ValueToCopy {get;set;}
		}

		#region Top-Level Node-Building

		static TreeNode BuildInfoNode(uint rawEMsg)
		{
			var eMsg = MsgUtil.GetMsg(rawEMsg);

			return new TreeNode("Info", new[] 
			{
				CreateNode("EMsg", string.Format("{0} ({1})", eMsg.ToString(), (long)eMsg)),
				CreateNode("Is ProtoBuf", MsgUtil.IsProtoBuf(rawEMsg).ToString()),
			});
		}

		static TreeNode BuildHeaderNode(ISteamSerializableHeader header)
		{
			var node = new TreeNode("Header");
			AddObjectValue(node, header);
			return node;
		}

		static TreeNode BuildBodyNode(object body)
		{
			var node = new TreeNode("Body");
			AddObjectValue(node, body);
			return node;
		}

		static TreeNode BuildPayloadNode(byte[] data)
		{
			var node = new TreeNode("Payload");
			AddObjectValue(node, data);
			return node;
		}

		static TreeNode BuildGCBodyNode(CMsgGCClient body)
		{
			var node = new TreeNode("Game Coordinator Message");
			var gcBody = body as CMsgGCClient;

			using (var ms = new MemoryStream(gcBody.payload))
			{
				var gc = new
				{
					emsg = EMsgExtensions.GetGCMessageName(gcBody.msgtype),
					header = ReadGameCoordinatorHeader(gcBody.msgtype, ms),
					body = ReadMessageBody(gcBody.msgtype, ms, gcBody.appid),
				};

				AddObjectValue(node, gc);
			}

			return node;
		}

		static TreeNode BuildServiceMethodBodyNode(CMsgClientServiceMethod body)
		{
			var node = new TreeNode("Service Method");

			var name = body.method_name;
			object innerBody;

			using (var ms = new MemoryStream(body.serialized_method))
			{
				innerBody = ReadServiceMethodBody(body.method_name, ms, x => x.GetParameters().First().ParameterType);
			}

			AddObjectValue(node, innerBody);

			return node;
		}

		static TreeNode BuildServiceMethodResponseBodyNode(CMsgClientServiceMethodResponse body)
		{
			var node = new TreeNode("Service Method Response");

			var name = body.method_name;
			object innerBody;

			using (var ms = new MemoryStream(body.serialized_method_response))
			{
				innerBody = ReadServiceMethodBody(body.method_name, ms, x => x.ReturnType);
			}

			AddObjectValue(node, innerBody);

			return node;
		}

		#endregion

		#region Lower-Level Node-Building

		const string NodeValuePrefix = ": ";

		static void AddObjectValue(TreeNode node, object obj)
		{
			if (obj == null)
			{
				SetNodeValueWithCopyMenu(node, "<null>");
				return;
			}

			var objectType = (obj != null ? obj.GetType() : null);
			if (obj is ulong)
			{
				AddUInt64Value(node, (ulong)obj);
				return;
			}
			else if (objectType != null && objectType.IsValueType)
			{
				AddValueObjectValue(node, obj);
				return;
			}
			else if (obj is string)
			{
				AddStringValue(node, (string)obj);
				return;
			}
			else if (obj is SteamID)
			{
				AddSteamIDValue(node, (SteamID)obj);
				return;
			}
			else if (obj is byte[])
			{
				AddByteArrayValue(node, (byte[])obj);
				return;
			}
			else if (objectType.IsDictionaryType())
			{
				var dictionary = obj as IDictionary;
				foreach (DictionaryEntry entry in dictionary)
				{
					var childNode = new TreeNode(string.Format("[ {0} ]", entry.Key.ToString()));
					node.Nodes.Add(childNode);

					AddObjectValue(childNode, entry.Value);
				}

				return;
			}
			else if (objectType.IsEnumerableType())
			{
				Type innerType = null;
				var index = 0;

				foreach (var subObj in obj as IEnumerable)
				{
					innerType = subObj.GetType();

					var childNode = new TreeNode();
					var name = string.Format("[ {0} ]", index);
					if (innerType.IsValueType)
					{
						SetNodeValue(childNode, name, innerType.Name);
					}
					else
					{
						childNode.Text = name;
					}

					node.Nodes.Add(childNode);

					AddObjectValue(childNode, subObj);

					index++;
				}

				node.Text += string.Format(
					"{0}{1}[ {2} ]",
					NodeValuePrefix,
					(innerType == null ? objectType.Name : innerType.Name),
					index
				);

				if (index >= 100)
				{
					SetNodeExpandByDefault(node, false);
				} 

				return;
			}

			foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var childObject = property.GetValue(obj, null);
				var childNode = new TreeNode(property.Name);
				node.Nodes.Add(childNode);

				AddObjectValue(childNode, childObject);
			}
		}

		static void AddUInt64Value(TreeNode node, ulong value)
		{
			var nodeName = node.Text;
			SetNodeValue(node, value.ToString());
			SetNodeValueToCopy(node, value.ToString());
			SetNodeContextMenuForValueCopy(node);

			node.ContextMenu.MenuItems.Add("-"); // Separator

			MenuItem initialMenuItem;

			node.ContextMenu.MenuItems.Add((initialMenuItem = new MenuItem("Unsigned 64-bit Integer", delegate(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);
				node.Nodes.Clear();

				var strUll = value.ToString();
				SetNodeValue(node, nodeName, strUll);

			}) { RadioCheck = true, Checked = true }));

			node.ContextMenu.MenuItems.Add(new MenuItem("SteamID (Steam2)", delegate(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);
				node.Nodes.Clear();

				var strSteamID2 = new SteamID(value).Render(steam3: false);
				SetNodeValue(node, nodeName, strSteamID2);

			}) { RadioCheck = true });

			node.ContextMenu.MenuItems.Add(new MenuItem("SteamID (Steam3)", delegate(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);
				node.Nodes.Clear();

				var strSteamID3 = new SteamID(value).Render(steam3: true);
				SetNodeValue(node, nodeName, strSteamID3);

			}) { RadioCheck = true });

			node.ContextMenu.MenuItems.Add(new MenuItem("GlobalID", delegate(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);
				node.Nodes.Clear();

				var gid = new GlobalID(value);
				node.Nodes.Add(CreateNode("BoxID", gid.BoxID.ToString()));
				node.Nodes.Add(CreateNode("ProcessID", gid.ProcessID.ToString()));
				node.Nodes.Add(CreateNode("StartTime", gid.StartTime.ToString("yyyy-MM-dd HH:mm:ss")));
				node.Nodes.Add(CreateNode("SequentialCount", gid.SequentialCount.ToString()));

				node.Text = nodeName;
				SetNodeValueToCopy(node, value.ToString());
				node.Expand();

			}) { RadioCheck = true });

				
			var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			node.ContextMenu.MenuItems.Add(new MenuItem("Date/Time", delegate(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);
				node.Nodes.Clear();

				string dateTimeValue;
				try
				{
					dateTimeValue = unixEpoch.AddSeconds((double)value).ToString("yyyy-MM-dd HH:mm:ss");
				}
				catch (ArgumentOutOfRangeException)
				{
					dateTimeValue = "Out of range!";
				}

				SetNodeValue(node, nodeName, dateTimeValue);

			}) { RadioCheck = true });
		}

		static void AddValueObjectValue(TreeNode node, object obj)
		{
			SetNodeValueWithCopyMenu(node, obj.ToString());
		}

		static void AddStringValue(TreeNode node, string stringValue)
		{
			SetNodeValue(node, string.Format("\"{0}\"", stringValue));

			if (!string.IsNullOrEmpty(stringValue))
			{
				SetNodeValueToCopy(node, stringValue);
				SetNodeContextMenuForValueCopy(node);
			}
		}

		static void AddSteamIDValue(TreeNode node, SteamID steamID)
		{
			SetNodeValueWithCopyMenu(node, string.Format("{0} ({1}) ", steamID.Render(steam3: true), steamID.ConvertToUInt64()));
		}

		static void AddByteArrayValue(TreeNode node, byte[] data)
		{
			var nodeName = node.Text;
			SetNodeValue(node, string.Format("byte[ {0} ]", data.Length));

			if (data.Length == 0)
			{
				return;
			}

			node.ContextMenu = new ContextMenu(new[]
			{
				new MenuItem( "Save to File...", delegate( object sender, EventArgs e )
				{
					var dialog = new SaveFileDialog { DefaultExt = "bin", SupportMultiDottedExtensions = true };
					var result = dialog.ShowDialog();
					if ( result == DialogResult.OK )
					{
						File.WriteAllBytes( dialog.FileName, data );
					}
				}),
			});

			const int MaxBinLength = 400;
			if (data.Length > MaxBinLength)
			{
				node.Nodes.Add(string.Format("Length exceeded {0} bytes! Value not shown - right-click to save.", MaxBinLength));
				return;
			}

			SetNodeContextMenuForValueCopy(node);

			node.ContextMenu.MenuItems.Add(new MenuItem("-")); // Separator

			MenuItem intialMenuItem;

			node.ContextMenu.MenuItems.Add(new MenuItem("ASCII", delegate(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);
				node.Nodes.Clear();

				var strAscii = Encoding.ASCII.GetString(data).Replace("\0", "\\0");
				SetNodeValue(node, nodeName, strAscii);

			}) { RadioCheck = true });

			node.ContextMenu.MenuItems.Add(new MenuItem("UTF-8", delegate(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);
				node.Nodes.Clear();

				var strUnicode = Encoding.UTF8.GetString(data).Replace("\0", "\\0");
				SetNodeValue(node, nodeName, strUnicode);

			}) { RadioCheck = true });

			node.ContextMenu.MenuItems.Add((intialMenuItem = new MenuItem("Hexadecimal", delegate(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);
				node.Nodes.Clear();

				var hexString = data.Aggregate(new StringBuilder(), (str, value) => str.Append(value.ToString("X2"))).ToString();
				SetNodeValue(node, nodeName, hexString);

			}) { RadioCheck = true, Checked = true }));

			node.ContextMenu.MenuItems.Add(new MenuItem("Binary KeyValues", delegate(object sender, EventArgs e)
			{
				SetAsRadioSelected(sender);
				node.Nodes.Clear();
				SetNodeValueToCopy(node, string.Empty);
				node.Text = nodeName;

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
					node.Nodes.Add("Not a valid KeyValues object!");
				}
				else
				{
					node.Nodes.Add(BuildKeyValuesNode(kv.Children[0]));
				}

				node.ExpandAll();

			}) { RadioCheck = true });

			intialMenuItem.PerformClick();

			return;
		}

		static TreeNode BuildKeyValuesNode(KeyValue kv)
		{
			var node = new TreeNode(kv.Name);
			if (kv.Children.Count > 0)
			{
				foreach (var child in kv.Children)
				{
					node.Nodes.Add(BuildKeyValuesNode(child));
				}
			}
			else
			{
				SetNodeValueWithCopyMenu(node, kv.Value);
			}

			return node;
		}

		static TreeNode CreateNode(string key, string value)
		{
			var node = new TreeNode(key);
			SetNodeValueWithCopyMenu(node, value);
			return node;
		}

		#endregion

		#region Helpers

		static void SetNodeValue(TreeNode node, string key, string value)
		{
			SetNodeValueToCopy(node, value);
			node.Text = string.Format("{0}{1}{2}", key, NodeValuePrefix, value);
		}

		static void SetNodeValue(TreeNode node, string value)
		{
			SetNodeValue(node, node.Text, value);
		}

		static void SetNodeValueWithCopyMenu(TreeNode node, string value)
		{
			SetNodeValue(node, value);
			SetNodeContextMenuForValueCopy(node);
		}

		static void SetNodeContextMenuForValueCopy(TreeNode node)
		{
			node.ContextMenu = node.ContextMenu ?? new ContextMenu();
			node.ContextMenu.MenuItems.Add(new MenuItem("&Copy", delegate(object sender, EventArgs e)
			{
				Clipboard.SetText(GetNodeValueToCopy(node));
			}));
		}

		static void SetAsRadioSelected(object sender)
		{
			var senderItem = (MenuItem)sender;
			foreach (MenuItem item in senderItem.Parent.MenuItems)
			{
				item.Checked = false;
			}
			senderItem.Checked = true;
		}

		static string GetNodeValueToCopy(TreeNode node)
		{
			var nodeInfo = node.Tag as NodeInfo;
			return nodeInfo != null ? nodeInfo.ValueToCopy : null;
		}

		static void SetNodeValueToCopy(TreeNode node, string value)
		{
			var nodeInfo = node.Tag as NodeInfo ?? new NodeInfo();
			nodeInfo.ValueToCopy = value;
			node.Tag = nodeInfo;
		}

		static void SetNodeExpandByDefault(TreeNode node, bool value)
		{
			var nodeInfo = node.Tag as NodeInfo ?? new NodeInfo();
			nodeInfo.ShouldExpandByDefault = value;
			node.Tag = nodeInfo;
		}

		#endregion
	}
}
