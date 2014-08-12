using System;
using log4net;
using log4net.Config;
using SteamKit2;

namespace BotTest 
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            log.Debug("Creating dota bot...");
            var bot = new DotaBot.DotaBot(true, new SteamUser.LogOnDetails()
            {
                Username = "webleague2",
                Password = "ciagnickeyen"
            });
            bot.Start();
            Console.ReadLine();
        }
    }
}
