using System.Windows;
using Microsoft.Phone.Controls;
using TimeKeeper.Resources;

namespace TimeKeeper
{
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

        private void UpdateTexts()
        {
            StatUtilizedTime.Text = string.Format(AppResources.StatUtilizedTimeText, MainPage.Data.UtilizedTimeStr);
            StatCountedTime.Text = string.Format(AppResources.StatCountedTimeText, MainPage.Data.CountedTimeStr);
            StatUncountedTime.Text = string.Format(AppResources.StatUncountedTimeText, MainPage.Data.UncountedTimeStr);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateChart();
            UpdateTexts();
        }
    }
}