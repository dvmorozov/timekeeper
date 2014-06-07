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
using System.Windows.Threading;
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

        public static void ResetStatistics()
        {
            Data.Reset();
            Statistics = new StatStack(Data);
        }

        private void LoadCategories()
        {
            var errMessage = string.Empty;
            Data = TimeExpensesData.Load(out errMessage);
            if (errMessage != string.Empty)
                MessageBox.Show(string.Format(AppResources.ActionLoadingErrorMessage, errMessage));
        }

        private void SaveCategories()
        {
            Data.Save();
        }

        private void LoadStatistics()
        {
            var errMsg = string.Empty;
            Statistics = StatStack.Load(Data, out errMsg);
            if (errMsg != string.Empty)
                MessageBox.Show(string.Format(AppResources.ActionLoadingErrorMessage, errMsg));
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
            DeleteCategoryList.ItemsSource = null;

            CategoryListActive.ItemsSource = Data.Active;
            CategoryListPaused.ItemsSource = Data.Any;
            DeleteCategoryList.ItemsSource = Data.Any;
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            LoadCategories();
            LoadStatistics();

            if (Data.Any.Count == 0)
            {
                //  To meet Windows Store conditions!
                if (MessageBox.Show(
                        AppResources.AddDefaultCategoriesMessage, AppResources.AddDefaultCategoriesCaption, 
                        MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    Data.AddDefaultCategories();
            }

            UpdateLists();
            UpdatePerfShortText();
            ShowArrow();

            BuildLocalizedApplicationBar();

            //StartPeriodicAgent();
            SetTimer();
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

        private void SetAppBarTexts()
        {
            ApplicationBarIconButton b = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            /*
            if (b != null)
            {
                b.Text = "p0wnage";
            }
             * */
            ApplicationBarMenuItem m = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            if (m != null)
            {
                m.Text = AppResources.MainMenuItemAboutCaption;
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetAppBarTexts();            
        }

        private void StartActivity(Category item)
        {
            Data.SetActive(item.Name, true);
            //  Must be after SetActive.
            Statistics.StartActivity();

            UpdateLists();
            UpdatePerfShortText();
            ShowArrow();
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
            ShowArrow();
        }

        private void ButtonStartStopAction_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var value = (Guid)button.CommandParameter;

            if (Data.Paused.Any(t => t.CategoryId == value))
            {
                var item = Data.Paused.Single(t => t.CategoryId == value);

                if (item.Active) StopActivity(item);
                else StartActivity(item);
            }
        }

        private void ButtonStopAction_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var value = (Guid)button.CommandParameter;

            if (Data.Active.Any(t => t.CategoryId == value))
            {
                var item = Data.Active.Single(t => t.CategoryId == value);
                StopActivity(item);
            }
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
            periodicTask.Description = AppResources.BackgroundAgentDescription;

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
                    MessageBox.Show(AppResources.BackgroundAgentsDisabled);
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

        private double _prevPerf;
        private bool _prevPerfInitialized;

        private void PlayDing()
        {
            var perf = Data.LastPerf;
            if (_prevPerfInitialized)
            {
                if (perf >= _prevPerf)
                {
                    DingUp.Play();
                    TriangleUp.Visibility = Visibility.Visible;
                    TriangleDown.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DingDown.Play();
                    TriangleUp.Visibility = Visibility.Collapsed;
                    TriangleDown.Visibility = Visibility.Visible;
                }
            }
            _prevPerf = perf;
            _prevPerfInitialized = true;
        }

        private void ShowArrow()
        {
            var perf = Data.LastPerf;
            if (_prevPerfInitialized)
            {
                if (perf >= _prevPerf)
                {
                    TriangleUp.Visibility = Visibility.Visible;
                    TriangleDown.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TriangleUp.Visibility = Visibility.Collapsed;
                    TriangleDown.Visibility = Visibility.Visible;
                }
            }
            _prevPerf = perf;
            _prevPerfInitialized = true;
        }

        private bool _wasATimerEvent;
        private DateTime _prevTimerEvent;
        private static IDateTime _dt = new SysDateTime();

        void OnTimerTick(Object sender, EventArgs args)
        {
            UpdatePerfShortText();
            ShowArrow();

            if (!_wasATimerEvent)
            {
                //  Must be after UpdatePerfShortText(), because uses the cached value of performance.
                PlayDing();
                _wasATimerEvent = true;
            }
            else
            {
                //  Ding sounds every half an hour.
                var now = _dt.Now;
                if((now.Minute == 0 && _prevTimerEvent.Minute != 0) ||
                   (now.Minute >= 30 && _prevTimerEvent.Minute < 30))
                    PlayDing();
            }
            _prevTimerEvent = _dt.Now;
        }

        private void SetTimer()
        {
            // creating timer instance
            var newTimer = new DispatcherTimer();
            // timer interval specified as 1 second
            newTimer.Interval = TimeSpan.FromSeconds(10);
            // Sub-routine OnTimerTick will be called at every ... seconds
            newTimer.Tick += OnTimerTick;
            // starting the timer
            newTimer.Start();
        }

        private void ApplicationBarAboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void DeleteSelectedItem()
        {
            var item = (Category)DeleteCategoryList.SelectedItem;
            if (item != null)
            {
                //  Searches the list to protect from duplicate question.
                if (MainPage.Data.Any.IndexOf(item) != -1)
                {
                    var name = item.Name;
                    if (MessageBox.Show(string.Format(AppResources.DeleteCategoryMessage, name), AppResources.DeleteCategoryCaption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        MainPage.Data.DeleteCategory(item);
                    }
                }
            }
        }

        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteSelectedItem();
        }

        //  This allows to repeat the deleting procedure if user cancels it for the first time.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedItem();
        }

        private void ButtonAddUrgentImportant_Click(object sender, RoutedEventArgs e)
        {
            Data.AddCategory(TextBoxCategoryName.Text, true, true);
            MainPivot.SelectedIndex = 0;
        }

        private void ButtonAddNotUrgentImportant_Click(object sender, RoutedEventArgs e)
        {
            Data.AddCategory(TextBoxCategoryName.Text, false, true);
            MainPivot.SelectedIndex = 0;
        }

        private void ButtonAddUrgentNotImportant_Click(object sender, RoutedEventArgs e)
        {
            Data.AddCategory(TextBoxCategoryName.Text, true, false);
            MainPivot.SelectedIndex = 0;
        }

        private void ButtonAddNotUrgentNotImportant_Click(object sender, RoutedEventArgs e)
        {
            Data.AddCategory(TextBoxCategoryName.Text, false, false);
            MainPivot.SelectedIndex = 0;
        }
    }
}