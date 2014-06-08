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

        private const string silenceTimeFrom = "SilenceTimeFrom";
        private const string silenceTimeTo = "SilenceTimeTo";

        private void UpdateSilenceInterval()
        {
            if (_isConfigUpdateBlocked) return;

            var settings = IsolatedStorageSettings.ApplicationSettings;

            var t = ((DateTime)TimePickerFrom.Value).ToShortTimeString();
            if (!settings.Contains(silenceTimeFrom))
                settings.Add(silenceTimeFrom, t);
            else
                settings[silenceTimeFrom] = t;

            t = ((DateTime)TimePickerTo.Value).ToShortTimeString();
            if (!settings.Contains(silenceTimeTo))
                settings.Add(silenceTimeTo, t);
            else
                settings[silenceTimeTo] = t;

            settings.Save();
        }

        private bool _isConfigUpdateBlocked = false;

        private void LoadSilenceInterval()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            //  Prevents saving default values from event handlers 
            //  during the initialization process.
            _isConfigUpdateBlocked = true;

            if (settings.Contains(silenceTimeFrom))
            {
                var t = settings[silenceTimeFrom] as string;
                TimePickerFrom.Value = DateTime.Parse(t);
            }

            if (settings.Contains(silenceTimeTo))
            {
                var t = settings[silenceTimeTo] as string;
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
    }
}