using System;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class GameIDFacts
    {
        [Fact]
        public void ModCRCCorrect()
        {
            GameID gameId = new GameID(420, "Research and Development");

            Assert.True(gameId.IsMod);
            Assert.Equal(420u, gameId.AppID);
            Assert.Equal(new GameID(10210309621176861092), gameId);

            GameID gameId2 = new GameID(215, "hidden");

            Assert.True(gameId2.IsMod);
            Assert.Equal(215u, gameId2.AppID);
            Assert.Equal(new GameID(9826266959967158487), gameId2);
        }

        [Fact]
        public void ShortcutCRCCorrect()
        {
            GameID gameId = new GameID("\"C:\\Program Files (x86)\\Git\\mingw64\\bin\\wintoast.exe\"", "Git for Windows");

            Assert.True(gameId.IsShortcut);
            Assert.Equal(new GameID(12754778225939316736), gameId);
        }
    }
}
