using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SteamLib
{
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public class MicroTime : Serializable<MicroTime>
    {
        public ulong Time;


        public DateTime ToDateTime()
        {
            return ( DateTime.MinValue + TimeSpan.FromTicks( ( long )Time * 10 ) );
        }


        public override string ToString()
        {
            return ToDateTime().ToString();
        }
    }
}
