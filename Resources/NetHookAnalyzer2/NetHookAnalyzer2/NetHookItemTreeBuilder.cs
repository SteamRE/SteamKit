using System.Windows.Forms;
using System.Linq;
using System;
using SteamKit2;

namespace NetHookAnalyzer2
{
	class NetHookItemTreeBuilder
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

		public TreeNode BuildTree(bool displayUnsetFields)
		{
			if (Node != null)
			{
				return Node;
			}

			CreateTreeNode(displayUnsetFields);
			return Node;
		}

		void CreateTreeNode(bool displayUnsetFields)
		{
			try
			{
				Node = CreateTreeNodeCore(displayUnsetFields);
			}
			catch (Exception ex)
			{
				Node = new TreeNode($"{ex.GetType().Name} encountered whilst parsing item: {ex.Message}");
			}
		}

		TreeNode CreateTreeNodeCore(bool displayUnsetFields)
		{
			var configuration = new TreeNodeObjectExplorerConfiguration { ShowUnsetFields = displayUnsetFields };

			var (rawEMsg, header, body, payload) = item.ReadFile();

			var node = BuildInfoNode(rawEMsg);
			node.Expand();

			node.Nodes.Add(new TreeNodeObjectExplorer("Header", header, configuration).TreeNode);

			var bodyNode = new TreeNodeObjectExplorer("Body", body, configuration).TreeNode;
			node.Nodes.Add(bodyNode);

			if (payload != null && payload.Length > 0)
			{
				node.Nodes.Add(new TreeNodeObjectExplorer("Payload", payload, configuration).TreeNode);
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

					var extraNodes = specializations.Select(x => new TreeNodeObjectExplorer(x.Key, x.Value, configuration).TreeNode).ToArray();
					node.Nodes.AddRange(extraNodes);

					// Let the specializers examine any new message objects.
					objectsToSpecialize = specializations.Select(x => x.Value).ToArray();
				}
			}

			return node;
		}

		static TreeNode BuildInfoNode(uint rawEMsg)
		{
			var eMsg = MsgUtil.GetMsg( rawEMsg );
			var eMsgName = $"EMsg {eMsg:G} ({eMsg:D})";

			if( MsgUtil.IsProtoBuf( rawEMsg ) )
			{
				return new TreeNode( eMsgName );
			}

			return new TreeNode( $"{eMsgName} (Non-Protobuf)" );
		}
	}
}
