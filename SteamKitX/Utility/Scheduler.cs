using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SteamKit
{
    class Scheduler : Singleton<Scheduler>
    {
        private Thread scheduleThread;

        public event EventHandler Think;

        private EventArgs emptyArgs = new EventArgs();

        public Scheduler()
        {
            scheduleThread = new Thread(ScheduleThink);
            scheduleThread.Start();
        }

        private void ScheduleThink()
        {
            while (true)
            {
                if (Think != null)
                {
                    Think(this, emptyArgs);
                }

                Thread.Sleep(500);
            }
        }
    }
}
