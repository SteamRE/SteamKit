﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SteamKit2.Internal;
using System.Linq;
using System;
using ProtoBuf.Meta;
using SteamKit2;

namespace NetHookAnalyzer2
{
	partial class NetHookItemTreeBuilder
	{
		public NetHookItemTreeBuilder(NetHookItem item)
		{
			this.item = item;
		}

		readonly NetHookItem item;


		public ISpecialization[] Specializations
		{
			get;
			set;
		}

		TreeNode Node { get; set; }

		public TreeNode BuildTree()
		{
			if (Node != null)
			{
				return Node;
			}

			CreateTreeNode();
			return Node;
		}

		void CreateTreeNode()
		{
			try
			{
				Node = CreateTreeNodeCore();
			}
			catch (Exception ex)
			{
				Node = new TreeNode(null, new[] { new TreeNode(string.Format("{0} encountered whilst parsing item: {1}", ex.GetType().Name, ex.Message)) });
			}
		}

		TreeNode CreateTreeNodeCore()
		{
			var node = new TreeNode();

			using (var stream = item.OpenStream())
			{
				var rawEMsg = PeekUInt(stream);

				node.Nodes.Add(BuildInfoNode(rawEMsg));

				var header = ReadHeader(rawEMsg, stream);
				node.Nodes.Add(new TreeNodeObjectExplorer("Header", header).TreeNode);

				var body = ReadBody(rawEMsg, stream, header);
				var bodyNode = new TreeNodeObjectExplorer("Body", body).TreeNode;
				node.Nodes.Add(bodyNode);

				var payload = ReadPayload(stream);
				if (payload != null && payload.Length > 0)
				{
					node.Nodes.Add(new TreeNodeObjectExplorer("Payload", payload).TreeNode);
				}

				if (Specializations != null)
				{
					var objectsToSpecialize = new[] { body };
					while (objectsToSpecialize.Any())
					{
						var specializations = objectsToSpecialize.SelectMany(o => Specializations.SelectMany(x => x.ReadExtraObjects(o)));

						if (!specializations.Any())
						{
							break;
						}

						bodyNode.Collapse(ignoreChildren: true);

						var extraNodes = specializations.Select(x => new TreeNodeObjectExplorer(x.Key, x.Value).TreeNode).ToArray();
						node.Nodes.AddRange(extraNodes);

						// Let the specializers examine any new message objects.
						objectsToSpecialize = specializations.Select(x => x.Value).ToArray();
					}
				}
			}

			return node;
		}

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
	}
}
