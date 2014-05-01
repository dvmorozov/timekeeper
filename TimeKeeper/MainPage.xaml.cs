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
            var preparedName = name.Trim().ToLower();
            //  Searches for existing category with the same name.
            foreach (var c in categories)
            {
                //  Silently exits if the category already exists.
                if (c.Name == preparedName)
                    return;
            }
            categories.Add(new Category(name));
            Save();
        }

        public void DeleteCategory(Category obj)
        {
            categories.Remove(obj);
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
            catch
            {
                //  TODO: show message!
                var i = 0;
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
                using (var stream = new IsolatedStorageFileStream(_fileName, FileMode.OpenOrCreate, fileStorage))
                {
                    var ser = new DataContractJsonSerializer(typeof(TimeExpensesData));
                    ser.WriteObject(stream, this);
                }
            }
            catch (Exception e)
            {
                // TODO: show message! 
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

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            LoadCategories();

            CategoryList.ItemsSource = Data.categories;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //???
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
    }
}