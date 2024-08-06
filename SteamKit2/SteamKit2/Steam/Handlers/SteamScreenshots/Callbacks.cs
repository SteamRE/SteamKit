using SteamKit2.Internal;

namespace SteamKit2
{
	public sealed partial class SteamScreenshots
	{
		/// <summary>
		/// This callback is fired when this client receives a new screenshot.
		/// </summary>
		public sealed class ScreenshotAddedCallback : CallbackMsg
		{
			/// <summary>
			/// Gets the result.
			/// </summary>
			public EResult Result { get; private set; }

			/// <summary>
			/// Gets the screenshot ID of the newly added screenshot.
			/// </summary>
			public UGCHandle ScreenshotID { get; private set; }

			internal ScreenshotAddedCallback( IPacketMsg packetMsg )
			{
                var resp = new ClientMsgProtobuf<CMsgClientUCMAddScreenshotResponse>( packetMsg );
                var msg = resp.Body;
                
                JobID = resp.TargetJobID;

				Result = ( EResult )msg.eresult;
				ScreenshotID = msg.screenshotid;
			}
		}
	}
}
