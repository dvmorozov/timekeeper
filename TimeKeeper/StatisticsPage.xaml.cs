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
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace TimeKeeper
{
    [DataContract]
    public class StatDay
    {
        [DataMember]
        public double IntegralPerf { get; set; }

        public StatDay() { }
    }

    [DataContract]
    public class StatStack
    {
        [DataMember]
        private ObservableCollection<StatDay> _lastDays;
        [DataMember]
        private ObservableCollection<Category> _prevTimeActive;
        [DataMember]
        private DateTime _lastRecalculationTime;

        public StatStack()
        {
            _lastDays = new ObservableCollection<StatDay>();
            _prevTimeActive = new ObservableCollection<Category>();
        }

        private void CopyActiveCategories()
        {
            _prevTimeActive = new ObservableCollection<Category>();
            foreach (var c in MainPage.Data.Active)
                _prevTimeActive.Add(new Category(c));

            _lastRecalculationTime = DateTime.Now;
        }

        public void StartActivity()
        {
            RecalculateStatisitics();
        }

        public void StopActivity()
        {
            RecalculateStatisitics(); 
        }

        public void RecalculateStatisitics()
        {
            //  Gets the current active task list.

            //  Checks has the change been done in the current day or not.
            if (_prevTimeActive != null)
            {
                //  Uses _prevTimeActive as initialization flag of _lastRecalculationTime.
                var diff = DateTime.Now.Subtract(_lastRecalculationTime);

                var days = diff.TotalDays;
            }
                        
            CopyActiveCategories();
            Save();
        }

        private static string _fileName = "StatisticsData";

        public static StatStack Load()
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
                        var ser = new DataContractJsonSerializer(typeof(StatStack));
                        var stack = (StatStack)ser.ReadObject(stream);
                        if (stack == null) stack = new StatStack();
                        return stack;
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(string.Format(AppResources.ActionLoadingErrorMessage, e.Message));
            }
            //  In any case the object must be created!
            return new StatStack();
        }

        public void Save()
        {
            //  Save data to file.
            try
            {
                var fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
                using (var stream = new IsolatedStorageFileStream(_fileName, FileMode.Create, fileStorage))
                {
                    var ser = new DataContractJsonSerializer(typeof(StatStack));
                    ser.WriteObject(stream, this);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(AppResources.ActionSavingErrorMessage, e.Message));
            }
        }
    }

    public partial class StatisticsPage : PhoneApplicationPage
    {
        private StatStack _statistics;

        private void LoadStatistics()
        {
            _statistics = StatStack.Load();
        }

        public StatisticsPage()
        {
            InitializeComponent();
            
            LoadStatistics();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            _statistics.RecalculateStatisitics();
        }
    }
}