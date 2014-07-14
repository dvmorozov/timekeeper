﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows;
using Newtonsoft.Json;
using System.Linq;
using TimeKeeper.Core.Resources;
using System.Threading.Tasks;

namespace TimeKeeper.Core
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

        //  Unique category identifier.
        [DataMember]
        public Guid CategoryId
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
            this.CategoryId = Guid.NewGuid();
            this.Name = name;
        }

        public Category(Category c)
        {
            this.CategoryId = c.CategoryId;
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
        DateTime Now { get; }
    }

    public class SysDateTime : IDateTime
    {
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }

        //  Returns the middle of the night (the first second of the day).
        static public DateTime Date(DateTime now)
        {
            var d = now.Date;
            d = d.AddHours(-12);
            return d;
        }

        static public DateTime NextDay(DateTime now)
        {
            //  The begin of new day must be excluded.
            var r = Date(now).AddDays(1);
            return r;
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
        [DataMember]
        private int _backgroundAgentInterval;
        //  Used in debug environment.
        public int BackgroundAgentInterval { get { return _backgroundAgentInterval; } }

        //  The time duration when no tasks are performed.
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

        //  Instantaneous performance (for statistics).
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
                else return (double)importantCount * 100.0 / (double)totalCount;
            }
        }

        //  Cached value of the performance.
        private double _lastPerf;

        public double LastPerf
        {
            get { return _lastPerf; }
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

                //  Adds inactive duration.
                notImportantDuration = notImportantDuration.Add(_inactiveDuration);
                if (Active.Count == 0)
                    notImportantDuration = notImportantDuration.Add(_dt.Now.Subtract(_lastActiveIsEmpty));

                var totalSeconds = notImportantDuration.TotalSeconds + importantDuration.TotalSeconds;
                if (totalSeconds != 0)
                    perf = importantDuration.TotalSeconds * 100.0 / totalSeconds;

                _lastPerf = perf;
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
            _startDate = t._startDate;
            _inactiveDuration = t._inactiveDuration;
            _lastActiveIsEmpty = t._lastActiveIsEmpty;
            _lastActiveIsEmptyInitialized = t._lastActiveIsEmptyInitialized;
            _backgroundAgentInterval = t._backgroundAgentInterval;
        }

        public void Reset()
        { 
            _startDate = _dt.Now;
            _lastActiveIsEmpty = _dt.Now;
            _inactiveDuration = new TimeSpan();
            _lastActiveIsEmptyInitialized = false;
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
            var categories = AppResources.DefaultCategories;
            var rootObject = JsonConvert.DeserializeObject<RootObject>(categories);
            foreach (var c in rootObject.CategorieList)
            {
                AddCategory(c.Name, c.Urgent, c.Important);
            }
        }

        public void AddCategory(string name, bool urgent = false, bool important = false)
        {
            var preparedName = name.Trim().ToLower();
            if (preparedName == string.Empty) return;

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

        private const string _fileName = "TimeExpensesData";

        public static async Task<TimeExpensesData> Load()
        {
            try
            {
                using (var stream = await PersistentData.GetReadFileStream(_fileName))
                {
                    //  Deserialize the object.
                    var ser = new DataContractJsonSerializer(typeof(TimeExpensesData));
                    return (TimeExpensesData)ser.ReadObject(stream);
                }
            }
            catch
            {
                //  In any case the object must be created!
                return new TimeExpensesData()
                {
                    _startDate = _dt.Now,
                    _lastActiveIsEmpty = _dt.Now,
                    _lastActiveIsEmptyInitialized = true,
                    _backgroundAgentInterval = 600 /*10 min*/
                };
            }
        }

        public async void Save()
        {
            //  Save data to file.
            try
            {
                using (var stream = await PersistentData.GetWriteFileStream(_fileName))
                {
                    var ser = new DataContractJsonSerializer(typeof(TimeExpensesData));
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
