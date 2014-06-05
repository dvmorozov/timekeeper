using System.Windows;
using Microsoft.Phone.Controls;
using TimeKeeper.Resources;
using System;

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

        private void UpdateChart(out DateTime bestDay, out DateTime worstDay, out double bestPerf, out double worstPerf)
        {
            _lastDaysSerie.Points.Clear();
            bestDay = DateTime.Now;
            worstDay = DateTime.Now;
            bestPerf = 100;
            worstPerf = 100;

            if (MainPage.Statistics.LastDays.Count != 0)
            {
                bestDay = MainPage.Statistics.LastDays[0].Date;
                bestPerf = MainPage.Statistics.LastDays[0].IntegralPerf;

                worstDay = MainPage.Statistics.LastDays[0].Date;
                worstPerf = bestPerf;

                foreach (var c in MainPage.Statistics.LastDays)
                {
                    var point = new Sparrow.Chart.TimePoint();
                    point.Time = c.Date;
                    point.Value = c.IntegralPerf;
                    _lastDaysSerie.Points.Add(point);

                    if (c.IntegralPerf > bestPerf)
                    {
                        bestPerf = c.IntegralPerf;
                        bestDay = c.Date;
                    }

                    if (c.IntegralPerf < worstPerf)
                    {
                        worstPerf = c.IntegralPerf;
                        worstDay = c.Date;
                    }
                }
            }
        }

        private void UpdateTexts(DateTime bestDay, DateTime worstDay, double bestPerf, double worstPerf)
        {
            StatBestDay.Text = string.Format(AppResources.StatBestDayText, bestDay.ToString("dd.MM.yy"), string.Format("{0:0.0}", bestPerf));
            StatWorstDay.Text = string.Format(AppResources.StatWorstDayText, worstDay.ToString("dd.MM.yy"), string.Format("{0:0.0}", worstPerf));
            StatCountedTime.Text = string.Format(AppResources.StatCountedTimeText, MainPage.Data.CountedTimeStr);
            StatUncountedTime.Text = string.Format(AppResources.StatUncountedTimeText, MainPage.Data.UncountedTimeStr);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime bestDay, worstDay;
            double bestPerf, worstPerf;
            UpdateChart(out bestDay, out worstDay, out bestPerf, out worstPerf);
            UpdateTexts(bestDay, worstDay, bestPerf, worstPerf);
        }
    }
}