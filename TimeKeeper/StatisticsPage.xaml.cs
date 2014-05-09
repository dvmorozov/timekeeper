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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TimeKeeper
{
    [DataContract]
    public class StatDay
    {
        [DataMember]
        public double IntegralPerf { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        public StatDay(DateTime date, double integralPerf) 
        {
            Date = date;
            IntegralPerf = integralPerf;
        }
    }

    [DataContract]
    public class StatStack
    {
        private const int _stackCapacity = 30;
        [DataMember]
        private ObservableCollection<StatDay> _lastDays;
        [DataMember]
        private TimeExpensesData _prevData;
        [DataMember]
        private DateTime _lastRecalculationTime;
        [DataMember]
        private bool _lastRecalculationTimeInitialized;
        [DataMember]
        private double _integralPerf;

        public ObservableCollection<StatDay> LastDays
        {
            get { return _lastDays; }
        }

        public StatStack()
        {
            _lastDays = new ObservableCollection<StatDay>();
            _prevData = new TimeExpensesData();
        }

        private void CopyData()
        {
            _prevData = new TimeExpensesData(MainPage.Data);
            _lastRecalculationTime = DateTime.Now;
            _lastRecalculationTimeInitialized = true;
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
            if (_lastRecalculationTimeInitialized)
            {
                var diff = DateTime.Now.Subtract(_lastRecalculationTime);
                //??? var days = diff.Days;
                var days = 1;

                //if (days == 0)
                //{
                //    //  Activity list changed at the same day.
                //    var seconds = diff.Seconds;
                //    _integralPerf += _prevData.Perf * seconds;
                //}
                //else
                { 
                    //  Calculates integral performance.
                    //_integralPerf = _integralPerf / (24 * 60 * 60);
                    //???
                    _integralPerf = _prevData.Perf;

                    //  Adds days to the history list.
                    for (var i = 0; i < days; i++)
                    {
                        _lastDays.Add(new StatDay(DateTime.Now, _integralPerf));
                    }
                    //  Removes old history.
                    while (_lastDays.Count > _stackCapacity)
                        _lastDays.RemoveAt(0);
                    //  Recalculates dates.
                    /*
                    var index = 0;
                    for (var i = -1 * _lastDays.Count; i < 0; i++)
                        _lastDays[index++].Index = i;
                     */
                    //  Resets the integral performance.
                    _integralPerf = 0.0;
                }
            }
                        
            CopyData();
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
        //  Chart serie.
        private Sparrow.Chart.ColumnSeries _lastDaysSerie;

        private void LoadStatistics()
        {
            _statistics = StatStack.Load();
        }

        public StatisticsPage()
        {
            InitializeComponent();

            _lastDaysSerie = new Sparrow.Chart.ColumnSeries();
            Chart.Series.Add(_lastDaysSerie);

            LoadStatistics();
        }

        private void UpdateChart()
        {
            _lastDaysSerie.Points.Clear();
            foreach (var c in _statistics.LastDays)
            { 
                var point = new Sparrow.Chart.TimePoint();
                point.Time = c.Date;
                point.Value = c.IntegralPerf;
                _lastDaysSerie.Points.Add(point);
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            _statistics.RecalculateStatisitics();
            UpdateChart();
        }
    }
}