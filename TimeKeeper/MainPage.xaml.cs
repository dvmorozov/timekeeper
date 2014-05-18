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

namespace TimeKeeper
{
    //  This class represents the work category.
    [DataContract]
    public class Category
    {
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        public bool Active
        {
            get;
            set;
        }

        [DataMember]
        public TimeSpan Duration
        {
            get;
            set;
        }

        [DataMember]
        public bool Urgent
        {
            get;
            set;
        }

        [DataMember]
        public bool Important
        {
            get;
            set;
        }

        [DataMember]
        public DateTime LastStart
        {
            get;
            set;
        }

        public static string DurationS(TimeSpan duration)
        {
            int hour, min, sec;
            hour = duration.Hours;
            min = duration.Minutes;
            sec = duration.Seconds;
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
        }

        public string DurationStr
        {
            get 
            {
                return DurationS(Duration);
            }
        }

        public Category(string name)
        {
            this.Name = name;
        }

        public Category(Category c)
        {
            this.Name = c.Name;
            this.Active = c.Active;
            this.Urgent = c.Urgent;
            this.Important = c.Important;
            this.Duration = c.Duration;
            this.LastStart = c.LastStart;
        }
    }

    //  Used for unit testing. 
    public interface IDateTime
    {
        DateTime Now{ get; }
    }

    public class SysDateTime : IDateTime
    {
        public DateTime Now
        {
            get {
                return DateTime.Now;
            }
        }

        //  Returns the middle of the night.
        static public DateTime Date(DateTime now)
        {
            return now.Date.AddHours(-12);
        }

        static public DateTime NextDay(DateTime now)
        {
            return Date(now).AddDays(1);
        }
    }

    [DataContract]
    public class TimeExpensesData
    {
        [DataMember]
        private ObservableCollection<Category> _categories;
        [DataMember]
        //  Initialized at the first attempt of loading.
        private DateTime _startDate;
        [DataMember]
        private TimeSpan _inactiveDuration;
        [DataMember]
        private DateTime _lastActiveIsEmpty;
        [DataMember]
        private bool _lastActiveIsEmptyInitialized;

        //  For unit-testing.
        public TimeSpan InactiveDuration { get { return _inactiveDuration; } }

        private static IDateTime _dt = new SysDateTime();
        //  Used for unit testing.
        public static IDateTime Dt { set { _dt = value; } }

        public void SetActive(string name, bool active)
        {
            var item = _categories.Single(t => t.Name == name);
            item.Active = active;
            if (active)
            {
                item.LastStart = _dt.Now;

                if (_lastActiveIsEmptyInitialized)
                {
                    _inactiveDuration = _inactiveDuration.Add(_dt.Now.Subtract(_lastActiveIsEmpty));
                    _lastActiveIsEmptyInitialized = false;
                }
            }
            else
            {
                item.Duration = item.Duration.Add(_dt.Now.Subtract(item.LastStart));

                var isAnyActive = Active.Count != 0;
                if (!isAnyActive)
                {
                    _lastActiveIsEmpty = _dt.Now;
                    _lastActiveIsEmptyInitialized = true;
                }
            }

            Save();
        }

        public ObservableCollection<Category> Active
        {
            get
            {
                var result = new ObservableCollection<Category>();
                foreach (var c in _categories)
                {
                    if (c.Active)
                        //  Copy ensures refreshment of the list.
                        result.Add(new Category(c));
                }
                //  TODO: sorting.
                return result;
            }
        }

        public ObservableCollection<Category> Paused
        {
            get
            {
                var result = new ObservableCollection<Category>();
                foreach (var c in _categories)
                {
                    if (!c.Active)
                        result.Add(new Category(c));
                }
                //  TODO: sorting.
                return result;
            }
        }

        public ObservableCollection<Category> Any
        {
            get { return _categories; }
        }

        //  Instantaneous performance.
        public double InstPerf
        {
            get
            {
                var importantCount = 0;
                var notImportantCount = 0;

                foreach (var c in Active)
                {
                    if (c.Important)
                        importantCount++;
                    else
                        notImportantCount++;
                }

                var totalCount = importantCount + notImportantCount;

                if (totalCount == 0) return 0;
                else return (double) importantCount * 100.0 / (double) totalCount;
            }
        }

        //  Accumulative performance.
        public double Perf
        {
            get
            {
                var perf = 0.0;
                var importantDuration = new TimeSpan();
                var notImportantDuration = new TimeSpan();

                //  Adds durations of all finished activities.
                foreach (var c in Any)
                {
                    if (c.Important)
                        importantDuration = importantDuration.Add(c.Duration);
                    else
                        notImportantDuration = notImportantDuration.Add(c.Duration);
                }

                //  Adds durations of all current activities.
                foreach (var c in Active)
                {
                    if (c.Important)
                        importantDuration = importantDuration.Add(_dt.Now.Subtract(c.LastStart));
                    else
                        notImportantDuration = notImportantDuration.Add(_dt.Now.Subtract(c.LastStart));
                }

                var totalSeconds = notImportantDuration.TotalSeconds + importantDuration.TotalSeconds;
                if (totalSeconds != 0)
                    perf = importantDuration.TotalSeconds * 100.0 / totalSeconds;

                return perf;
            }
        }

