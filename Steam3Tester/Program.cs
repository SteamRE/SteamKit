using System;
using System.Collections.Generic;
using System.Text;
using SteamLib;
using System.Net;

namespace Steam3Tester
{
    class Program
    {
        static void Main( string[] args )
        {
            CMInterface cmInterface = CMInterface.Instance;

            cmInterface.ConnectToCM();
        }
    }
}
