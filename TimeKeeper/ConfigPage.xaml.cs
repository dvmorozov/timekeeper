using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TimeKeeper
{
    public partial class ConfigPage : PhoneApplicationPage
    {
        public ConfigPage()
        {
            InitializeComponent();
        }

        private void ApplicationBarIconActivitiesButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ApplicationBarIconButtonStatistics_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/StatisticsPage.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}