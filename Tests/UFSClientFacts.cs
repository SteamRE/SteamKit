using System.Linq;
using SteamKit2;
using Xunit;

namespace Tests
{
	public class UFSClientFacts
	{
		[Fact]
		public void LogOnPostsLoggedOnCallbackWhenNoConnection()
		{
			var client = new SteamClient();
			var ufsClient = new UFSClient( client );

			var logonJobID = ufsClient.Logon( Enumerable.Empty<uint>() );

			var callback = client.GetCallback(freeLast: true);
			Assert.NotNull( callback );
			Assert.IsType<UFSClient.LoggedOnCallback>( callback );

			var jc = (UFSClient.LoggedOnCallback)callback;
			Assert.Equal( logonJobID, jc.JobID );
			Assert.Equal( EResult.NoConnection, jc.Result );
		}
	}
}
