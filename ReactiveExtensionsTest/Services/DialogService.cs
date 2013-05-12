using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ReactiveExtensionsTest.Services
{
    public class DialogService
    {
        public async static Task DisplayDialog(String message, String title = null)
        {
            MessageDialog msg = new MessageDialog(message, title??String.Empty);
            await msg.ShowAsync();
        }
    }
}
