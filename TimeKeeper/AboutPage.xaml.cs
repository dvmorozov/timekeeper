using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Storage;
using Windows.ApplicationModel;
using Windows.System;
using TimeKeeper.Resources;

namespace TimeKeeper
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFile file = await Package.Current.InstalledLocation.GetFileAsync(@"Assets\Pages\Donate.html");
                await Launcher.LaunchFileAsync(file);
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Format(AppResources.PageOpeningErrorMessage, ex.Message));
            }
        }
    }
}