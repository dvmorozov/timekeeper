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

        public Category(string name)
        {
            this.Name = name;
        }
    }

    [DataContract]
    public class TimeExpensesData
    {
        [DataMember]
        private ObservableCollection<Category> _categories;

        public ObservableCollection<Category> Active
        {
            get
            {
                var result = new ObservableCollection<Category>();
                foreach (var c in _categories)
                {
                    if (c.Active)
                        result.Add(c);
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
                        result.Add(c);
                }
                //  TODO: sorting.
                return result;
            }
        }

        public ObservableCollection<Category> Any
        {
            get { return _categories; }
        }

        public TimeExpensesData()
        {
            //  Must be initialized in the constructor.
            _categories = new ObservableCollection<Category>();
        }

        public void AddCategory(string name)
        {
            var preparedName = name.Trim().ToLower();
            //  Searches for existing category with the same name.
            foreach (var c in _categories)
            {
                //  Silently exits if the category already exists.
                if (c.Name == preparedName)
                    return;
            }
            _categories.Add(new Category(name));
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
            return new TimeExpensesData();
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

    public partial class MainPage : PhoneApplicationPage
    {
        //  For exchange between pages.
        public static TimeExpensesData Data; 

        private void LoadCategories()
        {
            Data = TimeExpensesData.Load();
        }

        private void SaveCategories()
        {
            Data.Save();
        }

        private void UpdateLists()
        {
            CategoryListActive.ItemsSource = Data.Active;
            CategoryListPaused.ItemsSource = Data.Paused;
            //  Must be reset to interpret the first click as a change.
            CategoryListActive.SelectedItem = null;
            CategoryListPaused.SelectedItem = null;
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            LoadCategories();
            UpdateLists();

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
            var item = (Category)CategoryListPaused.SelectedItem;

            if (item != null)
            {
                item.Active = true;
                UpdateLists();
            }
        }

        private void StopActivity()
        {
            var item = (Category)CategoryListActive.SelectedItem;

            if (item != null)
            {
                item.Active = false;
                UpdateLists();
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
    }
}