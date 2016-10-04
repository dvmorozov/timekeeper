using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using TimeKeeper.WCFProxy;

namespace TimeKeeper
{
    public partial class ConfigPage : PhoneApplicationPage
    {
        public ConfigPage()
        {
            InitializeComponent();
            LoadSilenceInterval();
        }

        private void ApplicationBarIconActivitiesButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ApplicationBarIconButtonStatistics_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/StatisticsPage.xaml", UriKind.RelativeOrAbsolute));
        }

        public static string SilenceTimeFrom = "SilenceTimeFrom";
        public static string SilenceTimeTo = "SilenceTimeTo";

        private void UpdateSilenceInterval()
        {
            if (_isConfigUpdateBlocked) return;

            var settings = IsolatedStorageSettings.ApplicationSettings;

            var t = ((DateTime)TimePickerFrom.Value).ToShortTimeString();
            if (!settings.Contains(SilenceTimeFrom))
                settings.Add(SilenceTimeFrom, t);
            else
                settings[SilenceTimeFrom] = t;

            t = ((DateTime)TimePickerTo.Value).ToShortTimeString();
            if (!settings.Contains(SilenceTimeTo))
                settings.Add(SilenceTimeTo, t);
            else
                settings[SilenceTimeTo] = t;

            settings.Save();
        }

        private bool _isConfigUpdateBlocked = false;

        private void LoadSilenceInterval()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            //  Prevents saving default values from event handlers 
            //  during the initialization process.
            _isConfigUpdateBlocked = true;

            if (settings.Contains(SilenceTimeFrom))
            {
                var t = settings[SilenceTimeFrom] as string;
                TimePickerFrom.Value = DateTime.Parse(t);
            }

            if (settings.Contains(SilenceTimeTo))
            {
                var t = settings[SilenceTimeTo] as string;
                TimePickerTo.Value = DateTime.Parse(t);
            }

            _isConfigUpdateBlocked = false;
        }

        private void TimePickerFrom_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            UpdateSilenceInterval();
        }

        private void TimePickerTo_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            UpdateSilenceInterval();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var client = new WCFProxyClient();
            client.GetMessageCompleted += client_GetMessageCompleted;
            client.GetMessageAsync();
        }

        void client_GetMessageCompleted(object sender, GetMessageCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MessageBox.Show(e.Result);
            }
        }
    }
}
