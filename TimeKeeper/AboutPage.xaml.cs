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

        private void ApplicationBarIconActivitiesButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ApplicationBarIconStatisticsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/StatisticsPage.xaml", UriKind.RelativeOrAbsolute));
        }

        //  Replaces names with localized strings.
        private void BuildLocalizedApplicationBar()
        {
            for (var i = 0; i < ApplicationBar.Buttons.Count; i++)
            {
                var btn = ApplicationBar.Buttons[i] as ApplicationBarIconButton;
                if (btn != null)
                {
                    switch (btn.Text.Trim().ToLower())
                    {
                        //  Uses default names to update button captions.
                        case ("statistics"):
                            btn.Text = AppResources.AppBarButtonStatisticsText;
                            break;

                        case ("activities"):
                            btn.Text = AppResources.AppBarButtonActivitiesText;
                            break;
                    }
                }
            }
        }
    }
}