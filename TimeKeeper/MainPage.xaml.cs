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
        private string _name;

        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
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
        public ObservableCollection<Category> categories;

        public TimeExpensesData()
        {
            //  Must be initialized in the constructor.
            categories = new ObservableCollection<Category>();
        }

        public void AddCategory(string name)
        {
            categories.Add(new Category(name));
        }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        private TimeExpensesData _data;
        private string _fileName = "TimeExpensesData";

        public static string CategoryName;

        private void LoadCategories()
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
                        _data = (TimeExpensesData)ser.ReadObject(stream);
                    }
                }
                
                if(_data == null)
                    _data = new TimeExpensesData();

                //???
                //for (var i = 0; i < 50; i++)
                //    _data.categories.Add(new Category((i * 100000).ToString()));
            }
            catch
            {
                //  TODO: show message!
            }
        }

        private void SaveCategories()
        {
            //  Save data to file.
            try
            {
                var fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
                using (var stream = new IsolatedStorageFileStream(_fileName, FileMode.OpenOrCreate, fileStorage))
                {
                    var ser = new DataContractJsonSerializer(typeof(TimeExpensesData));
                    ser.WriteObject(stream, _data);
                }
            }
            catch(Exception e) 
            { 
                // TODO: show message! 
                var i = 0;
            }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            LoadCategories();

            CategoryList.ItemsSource = _data.categories;

            BuildLocalizedApplicationBar();
        }

        //  Replaces names with localized strings.
        private void BuildLocalizedApplicationBar()
        {
            for(var i = 0; i < ApplicationBar.Buttons.Count; i++)
            {
                var btn = ApplicationBar.Buttons[i] as ApplicationBarIconButton;
                if (btn != null && btn.Text.ToLower() == "add")
                {
                    btn.Text = AppResources.AppBarButtonAddText;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //???
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddCategoryPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (CategoryName != null)
            {
                //  Return by pressing "Add category".
                _data.AddCategory(CategoryName);
                SaveCategories();
            }
        }
    }
}