        public string PerfStr
        {
            get 
            {
                var perf = Perf;
                return string.Format("{0:0.0}", perf);
            }
        }

        //  The time counted for all activities in summary.
        public string UtilizedTimeStr
        {
            get
            {
                TimeSpan duration;
                foreach (var c in Any)
                    duration = duration.Add(c.Duration);

                return Category.DurationS(duration);
            }
        }

        //  The time counted for any activity.
        public string CountedTimeStr
        {
            get
            {
                TimeSpan totalDuration = _dt.Now.Subtract(_startDate);

                return Category.DurationS(totalDuration.Subtract(UncountedTime));
            }
        }

        public TimeSpan UncountedTime
        {
            get
            {
                TimeSpan duration;
                duration = duration.Add(_inactiveDuration);

                if (_lastActiveIsEmptyInitialized)
                    duration = duration.Add(_dt.Now.Subtract(_lastActiveIsEmpty));

                return duration;
            }
        }

        //  The time not counted for any activity.
        public string UncountedTimeStr
        {
            get
            {
                return Category.DurationS(UncountedTime);
            }
        }

        public TimeExpensesData()
        {
            //  Must be initialized in the constructor.
            _categories = new ObservableCollection<Category>();
        }

        public TimeExpensesData(TimeExpensesData t)
        {
            _categories = t.CopyCategories();
        }

        private ObservableCollection<Category> CopyCategories()
        {
            var result = new ObservableCollection<Category>();
            foreach (var c in _categories)
                result.Add(new Category(c));
            return result;
        }

        //  Classes for parsing JSON configuration data.
        public class CategorieList
        {
            public string Name { get; set; }
            public bool Urgent { get; set; }
            public bool Important { get; set; }
        }

        public class RootObject
        {
            public List<CategorieList> CategorieList { get; set; }
        }

        public void AddDefaultCategories()
        {
            if (MessageBox.Show(AppResources.AddDefaultCategoriesMessage, AppResources.AddDefaultCategoriesCaption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var categories = AppResources.DefaultCategories;
                var rootObject = JsonConvert.DeserializeObject<RootObject>(categories);
                foreach (var c in rootObject.CategorieList)
                {
                    AddCategory(c.Name, c.Urgent, c.Important);
                } 
            }
        }

        public void AddCategory(string name, bool urgent = false, bool important = false)
        {
            var preparedName = name.Trim().ToLower();
            //  Searches for existing category with the same name.
            foreach (var c in _categories)
            {
                //  Silently exits if the category already exists.
                if (c.Name == preparedName)
                    return;
            }
            _categories.Add(new Category(preparedName) { Urgent = urgent, Important = important });
            Save();
        }

        public void DeleteCategory(Category obj)
        {
            _categories.Remove(obj);
            Save();
        }

        private static string _fileName = "TimeExpensesData";

        public static TimeExpensesData Load()
        {
            try
            {
                //  Load data from file.
                var fileStorage = IsolatedStorageFile.GetUserStoreForApplication();

                if (fileStorage.FileExists(_fileName))
                {
                    using (var stream = new IsolatedStorageFileStream(_fileName, FileMode.Open, fileStorage))
                    {
                        //  Deserialize the object.
                        var ser = new DataContractJsonSerializer(typeof(TimeExpensesData));
                        return (TimeExpensesData)ser.ReadObject(stream);
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(string.Format(AppResources.ActionLoadingErrorMessage, e.Message));
            }
            //  In any case the object must be created!
            return new TimeExpensesData() { _startDate = _dt.Now, _lastActiveIsEmpty = _dt.Now, _lastActiveIsEmptyInitialized = true };
        }

        public void Save()
        {
            //  Save data to file.
            try
            {
                var fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
                using (var stream = new IsolatedStorageFileStream(_fileName, FileMode.Create, fileStorage))
                {
                    var ser = new DataContractJsonSerializer(typeof(TimeExpensesData));
                    ser.WriteObject(stream, this);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(AppResources.ActionSavingErrorMessage, e.Message));
            }
        }
    }

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
            CategoryList.ItemsSource = Data.Any;

            //  Must be reset to interpret the first click as a change.
            CategoryList.SelectedItem = null;
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

        private void StartActivity()
        {
            var item = (Category)CategoryList.SelectedItem;

            if (item != null)
            {
                Data.SetActive(item.Name, true);
                //  Must be after SetActive.
                Statistics.StartActivity();

                UpdateLists();
                UpdatePerfShortText();
            }

            
        }

        private void UpdatePerfShortText()
        {
            var text = string.Format(AppResources.PerformanceShortText, Data.PerfStr);
            PerfShortText.Text = text;
        }

        private void StopActivity()
        {
            var item = (Category)CategoryList.SelectedItem;

            if (item != null)
            {
                Data.SetActive(item.Name, false);
                //  Must be after SetActive.
                Statistics.StopActivity();

                UpdateLists();
                UpdatePerfShortText();
            }
        }

        private void ButtonStartAction_Click(object sender, RoutedEventArgs e)
        {
            StartActivity();
        }

        private void ButtonStopAction_Click(object sender, RoutedEventArgs e)
        {
            StopActivity();
        }

        private void CategoryListActive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StopActivity();
        }

        private void CategoryListPaused_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StartActivity();
        }

        private void ApplicationBarIconButtonStatistics_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/StatisticsPage.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}