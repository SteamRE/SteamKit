using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
	public sealed partial class SteamScreenshots
	{
		/// <summary>
		/// This callback is fired when this client receives a trade proposal.
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

			internal ScreenshotAddedCallback( JobID jobID, CMsgClientUCMAddScreenshotResponse msg )
			{
				JobID = jobID;

				Result = ( EResult )msg.eresult;
				ScreenshotID = msg.screenshotid;
			}
		}
	}
}
