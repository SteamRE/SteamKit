using System.Collections.Generic;
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

		public void CreateTreeNode()
		{
			Node = new TreeNode();

			using (var stream = item.OpenStream())
			{
				var rawEMsg = PeekUInt(stream);

				Node.Nodes.Add(BuildInfoNode(rawEMsg));

				var header = ReadHeader(rawEMsg, stream);
				Node.Nodes.Add(new TreeNodeObjectExplorer("Header", header).TreeNode);

				var body = ReadBody(rawEMsg, stream, header);
				var bodyNode = new TreeNodeObjectExplorer("Body", body).TreeNode;
				Node.Nodes.Add(bodyNode);

				var payload = ReadPayload(stream);
				if (payload != null && payload.Length > 0)
				{
					Node.Nodes.Add(new TreeNodeObjectExplorer("Payload", payload).TreeNode);
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
						Node.Nodes.AddRange(extraNodes);

						// Let the specializers examine any new message objects.
						objectsToSpecialize = specializations.Select(x => x.Value).ToArray();
					}
				}
			}
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
