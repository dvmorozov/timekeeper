
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace TimeKeeper.Core
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
        private TimeExpensesDataBase _prevData;
        [DataMember]
        private DateTime _lastRecalculationTime;
        [DataMember]
        private bool _lastRecalculationTimeInitialized;
        [DataMember]
        private double _integralPerf;

        private TimeExpensesDataBase _data;
        private IDateTime _dt;
        //  Used for unit testing.
        public IDateTime Dt { set { _dt = value; } }

        public ObservableCollection<StatDay> LastDays
        {
            get { return _lastDays; }
        }

        //  Initializes the attributes not read from the JSON-stream.
        //  Is called after ReadObject.
        public void Initialize(TimeExpensesDataBase data)
        {
            _dt = new SysDateTime();
            _data = data;

            //  Adds additional points if necessary.
            RecalculateStatisitics();
        }

        public StatStack(TimeExpensesDataBase data)
        {
            _lastDays = new ObservableCollection<StatDay>();
            _prevData = new TimeExpensesData_2();

            Initialize(data);
        }

        private void CopyData()
        {
            _prevData = new TimeExpensesData_2(_data);
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
            //  Gets the current Active Task list.

            //  Checks has the change been done in the current day or not.
            if (_lastRecalculationTimeInitialized)
            {
                //  By default this gives the middle of the day, not 00:00. So, correct that!
                var firstDay = SysDateTime.Date(_lastRecalculationTime);
                var savedIntegralPerf = 0.0;

                for (var day = firstDay; day <= SysDateTime.Date(_dt.Now); day = day.AddDays(1))
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
                            var diff = 24 * 3600 - _lastRecalculationTime.TimeOfDay.TotalSeconds; 
                            _integralPerf += _prevData.InstPerf * diff;

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

                //  Adds missed days to get full list.
                firstDay = _lastDays.Count != 0 ? _lastDays[0].Date : _dt.Now;
                while (_lastDays.Count < _stackCapacity)
                {
                    firstDay = firstDay.AddDays(-1);
                    _lastDays.Insert(0, new StatDay(SysDateTime.Date(firstDay), 0));
                }
            }

            CopyData();
            Save();
        }

        private const string _fileName = "StatisticsData";

        public static async Task<StatStack> Load(TimeExpensesDataBase data)
        {
            try
            {
                using (var stream = await PersistentData.GetReadFileStream(_fileName))
                {
                    //  Deserialize the object.
                    var ser = new DataContractJsonSerializer(typeof(StatStack));
                    var stack = (StatStack)ser.ReadObject(stream);

                    if (stack == null) stack = new StatStack(data);
                    else stack.Initialize(data);

                    return stack;
                }
            }
            catch
            {
                //  In any case the object must be created!
                return new StatStack(data);
            }
        }

        public async void Save()
        {
            //  Save data to file.
            try
            {
                using (var stream = await PersistentData.GetWriteFileStream(_fileName))
                {
                    var ser = new DataContractJsonSerializer(typeof(StatStack));
                    ser.WriteObject(stream, this);
                }
            }
            catch
            {
                //  To meet Windows Store conditions!
            }
        }
    }
}