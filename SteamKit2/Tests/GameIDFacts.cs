using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
    public class GameIDFacts
    {
        [TestMethod]
        public void ModCRCCorrect()
        {
            GameID gameId = new GameID(420, "Research and Development");

            Assert.IsTrue(gameId.IsMod);
            Assert.AreEqual(420u, gameId.AppID);
            Assert.AreEqual(new GameID(10210309621176861092), gameId);

            GameID gameId2 = new GameID(215, "hidden");

            Assert.IsTrue(gameId2.IsMod);
            Assert.AreEqual(215u, gameId2.AppID);
            Assert.AreEqual(new GameID(9826266959967158487), gameId2);
        }

        [TestMethod]
        public void ShortcutCRCCorrect()
        {
            GameID gameId = new GameID("\"C:\\Program Files (x86)\\Git\\mingw64\\bin\\wintoast.exe\"", "Git for Windows");

            Assert.IsTrue(gameId.IsShortcut);
            Assert.AreEqual(new GameID(12754778225939316736), gameId);
        }
    }
}
