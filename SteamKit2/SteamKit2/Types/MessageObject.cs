using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public class MessageObject
    {
        public KeyValue KeyValues { get; private set; }


        public MessageObject( KeyValue keyValues )
        {
            this.KeyValues = keyValues;
        }
        public MessageObject()
        {
            this.KeyValues = new KeyValue( "MessageObject" );
        }


        public bool ReadFromStream( Stream stream )
        {
            bool success = KeyValues.ReadAsBinary( stream );

            if ( success )
            {
                // our binary KV parsing is a a little wonky in that we load the data one extra layer deep,
                // so the actual MessageObject that we want is the first child
                KeyValues = KeyValues.Children.FirstOrDefault() ?? KeyValue.Invalid;
            }

            return success;
        }
    }
}
