using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
    public class SteamClientFacts
	{
		[TestMethod]
		public void ConstructorSetsInitialHandlers()
		{
			var steamClient = new SteamClient();
			Assert.IsNotNull(steamClient.GetHandler<SteamUser>());
			Assert.IsNotNull(steamClient.GetHandler<SteamFriends>());
			Assert.IsNotNull(steamClient.GetHandler<SteamApps>());
			Assert.IsNotNull(steamClient.GetHandler<SteamGameCoordinator>());
			Assert.IsNotNull(steamClient.GetHandler<SteamGameServer>());
			Assert.IsNotNull(steamClient.GetHandler<SteamUserStats>());
			Assert.IsNotNull(steamClient.GetHandler<SteamMasterServer>());
			Assert.IsNotNull(steamClient.GetHandler<SteamCloud>());
			Assert.IsNotNull(steamClient.GetHandler<SteamWorkshop>());
			Assert.IsNotNull(steamClient.GetHandler<SteamTrading>());
			Assert.IsNotNull(steamClient.GetHandler<SteamUnifiedMessages>());
			Assert.IsNotNull(steamClient.GetHandler<SteamScreenshots>());
		}

		[TestMethod]
		public void AddHandlerAddsHandler()
		{
			var steamClient = new SteamClient();
			var handler = new TestMsgHandler();
			Assert.IsNull(steamClient.GetHandler<TestMsgHandler>());

			steamClient.AddHandler(handler);
			Assert.AreEqual(handler, steamClient.GetHandler<TestMsgHandler>());
		}

		[TestMethod]
		public void RemoveHandlerRemovesHandler()
		{
			var steamClient = new SteamClient();
			steamClient.AddHandler(new TestMsgHandler());
			Assert.IsNotNull(steamClient.GetHandler<TestMsgHandler>());

			steamClient.RemoveHandler(typeof(TestMsgHandler));
			Assert.IsNull(steamClient.GetHandler<TestMsgHandler>());
		}

		[TestMethod]
		public void GetNextJobIDIsThreadsafe()
		{
			var steamClient = new SteamClient();
			var jobID = steamClient.GetNextJobID();

			Assert.AreEqual(1u, jobID.SequentialCount);

			Parallel.For(0, 1000, x =>
			{
				steamClient.GetNextJobID();
			});

			jobID = steamClient.GetNextJobID();
			Assert.AreEqual(1002u, jobID.SequentialCount);
		}

		[TestMethod]
		public void GetNextJobIDSetsProcessIDToZero()
		{
			var steamClient = new SteamClient();
			var jobID = steamClient.GetNextJobID();

			Assert.AreEqual(0u, jobID.ProcessID);
		}

		[TestMethod]
		public void GetNextJobIDFillsProcessStartTime()
		{
			var steamClient = new SteamClient();
			var jobID = steamClient.GetNextJobID();

			using (var process = Process.GetCurrentProcess())
			{
				var processStartTime = process.StartTime;

				// Recreate the datetime to get rid of milliseconds etc. and only keep the important bits
				var expectedProcessStartTime = new DateTime(processStartTime.Year, processStartTime.Month, processStartTime.Day, processStartTime.Hour, processStartTime.Minute, processStartTime.Second);

				Assert.AreEqual(expectedProcessStartTime, jobID.StartTime);
			}
		}

		[TestMethod]
		public void GetNextJobIDSetsBoxIDToZero()
		{
			var steamClient = new SteamClient();
			var jobID = steamClient.GetNextJobID();

			Assert.AreEqual(0u, jobID.BoxID);
		}

		class TestMsgHandler : ClientMsgHandler
		{
			public override void HandleMsg(IPacketMsg packetMsg)
			{
			}
		}
	}
}
