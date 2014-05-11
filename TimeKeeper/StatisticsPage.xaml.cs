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

        private TimeExpensesData _data;
        private IDateTime _dt = new SysDateTime();
        //  Used for unit testing.
        public IDateTime Dt { set { _dt = value; } }

        public ObservableCollection<StatDay> LastDays
        {
            get { return _lastDays; }
        }

        public StatStack(TimeExpensesData data)
        {
            _lastDays = new ObservableCollection<StatDay>();
            _prevData = new TimeExpensesData();
            _data = data;
        }

        private void CopyData()
        {
            _prevData = new TimeExpensesData(_data);
            _lastRecalculationTime = _dt.Now;
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
                //  By default this gives the middle of the day, not 00:00. So, correct that!
                var firstDay = SysDateTime.Date(_lastRecalculationTime);
                var savedIntegralPerf = 0.0;

                for(var day = firstDay; day <= SysDateTime.Date(_dt.Now); day = day.AddDays(1))
                {
                    if (day == SysDateTime.Date(_dt.Now))
                    {
                        //  The last day or today (must be the first).
                        //  Contributes to the integral performance.
                        var diff = _dt.Now.Subtract(_lastRecalculationTime);
                        var seconds = diff.TotalSeconds;
                        _integralPerf += _prevData.InstPerf * seconds;
                    }
                    else
                    if (day == firstDay)
                    {
                        //  The first day.
                        //  Adds value to the integral.
                        var diff = day.AddDays(1).Subtract(_lastRecalculationTime);
                        var seconds = diff.TotalSeconds;
                        _integralPerf += _prevData.InstPerf * seconds;

                        //  Calculates integral performance.
                        savedIntegralPerf = _integralPerf / (24 * 60 * 60);
                        _integralPerf = 0;

                        _lastDays.Add(new StatDay(day.Date, savedIntegralPerf));
                        //  Sets the marker at the beginning of next day.
                        _lastRecalculationTime = SysDateTime.NextDay(_lastRecalculationTime);
                    }
                    else
                    {
                        //  Any other day.
                        //  Replicates last calculated performance.
                        //  It will be equal to the integral performance for the all day.
                        _lastDays.Add(new StatDay(SysDateTime.Date(day), _prevData.InstPerf));
                        //  Sets the marker at the beginning of next day.
                        _lastRecalculationTime = SysDateTime.NextDay(_lastRecalculationTime);
                    }
                }

                //  Removes old history.
                while (_lastDays.Count > _stackCapacity)
                    _lastDays.RemoveAt(0);
            }
                        
            CopyData();
            Save();
        }

        private static string _fileName = "StatisticsData";

        public static StatStack Load(TimeExpensesData data)
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
                        if (stack == null) stack = new StatStack(data);
                        return stack;
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(string.Format(AppResources.ActionLoadingErrorMessage, e.Message));
            }
            //  In any case the object must be created!
            return new StatStack(data);
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
        //  Chart serie.
        private Sparrow.Chart.SeriesBase _lastDaysSerie;

        public StatisticsPage()
        {
            InitializeComponent();

            _lastDaysSerie = new Sparrow.Chart.StepLineSeries();
            Chart.Series.Add(_lastDaysSerie);
        }

        private void UpdateChart()
        {
            _lastDaysSerie.Points.Clear();
            foreach (var c in MainPage.Statistics.LastDays)
            { 
                var point = new Sparrow.Chart.TimePoint();
                point.Time = c.Date;
                point.Value = c.IntegralPerf;
                _lastDaysSerie.Points.Add(point);
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateChart();
        }
    }
}