﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
	class TreeNodeObjectExplorer : TreeNode
	{
		public TreeNodeObjectExplorer(string name, object value, TreeNodeObjectExplorerConfiguration configuration)
		{
			this.name = name;
			this.value = value;
			this.configuration = configuration;

			if (configuration.IsUnsetField)
			{
				ForeColor = System.Drawing.Color.DarkGray;
			}

			Initialize();
		}

		public void CreateContextMenu()
		{
			if (this.ContextMenuStrip != null)
			{
				return;
			}

			this.ContextMenuStrip = new ContextMenuStrip();
			InitializeContextMenu();
		}

		readonly string name;
		readonly object value;
		TreeNodeObjectExplorerConfiguration configuration;
		string clipboardCopyOverride;

		ToolStripItemCollection ContextMenuItems
		{
			get { return this.ContextMenuStrip.Items; }
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

		void CopyTypeToClipboard(object sender, EventArgs e)
		{
			Clipboard.SetText(value.GetType().Name, TextDataFormat.Text);
		}

		void CopyValueToClipboard(object sender, EventArgs e)
		{
			var valueToCopy = clipboardCopyOverride ?? ValueForDisplay;
			Clipboard.SetText(valueToCopy, TextDataFormat.Text);
		}
		void CopyNameAndValueToClipboard(object sender, EventArgs e)
		{
			Clipboard.SetText(this.Text, TextDataFormat.Text);
		}

		#endregion

		#region Binary Data

		const int MaxDataLengthForDisplay = 400;

		void SaveDataToFile(object sender, EventArgs e)
		{
			var data = (byte[])value;

			using var dialog = new SaveFileDialog { DefaultExt = "bin", SupportMultiDottedExtensions = true };
			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				File.WriteAllBytes(dialog.FileName, data);
			}
		}

		void DisplayDataAsAscii(object sender, EventArgs e)
		{
			var data = (byte[])value;

			var result = new StringBuilder( data.Length + 32 );
			foreach ( byte b in data )
			{
				if ( b == 0 )
				{
					result.Append( "\\0" );
				}
				else if ( !char.IsAsciiLetterOrDigit( ( char )b ) )
				{
					result.Append( $"\\x{b:X2}" );
				}
				else
				{
					result.Append( ( char )b );
				}
			}

			SetValueForDisplay(result.ToString());
		}

		void DisplayDataAsUTF8(object sender, EventArgs e)
		{
			var data = (byte[])value;
			SetValueForDisplay(Encoding.UTF8.GetString(data).Replace("\0", "\\0", StringComparison.Ordinal));
		}

		void DisplayDataAsHexadecimal(object sender, EventArgs e)
		{
			var data = (byte[])value;
			var hexString = data.Aggregate(new StringBuilder(), (str, val) => str.Append(val.ToString("X2"))).ToString();
			SetValueForDisplay(hexString);

		}

		void DisplayDataAsBinaryKeyValues(object sender, EventArgs e)
		{

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
				SetValueForDisplay(null, childNodes: [new TreeNodeObjectExplorer(kv.Name, kv, configuration)]);
			}

			this.ExpandAll();
		}

		void DisplayDataAsBinaryKeyValuesLzma( object sender, EventArgs e )
		{
			var data = ( byte[] )value;
			var kv = new KeyValue();
			bool didRead;

			using ( var compressed = new MemoryStream( data ) )
			{
				if ( !LzmaUtil.TryDecompress( compressed, static capacity => new MemoryStream( capacity ), out var decompressed ) )
				{
					SetValueForDisplay( "Not a valid LZMA-encoded blob!" );
				}

				using ( decompressed )
				{ 
					didRead = kv.TryReadAsBinary( decompressed );
				}

				if ( !didRead )
				{
					SetValueForDisplay( "Not a valid KeyValues object!" );
				}
				else
				{
					SetValueForDisplay( null, childNodes: [ new TreeNodeObjectExplorer( kv.Name, kv, configuration ) ] );
				}
			}

			this.ExpandAll();
		}

		void DisplayDataAsProtobuf( object sender, EventArgs e )
		{
			var data = ( byte[] )value;

			try
			{
				using var ms = new MemoryStream( data );
				var dictionary = ProtoBufFieldReader.ReadProtobuf( ms );

				SetValueForDisplay( null, childNodes: [new TreeNodeObjectExplorer( "Protobuf", dictionary, configuration )] );
			}
			catch
			{
				SetValueForDisplay( "Not a valid Protobuf object!" );
			}

			this.ExpandAll();
		}

		#endregion

		#region JSON Web Token

		void DisplayDataAsJWT( object sender, EventArgs e )
		{
			var data = ( string )value;

			try
			{
				var handler = new JwtSecurityTokenHandler();
				var token = handler.ReadJwtToken( data );

				SetValueForDisplay( null, childNodes: [new TreeNodeObjectExplorer( "JSON Web Token", token, configuration )] );
			}
			catch
			{
				SetValueForDisplay( "Not a valid JWT object!" );
			}

			this.ExpandAll();
		}

		#endregion

		#region IP Address

		void DisplayAsIPAddress(object sender, EventArgs e)
		{
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
			var steamID = ConvertToSteamID(value);
			SetValueForDisplay(steamID.Render(steam3: false));
		}

		void DisplayAsSteam3ID(object sender, EventArgs e)
		{
			var steamID = ConvertToSteamID(value);
			SetValueForDisplay(steamID.Render(steam3: true));
		}

		static SteamID ConvertToSteamID(object value)
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
			var gid = new GlobalID((ulong)Convert.ChangeType(value, typeof(ulong)));
			var children = new[]
			{
				new TreeNodeObjectExplorer("Box", gid.BoxID, configuration),
				new TreeNodeObjectExplorer("Process ID", gid.ProcessID, configuration),
				new TreeNodeObjectExplorer("Sequential Count", gid.SequentialCount, configuration),
				new TreeNodeObjectExplorer("StartTime", gid.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), configuration)
			};

			SetValueForDisplay(null, childNodes: children);
		}

		#endregion

		#region Date/Time

		void DisplayAsPosixTimestamp(object sender, EventArgs e)
		{
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

		static int ToolStripMenuItemComparisonByText( ToolStripMenuItem x, ToolStripMenuItem y )
		{
			return string.Compare( x.Text, y.Text, StringComparison.Ordinal );
		}

		#endregion

		#endregion

		void InitializeContextMenu()
		{
			if (ContextMenuItems.Count > 0)
			{
				return;
			}

			if (!string.IsNullOrEmpty(ValueForDisplay) || clipboardCopyOverride != null)
			{
				ContextMenuItems.Add(
					new ToolStripMenuItem(
						"&Copy",
						null,
						[
							new ToolStripMenuItem("Copy &Name", null, CopyNameToClipboard),
							new ToolStripMenuItem("Copy &Value", null, CopyValueToClipboard),
							new ToolStripMenuItem("Copy Name &and Value", null, CopyNameAndValueToClipboard),
							new ToolStripMenuItem("Copy &Type", null, CopyTypeToClipboard),
						]));
			}
			else
			{
				ContextMenuItems.Add(
					new ToolStripMenuItem(
						"&Copy",
						null,
						[
							new ToolStripMenuItem("Copy &Name", null, CopyNameToClipboard),
							new ToolStripMenuItem("Copy &Type", null, CopyTypeToClipboard),
						]));
			}

			if (value != null)
			{
				var objectType = value.GetType();

				if ((objectType.IsValueType || objectType == typeof(SteamID)) && !objectType.IsEnum && objectType != typeof(bool))
				{
					ContextMenuItems.Add(new ToolStripSeparator());

					ContextMenuItems.Add(new ToolStripMenuItem( "Display &Raw Value", null, ( s, _ ) =>
					{
						Initialize();
					} )
					{
						Checked = true
					}
					.AsRadioCheck());

					if (objectType != typeof(SteamID))
					{
						// only allow displaying as an enum value if we're not a steamid

						var enumTypesByNamespace = typeof(CMClient).Assembly.ExportedTypes
							.Where(x => x.IsEnum)
							.GroupBy(x => x.Namespace)
							.OrderBy(x => x.Key)
							.ToArray();

						if (enumTypesByNamespace.Length > 0)
						{
							var enumMenuItem = new ToolStripMenuItem( "Display as &Enum Value" );

							foreach (var enumTypes in enumTypesByNamespace)
							{
								var enumNamespaceMenuItem = new ToolStripMenuItem( enumTypes.Key);
								enumMenuItem.DropDownItems.Add(enumNamespaceMenuItem);

								var menuItems = new List<ToolStripMenuItem>();

								foreach (var enumType in enumTypes)
								{
									var enumName = enumType.FullName[ ( enumType.Namespace.Length + 1 ).. ];
									var item = new ToolStripMenuItem( enumName, null, (s, _) =>
									{
										DisplayAsEnumMember(enumType);
									}).AsRadioCheck();
									menuItems.Add(item);
								}

								menuItems.Sort(ToolStripMenuItemComparisonByText);
								enumNamespaceMenuItem.DropDownItems.AddRange(menuItems.ToArray());
							}
							ContextMenuItems.Add(enumMenuItem);
						}
					}

					if (objectType == typeof(long) || objectType == typeof(ulong) || objectType == typeof(SteamID))
					{
						ContextMenuItems.Add(
							new ToolStripMenuItem(
								"SteamID",
								null,
								[
									new ToolStripMenuItem("Steam2", null, DisplayAsSteam2ID).AsRadioCheck(),
									new ToolStripMenuItem("Steam3", null, DisplayAsSteam3ID).AsRadioCheck(),
								] ));

					}

					if (objectType == typeof(long) || objectType == typeof(ulong) || objectType == typeof(int) || objectType == typeof(uint))
					{
						ContextMenuItems.Add(new ToolStripMenuItem( "GlobalID (GID)", null, DisplayAsGlobalID).AsRadioCheck() );
						ContextMenuItems.Add(new ToolStripMenuItem( "Date/Time", null, DisplayAsPosixTimestamp).AsRadioCheck());
					}

					if (objectType == typeof(int) || objectType == typeof(uint))
					{
						ContextMenuItems.Add(new ToolStripMenuItem( "IPv4 Address", null, DisplayAsIPAddress).AsRadioCheck());
					}
				}

				if (objectType == typeof(byte[]))
				{
					ContextMenuItems.Add(new ToolStripMenuItem( "&Save to file...", null, SaveDataToFile));

					var data = (byte[])value;
					if ( data.Length > 0 )
					{

						ContextMenuItems.Add( new ToolStripMenuItem( "&Binary KeyValues (VDF)", null, DisplayDataAsBinaryKeyValues ).AsRadioCheck() );

						if ( LzmaUtil.HasLzmaHeader( data ) )
						{
							ContextMenuItems.Add( new ToolStripMenuItem( "&Binary KeyValues (VDF) [LZMA-encoded]", null, DisplayDataAsBinaryKeyValuesLzma ).AsRadioCheck() );
						}

						ContextMenuItems.Add( new ToolStripMenuItem( "&Protobuf", null, DisplayDataAsProtobuf ).AsRadioCheck() );
						ContextMenuItems.Add( new ToolStripMenuItem( "&ASCII", null, DisplayDataAsAscii ).AsRadioCheck() );
						ContextMenuItems.Add( new ToolStripMenuItem( "&UTF-8", null, DisplayDataAsUTF8 ).AsRadioCheck() );
						ContextMenuItems.Add( new ToolStripMenuItem( "&Hexadecimal", null, DisplayDataAsHexadecimal ) { Checked = true }.AsRadioCheck() );
					}
				}

				if ( objectType == typeof( string ) )
				{
					ContextMenuItems.Add( new ToolStripMenuItem( "Display &Raw Value", null, ( s, _ ) =>
					{
						Initialize();
					} )
					{
						Checked = true
					}
					.AsRadioCheck() );
					ContextMenuItems.Add( new ToolStripMenuItem( "&JSON Web Token", null, DisplayDataAsJWT ).AsRadioCheck() );
				}
			}
		}

		void SetValueForDisplay(string valueForDisplay, string clipboardOverrideValue = null, TreeNode[] childNodes = null)
		{
			this.ValueForDisplay = valueForDisplay;
			this.clipboardCopyOverride = clipboardOverrideValue;

			this.Nodes.Clear();
			if (childNodes != null)
			{
				this.Nodes.AddRange(childNodes);
			}

			if (childNodes != null && childNodes.Length > 100)
			{
				this.Collapse(ignoreChildren: true);
			}
			else
			{
				this.Expand();
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

			this.Text = textToDisplay;
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
			else if (value is string stringValue  )
			{
				SetValueForDisplay(string.Format("\"{0}\"", value), stringValue );
			}
			else if (value is SteamID steamID)
			{
				SetValueForDisplay(string.Format("{0} ({1})", steamID.Render(steam3: true), steamID.ConvertToUInt64()));
			}
			else if (value is byte[] data)
			{
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
			else if (value is KeyValue kv)
			{
				if (kv.Children.Count > 0)
				{
					var children = new List<TreeNode>();
					foreach (var child in kv.Children)
					{
						children.Add(new TreeNodeObjectExplorer(child.Name, child, configuration));
					}

					SetValueForDisplay(null, childNodes: children.ToArray());
				}
				else
				{
					SetValueForDisplay(string.Format("\"{0}\"", kv.Value), kv.Value);
				}
			}
			else if ( value is CMsgIPAddress msgIpAddr )
			{
				if ( msgIpAddr.ShouldSerializev4() )
				{
					byte[] addrBytes = BitConverter.GetBytes( msgIpAddr.v4 );
					Array.Reverse( addrBytes );
					SetValueForDisplay( new IPAddress( addrBytes ).ToString() );
				}
				else if ( msgIpAddr.ShouldSerializev6() )
				{
					SetValueForDisplay( new IPAddress( msgIpAddr.v6 ).ToString() );
				}
				else
				{
					SetValueForDisplay( "<null>" );
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
					childNodes.Add(childObjectExplorer);
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
					childNodes.Add(childObjectExplorer);

					index++;
				}

				SetValueForDisplay(string.Format("{0}[ {1} ]", innerType == null ? objectType.Name : innerType.Name, index), childNodes: childNodes.ToArray());
			}
			else
			{
				var childNodes = new List<TreeNode>();

				var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
				bool valueIsProtobufMsg = value is ProtoBuf.IExtensible;

				foreach (var property in properties)
				{
					var childName = property.Name;
					var childObject = property.GetValue(value, null);
					bool valueIsSet = true;
					if (valueIsProtobufMsg)
					{
						if (childObject is IList childObjectList)
						{
							// Repeated fields are marshalled as Lists, but aren't "set"/sent if they have no values added.
							valueIsSet = childObjectList.Count != 0;
						}
						else
						{
							// For non-repeated fields, look for the "ShouldSerialiez<blah>" method existing and being set to false;
							var shouldSerializeProp = value.GetType().GetMethod("ShouldSerialize" + property.Name);
							valueIsSet = shouldSerializeProp == null || (shouldSerializeProp.Invoke(value, null) is bool specified && specified);
						}
					}
					
					if (valueIsSet || configuration.ShowUnsetFields)
					{
						var childConfiguration = configuration;
						childConfiguration.IsUnsetField = !valueIsSet;

						var childObjectExplorer = new TreeNodeObjectExplorer(childName, childObject, childConfiguration);
						childNodes.Add(childObjectExplorer);
					}
				}

				SetValueForDisplay(null, childNodes: childNodes.ToArray());
			}
		}
	}
}
