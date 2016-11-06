using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Linq;
using TimeKeeper.Core.Resources;
using System.Threading.Tasks;

namespace TimeKeeper.Core
{
    //  https://action.mindjet.com/Task/14778395
    //  Base class contains all data attributes according to latest version.
    public class CategoryBase
    {
        public bool Active_;
        //  Unique category identifier.
        public Guid CategoryId_;
        public TimeSpan Duration_;
        public bool Important_;
        public DateTime LastStart_;
        public string Name_;
        public TaskId Task_;
        public bool Urgent_;

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
                return DurationS(Duration_);
            }
        }

        public CategoryBase()
        { }

        public CategoryBase(string name)
        {
            this.CategoryId_ = Guid.NewGuid();
            this.Name_ = name;
        }

        public CategoryBase(CategoryBase c)
        {
            this.CategoryId_ = c.CategoryId_;
            this.Name_ = c.Name_;
            this.Active_ = c.Active_;
            this.Urgent_ = c.Urgent_;
            this.Important_ = c.Important_;
            this.Duration_ = c.Duration_;
            this.LastStart_ = c.LastStart_;
        }
    }

    //  This class represents the work category.
    [DataContract]
    public class Category : CategoryBase
    {
        [DataMember]
        public string Name
        {
            get { return base.Name_; }
            set { base.Name_ = value; }
        }

        [DataMember]
        public bool Active
        {
            get { return base.Active_; }
            set { base.Active_ = value; }
        }

        [DataMember]
        public TimeSpan Duration
        {
            get { return base.Duration_; }
            set { base.Duration_ = value; }
        }

        [DataMember]
        public bool Urgent
        {
            get { return base.Urgent_; }
            set { base.Urgent_ = value; }
        }

        [DataMember]
        public bool Important
        {
            get { return base.Important_; }
            set { base.Important_ = value; }
        }

        [DataMember]
        public DateTime LastStart
        {
            get { return base.LastStart_; }
            set { base.LastStart_ = value; }
        }

        //  Unique category identifier.
        [DataMember]
        public Guid CategoryId
        {
            get { return base.CategoryId_; }
            set { base.CategoryId_ = value; }
        }

        public Category(string name) : base(name)
        {
        }

        public Category(CategoryBase c) : base(c)
        {
        }
    }

    [DataContract]
    //  https://action.mindjet.com/Task/14778395
    public class TaskId
    {
        [DataMember]
        public string Source
        {
            get;
            set;
        }

        [DataMember]
        public string Id
        {
            get;
            set;
        }

        [DataMember]
        //  Indicates that the Task was obtained from external source.
        public bool IsExternal
        {
            get;
            set;
        }
    }

    [DataContract]
    //  https://action.mindjet.com/Task/14778395
    //  The class has data member to store information about associated service from which Task was obtained.
    public class Category_2 : CategoryBase
    {
        [DataMember]
        public TaskId Task
        {
            get { return base.Task_; }
            set { base.Task_ = value; }
        }

        [DataMember]
        public string Name
        {
            get { return base.Name_; }
            set { base.Name_ = value; }
        }

        [DataMember]
        public bool Active
        {
            get { return base.Active_; }
            set { base.Active_ = value; }
        }

        [DataMember]
        public TimeSpan Duration
        {
            get { return base.Duration_; }
            set { base.Duration_ = value; }
        }

        [DataMember]
        public bool Urgent
        {
            get { return base.Urgent_; }
            set { base.Urgent_ = value; }
        }

        [DataMember]
        public bool Important
        {
            get { return base.Important_; }
            set { base.Important_ = value; }
        }

        [DataMember]
        public DateTime LastStart
        {
            get { return base.LastStart_; }
            set { base.LastStart_ = value; }
        }

        //  Unique category identifier.
        [DataMember]
        public Guid CategoryId
        {
            get { return base.CategoryId_; }
            set { base.CategoryId_ = value; }
        }

        public Category_2(string name) : base(name)
        {
        }

        public Category_2(CategoryBase c) : base(c)
        {
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

    public abstract class TimeExpensesDataBase
    {
        protected static IDateTime _dt = new SysDateTime();
        //  Used for unit testing.
        public static IDateTime Dt { set { _dt = value; } }

        protected ObservableCollection<CategoryBase> categories;
        protected const string fileName = "TimeExpensesDataBase";
        //  Initialized at the first attempt of loading.
        protected DateTime startDate;
        protected TimeSpan inactiveDuration;
        protected DateTime lastActiveIsEmpty;
        protected bool lastActiveIsEmptyInitialized;
        protected int backgroundAgentInterval;
        //  Used in debug environment.
        public int BackgroundAgentInterval { get { return backgroundAgentInterval; } }
        //  The time Duration when no tasks are performed.
        public TimeSpan InactiveDuration { get { return inactiveDuration; } }

        public void SetActive(string name, bool active)
        {
            var item = categories.Single(t => t.Name_ == name);
            item.Active_ = active;
            if (active)
            {
                item.LastStart_ = _dt.Now;

                if (lastActiveIsEmptyInitialized)
                {
                    inactiveDuration = inactiveDuration.Add(_dt.Now.Subtract(lastActiveIsEmpty));
                    lastActiveIsEmptyInitialized = false;
                }
            }
            else
            {
                item.Duration_ = item.Duration_.Add(_dt.Now.Subtract(item.LastStart_));

                var isAnyActive = Active.Count != 0;
                if (!isAnyActive)
                {
                    lastActiveIsEmpty = _dt.Now;
                    lastActiveIsEmptyInitialized = true;
                }
            }

            Save();
        }

        public ObservableCollection<CategoryBase> Active
        {
            get
            {
                var result = new ObservableCollection<CategoryBase>();
                foreach (var c in categories)
                {
                    if (c.Active_)
                        //  Copy ensures refreshment of the list.
                        result.Add(new CategoryBase(c));
                }
                //  TODO: sorting.
                return result;
            }
        }

        public ObservableCollection<CategoryBase> Paused
        {
            get
            {
                var result = new ObservableCollection<CategoryBase>();
                foreach (var c in categories)
                {
                    if (!c.Active_)
                        result.Add(new Category(c));
                }
                //  TODO: sorting.
                return result;
            }
        }

        public ObservableCollection<CategoryBase> Any
        {
            get { return categories; }
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
                    if (c.Important_)
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
                    if (c.Important_)
                        importantDuration = importantDuration.Add(c.Duration_);
                    else
                        notImportantDuration = notImportantDuration.Add(c.Duration_);
                }

                //  Adds durations of all current activities.
                foreach (var c in Active)
                {
                    if (c.Important_)
                        importantDuration = importantDuration.Add(_dt.Now.Subtract(c.LastStart_));
                    else
                        notImportantDuration = notImportantDuration.Add(_dt.Now.Subtract(c.LastStart_));
                }

                //  Adds inactive Duration.
                notImportantDuration = notImportantDuration.Add(inactiveDuration);
                if (Active.Count == 0)
                    notImportantDuration = notImportantDuration.Add(_dt.Now.Subtract(lastActiveIsEmpty));

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
                TimeSpan totalDuration = _dt.Now.Subtract(startDate);

                return Category.DurationS(totalDuration.Subtract(UncountedTime));
            }
        }

        public TimeSpan UncountedTime
        {
            get
            {
                TimeSpan duration;
                duration = duration.Add(inactiveDuration);

                if (lastActiveIsEmptyInitialized)
                    duration = duration.Add(_dt.Now.Subtract(lastActiveIsEmpty));

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

        public TimeExpensesDataBase()
        {
            //  Must be initialized in the constructor.
            categories = new ObservableCollection<CategoryBase>();
        }

        public TimeExpensesDataBase(TimeExpensesDataBase t)
        {
            categories = t.CopyCategories();
            startDate = t.startDate;
            inactiveDuration = t.inactiveDuration;
            lastActiveIsEmpty = t.lastActiveIsEmpty;
            lastActiveIsEmptyInitialized = t.lastActiveIsEmptyInitialized;
            backgroundAgentInterval = t.backgroundAgentInterval;
        }

        public void Reset()
        {
            startDate = _dt.Now;
            lastActiveIsEmpty = _dt.Now;
            inactiveDuration = new TimeSpan();
            lastActiveIsEmptyInitialized = false;
        }

        private ObservableCollection<CategoryBase> CopyCategories()
        {
            var result = new ObservableCollection<CategoryBase>();
            foreach (var c in categories)
                result.Add(new CategoryBase(c));
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

            //  Searches for existing category with the same Name.
            foreach (var c in categories)
            {
                //  Silently exits if the category already exists.
                if (c.Name_ == preparedName)
                    return;
            }
            categories.Add(new Category(preparedName) { Urgent = urgent, Important = important });
            Save();
        }

        public void DeleteCategory(CategoryBase obj)
        {
            categories.Remove(obj);
            Save();
        }

        public static async Task<TimeExpensesDataBase> Load()
        {
            try
            {
                using (var stream = await PersistentData.GetReadFileStream(fileName))
                {
                    //  Deserialize the object.
                    try
                    {
                        //  Tries to read object ver. 1.
                        var ser = new DataContractJsonSerializer(typeof(TimeExpensesData));
                        return (TimeExpensesData)ser.ReadObject(stream);
                    }
                    catch
                    {
                        //  Tries to read object ver. 1.
                        var ser = new DataContractJsonSerializer(typeof(TimeExpensesData_2));
                        return (TimeExpensesData_2)ser.ReadObject(stream);
                    }
                }
            }
            catch
            {
                //  In any case the object must be created!
                return new TimeExpensesData_2()
                {
                    startDate = _dt.Now,
                    lastActiveIsEmpty = _dt.Now,
                    lastActiveIsEmptyInitialized = true,
                    backgroundAgentInterval = 600 /*10 min*/
                };
            }
        }

        public abstract void Save();
    }

#pragma warning disable 169
    [DataContract]
    public class TimeExpensesData : TimeExpensesDataBase
    {
        //  https://action.mindjet.com/task/14778395
        //  Members of data contract are left as private to avoid any possible interference.
        [DataMember]
        //  Translates into appropriate data type (contract).
        private ObservableCollection<Category> _categories
        {
            get {
                var result = new ObservableCollection<Category>();
                foreach (var i in categories)
                    result.Add(new Category(i));
                return result;
            }

            set {
                categories.Clear();
                foreach (var i in value)
                    categories.Add(new CategoryBase(i));
            }
        }
        [DataMember]
        //  Initialized at the first attempt of loading.
        private DateTime _startDate
        {
            get { return startDate; }
            set { startDate = value; }
        }
        [DataMember]
        private TimeSpan _inactiveDuration
        {
            get { return inactiveDuration; }
            set { inactiveDuration = value; }
        }
        [DataMember]
        private DateTime _lastActiveIsEmpty
        {
            get { return lastActiveIsEmpty; }
            set { lastActiveIsEmpty = value; }
        }
        [DataMember]
        private bool _lastActiveIsEmptyInitialized
        {
            get { return lastActiveIsEmptyInitialized; }
            set { lastActiveIsEmptyInitialized = value; }
        }
        [DataMember]
        private int _backgroundAgentInterval
        {
            get { return backgroundAgentInterval; }
            set { backgroundAgentInterval = value; }
        }

        public TimeExpensesData() : base()
        {
        }

        public TimeExpensesData(TimeExpensesDataBase t) : base(t)
        {
        }

        public override async void Save()
        {
            //  Save data to file.
            try
            {
                using (var stream = await PersistentData.GetWriteFileStream(fileName))
                {
                    var ser = new DataContractJsonSerializer(typeof(TimeExpensesDataBase));
                    ser.WriteObject(stream, this);
                }
            }
            catch
            {
                //  To meet Windows Store conditions!
            }
        }
    }

    [DataContract]
    public class TimeExpensesData_2 : TimeExpensesDataBase
    {
        //  https://action.mindjet.com/task/14778395
        //  Members of data contract are left as private to avoid any possible interference.
        [DataMember]
        //  Translates into appropriate data type (contract).
        private ObservableCollection<Category_2> _categories
        {
            get
            {
                var result = new ObservableCollection<Category_2>();
                foreach (var i in categories)
                    result.Add(new Category_2(i));
                return result;
            }

            set
            {
                categories.Clear();
                foreach (var i in value)
                    categories.Add(new CategoryBase(i));
            }
        }
        [DataMember]
        //  Initialized at the first attempt of loading.
        private DateTime _startDate
        {
            get { return startDate; }
            set { startDate = value; }
        }
        [DataMember]
        private TimeSpan _inactiveDuration
        {
            get { return inactiveDuration; }
            set { inactiveDuration = value; }
        }
        [DataMember]
        private DateTime _lastActiveIsEmpty
        {
            get { return lastActiveIsEmpty; }
            set { lastActiveIsEmpty = value; }
        }
        [DataMember]
        private bool _lastActiveIsEmptyInitialized
        {
            get { return lastActiveIsEmptyInitialized; }
            set { lastActiveIsEmptyInitialized = value; }
        }
        [DataMember]
        private int _backgroundAgentInterval
        {
            get { return backgroundAgentInterval; }
            set { backgroundAgentInterval = value; }
        }

        public TimeExpensesData_2() : base()
        {
        }

        public TimeExpensesData_2(TimeExpensesDataBase t) : base(t)
        {
        }

        public override async void Save()
        {
            //  Save data to file.
            try
            {
                using (var stream = await PersistentData.GetWriteFileStream(fileName))
                {
                    var ser = new DataContractJsonSerializer(typeof(TimeExpensesData_2));
                    ser.WriteObject(stream, this);
                }
            }
            catch
            {
                //  To meet Windows Store conditions!
            }
        }
    }
#pragma warning restore 169
}
