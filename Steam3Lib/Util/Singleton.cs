using System;
using System.Collections.Generic;
using System.Text;

namespace SteamLib
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        static T _Instance;
        public static T Instance
        {
            get
            {
                if ( _Instance == null )
                    _Instance = new T();

                return _Instance;
            }

        }
    }
}
