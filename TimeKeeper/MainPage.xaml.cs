using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TimeKeeper.Resources;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization.Json;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Windows.Data;
using System.Globalization;
using TimeKeeper.Core;

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
    }
}