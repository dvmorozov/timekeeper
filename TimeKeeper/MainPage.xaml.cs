using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using TimeKeeper.Core;
using TimeKeeper.Resources;

namespace TimeKeeper
{
    //  ??? TODO: delete this.
    public class List_ClassConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = value as Category;
            if (c != null)
            {
                if (c.Active) return "/Assets/AppBar/transport.pause.png";
                else return "/Assets/AppBar/transport.play.png";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        //  For exchange between pages.
        public static TimeExpensesData Data;
        public static StatStack Statistics;

        private void LoadCategories()
        {
            Data = TimeExpensesData.Load();
        }

        private void SaveCategories()
        {
            Data.Save();
        }

        private void LoadStatistics()
        {
            Statistics = StatStack.Load(Data);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateLists()
        {
            CategoryListActive.ItemsSource = null;
            CategoryListPaused.ItemsSource = null;

            CategoryListActive.ItemsSource = Data.Active;
            CategoryListPaused.ItemsSource = Data.Any;
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            LoadCategories();
            LoadStatistics();

            if (Data.Any.Count == 0)
                Data.AddDefaultCategories();

            UpdateLists();
            UpdatePerfShortText();

            BuildLocalizedApplicationBar();

            StartPeriodicAgent();
        }

        //  Replaces names with localized strings.
        private void BuildLocalizedApplicationBar()
        {
            for(var i = 0; i < ApplicationBar.Buttons.Count; i++)
            {
                var btn = ApplicationBar.Buttons[i] as ApplicationBarIconButton;
                if (btn != null)
                {
                    switch(btn.Text.Trim().ToLower())
                    {
                        //  Uses default names to update button captions.
                        case("add"):
                            btn.Text = AppResources.AppBarButtonAddText;
                        break;

                        case ("delete"):
                            btn.Text = AppResources.AppBarButtonDeleteText;
                        break;

                        case("statistics"):
                        btn.Text = AppResources.AppBarButtonStatisticsText;
                        break;
                    }
                }
            }
        }

        private void ApplicationBarIconButtonAdd_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddCategoryPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ApplicationBarIconButtonDelete_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/DeleteCategoryPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void StartActivity(Category item)
        {
            Data.SetActive(item.Name, true);
            //  Must be after SetActive.
            Statistics.StartActivity();

            UpdateLists();
            UpdatePerfShortText();
        }

        private void UpdatePerfShortText()
        {
            var text = string.Format(AppResources.PerformanceShortText, Data.PerfStr);
            PerfShortText.Text = text;
        }

        private void StopActivity(Category item)
        {
            Data.SetActive(item.Name, false);
            //  Must be after SetActive.
            Statistics.StopActivity();

            UpdateLists();
            UpdatePerfShortText();
        }

        private void ButtonStartStopAction_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var value = (Guid)button.CommandParameter;

            var item = Data.Paused.Single(t => t.CategoryId == value);

            if (item.Active) StopActivity(item);
            else StartActivity(item);
        }

        private void ButtonStopAction_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var value = (Guid)button.CommandParameter;

            var item = Data.Active.Single(t => t.CategoryId == value);
            StopActivity(item);
        }

        private void ApplicationBarIconButtonStatistics_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/StatisticsPage.xaml", UriKind.RelativeOrAbsolute));
        }

        PeriodicTask periodicTask;

        string periodicTaskName = "TimeKeeperPerformanceUpdater";
        public bool agentsAreEnabled = true;

        private void StartPeriodicAgent()
        {
            // Variable for tracking enabled status of background agents for this app.
            agentsAreEnabled = true;

            // Obtain a reference to the period task, if one exists
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            if (periodicTask != null)
            {
                RemoveAgent(periodicTaskName);
            }

            periodicTask = new PeriodicTask(periodicTaskName);

            // The description is required for periodic agents. This is the string that the user
            // will see in the background services Settings page on the device.
            periodicTask.Description = "Updating TimeKeeper performance values.";

            // Place the call to Add in a try block in case the user has disabled agents.
            try
            {
                ScheduledActionService.Add(periodicTask);

                // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
                if (Debugger.IsAttached)
                    ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(Data.BackgroundAgentInterval));
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    MessageBox.Show("Background agents for this application have been disabled by the user.");
                    agentsAreEnabled = false;
                }

                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.
                }
            }
            catch (SchedulerServiceException)
            {
                // No user action required.
            }
        }

        private void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
            }
            catch (Exception)
            {
            }
        }
    }
}