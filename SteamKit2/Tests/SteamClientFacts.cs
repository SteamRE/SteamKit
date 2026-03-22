using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class SteamClientFacts
    {
        [Fact]
        public void ConstructorSetsInitialHandlers()
        {
            var steamClient = new SteamClient();
            Assert.NotNull(steamClient.GetHandler<SteamUser>());
            Assert.NotNull(steamClient.GetHandler<SteamFriends>());
            Assert.NotNull(steamClient.GetHandler<SteamApps>());
            Assert.NotNull(steamClient.GetHandler<SteamGameCoordinator>());
            Assert.NotNull(steamClient.GetHandler<SteamGameServer>());
            Assert.NotNull(steamClient.GetHandler<SteamUserStats>());
            Assert.NotNull(steamClient.GetHandler<SteamMasterServer>());
            Assert.NotNull(steamClient.GetHandler<SteamCloud>());
            Assert.NotNull(steamClient.GetHandler<SteamWorkshop>());
            Assert.NotNull(steamClient.GetHandler<SteamUnifiedMessages>());
            Assert.NotNull(steamClient.GetHandler<SteamScreenshots>());
            Assert.NotNull(steamClient.GetHandler<SteamMatchmaking>());
            Assert.NotNull(steamClient.GetHandler<SteamNetworking>());
            Assert.NotNull(steamClient.GetHandler<SteamContent>());
            Assert.NotNull(steamClient.GetHandler<SteamAuthTicket>());
        }

        [Fact]
        public void AddHandlerAddsHandler()
        {
            var steamClient = new SteamClient();
            var handler = new TestMsgHandler();
            Assert.Null(steamClient.GetHandler<TestMsgHandler>());

            steamClient.AddHandler(handler);
            Assert.Equal(handler, steamClient.GetHandler<TestMsgHandler>());
        }

        [Fact]
        public void RemoveHandlerRemovesHandler()
        {
            var steamClient = new SteamClient();
            steamClient.AddHandler(new TestMsgHandler());
            Assert.NotNull(steamClient.GetHandler<TestMsgHandler>());

            steamClient.RemoveHandler(typeof(TestMsgHandler));
            Assert.Null(steamClient.GetHandler<TestMsgHandler>());
        }

        [Fact]
        public void RemoveHandlerRemovesHandlerByInstance()
        {
            var steamClient = new SteamClient();
            var handler = new TestMsgHandler();
            steamClient.AddHandler(handler);
            Assert.NotNull(steamClient.GetHandler<TestMsgHandler>());

            steamClient.RemoveHandler(handler);
            Assert.Null(steamClient.GetHandler<TestMsgHandler>());
        }

        [Fact]
        public void AddHandlerThrowsOnDuplicateHandler()
        {
            var steamClient = new SteamClient();
            steamClient.AddHandler(new TestMsgHandler());

            Assert.Throws<InvalidOperationException>(() => steamClient.AddHandler(new TestMsgHandler()));
        }

        [Fact]
        public void RemoveHandlerByTypeDoesNothingWhenNotRegistered()
        {
            var steamClient = new SteamClient();
            Assert.Null(steamClient.GetHandler<TestMsgHandler>());

            steamClient.RemoveHandler(typeof(TestMsgHandler));
            Assert.Null(steamClient.GetHandler<TestMsgHandler>());
        }

        [Fact]
        public void GetRequiredHandlerReturnsHandler()
        {
            var steamClient = new SteamClient();
            var handler = steamClient.GetRequiredHandler<SteamUser>();

            Assert.NotNull(handler);
            Assert.IsType<SteamUser>(handler);
        }

        [Fact]
        public void GetRequiredHandlerThrowsWhenNotRegistered()
        {
            var steamClient = new SteamClient();

            Assert.Throws<InvalidOperationException>(() => steamClient.GetRequiredHandler<TestMsgHandler>());
        }

        [Fact]
        public void GetNextJobIDIsThreadsafe()
        {
            var steamClient = new SteamClient();
            var jobID = steamClient.GetNextJobID();

            Assert.Equal(1u, jobID.SequentialCount);

            Parallel.For(0, 1000, x =>
            {
                steamClient.GetNextJobID();
            });

            jobID = steamClient.GetNextJobID();
            Assert.Equal(1002u, jobID.SequentialCount);
        }

        [Fact]
        public void GetNextJobIDSetsProcessIDToZero()
        {
            var steamClient = new SteamClient();
            var jobID = steamClient.GetNextJobID();

            Assert.Equal(0u, jobID.ProcessID);
        }

        [Fact]
        public void GetNextJobIDFillsProcessStartTime()
        {
            var steamClient = new SteamClient();
            var jobID = steamClient.GetNextJobID();

            using var process = Process.GetCurrentProcess();
            var processStartTime = process.StartTime;

            // Recreate the datetime to get rid of milliseconds etc. and only keep the important bits
            var expectedProcessStartTime = new DateTime(processStartTime.Year, processStartTime.Month, processStartTime.Day, processStartTime.Hour, processStartTime.Minute, processStartTime.Second);

            Assert.Equal(expectedProcessStartTime, jobID.StartTime);
        }

        [Fact]
        public void GetNextJobIDSetsBoxIDToZero()
        {
            var steamClient = new SteamClient();
            var jobID = steamClient.GetNextJobID();

            Assert.Equal(0u, jobID.BoxID);
        }

        class TestMsgHandler : ClientMsgHandler
        {
            public override void HandleMsg(IPacketMsg packetMsg)
            {
            }
        }
    }
}
