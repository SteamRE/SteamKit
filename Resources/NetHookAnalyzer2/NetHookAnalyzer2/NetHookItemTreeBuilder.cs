using System.Windows.Forms;
using SteamKit2.Internal;

namespace NetHookAnalyzer2
{
	partial class NetHookItemTreeBuilder
	{
		public NetHookItemTreeBuilder(NetHookItem item)
		{
			this.item = item;
		}

		readonly NetHookItem item;

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
				Node.Nodes.Add(BuildHeaderNode(header));

				var body = ReadBody(rawEMsg, stream, header);
				var bodyNode = BuildBodyNode(body);
				Node.Nodes.Add(bodyNode);

				var payload = ReadPayload(stream);
				if (payload != null && payload.Length > 0)
				{
					Node.Nodes.Add(BuildPayloadNode(payload));
				}

				var nodeCount = Node.Nodes.Count;
				BuildSpecializations(body);

				var hasSpecializations = Node.Nodes.Count > nodeCount;
				if (hasSpecializations)
				{
					bodyNode.Collapse(ignoreChildren: true);
				}
			}
		}

		void BuildSpecializations(object body)
		{
			var gcBody = body as CMsgGCClient;
			if (gcBody != null)
			{
				Node.Nodes.Add(BuildGCBodyNode(gcBody));
			}

			var serviceMethodBody = body as CMsgClientServiceMethod;
			if (serviceMethodBody != null)
			{
				Node.Nodes.Add(BuildServiceMethodBodyNode(serviceMethodBody));
			}

			var serviceMethodResponseBody = body as CMsgClientServiceMethodResponse;
			if (serviceMethodResponseBody != null)
			{
				Node.Nodes.Add(BuildServiceMethodResponseBodyNode(serviceMethodResponseBody));
			}
		}
	}
}
