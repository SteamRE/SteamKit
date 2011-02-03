using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Tester
{
    class Friend
    {
        public ulong FriendID;

        public override string ToString()
        {
            return SteamContext.SteamFriends.GetFriendPersonaName( FriendID );
        }
    }

    class FriendListBox : ListBox
    {
        public new void RefreshItems()
        {
            base.RefreshItems();
        }

        public new void RefreshItem( int index )
        {
            base.RefreshItem( index );
        }

        public int FindIndexOfFriend( ulong steamid )
        {
            for ( int x = 0 ; x < this.Items.Count ; ++x )
            {
                Friend frnd = this.Items[ x ] as Friend;

                if ( frnd == null )
                    continue;

                if ( frnd.FriendID == steamid )
                    return x;
            }

            return -1;
        }
    }
}
