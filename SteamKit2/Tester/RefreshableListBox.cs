using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Tester
{
    class RefreshableListBox : ListBox
    {
        public new void RefreshItems()
        {
            base.RefreshItems();
        }
    }
}
