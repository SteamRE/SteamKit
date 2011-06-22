using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple =false )]
    sealed class HandlerAttribute : Attribute
    {
        public string Name { get; set; }

        public HandlerAttribute( string name )
        {
            this.Name = name;
        }
    }
}
