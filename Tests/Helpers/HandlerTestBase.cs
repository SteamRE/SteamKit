using SteamKit2;

namespace Tests
{
	public class HandlerTestBase<THandler>
		where THandler : ClientMsgHandler
	{
		public HandlerTestBase()
		{
			client = new SteamClient();
			handler = client.GetHandler<THandler>();
			
			callbackMgr = new CallbackManager( client );
		}

		readonly SteamClient client;
		readonly THandler handler;
		readonly CallbackManager callbackMgr;

		protected SteamClient SteamClient
		{
			get { return client; }
		}

		protected THandler Handler
		{
			get { return handler; }
		}

		protected CallbackManager CallbackManager
		{
			get { return callbackMgr; }
		}
	}
}
