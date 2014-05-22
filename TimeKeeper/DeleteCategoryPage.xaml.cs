﻿using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using TimeKeeper.Resources;
using TimeKeeper.Core;

namespace TimeKeeper
{
    public partial class DeleteCategoryPage : PhoneApplicationPage
    {
        public DeleteCategoryPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            CategoryList.ItemsSource = MainPage.Data.Any;
        }

        private void DeleteSelectedItem()
        {
            var item = (Category)CategoryList.SelectedItem;
            if (item != null)
            {
                //  Searches the list to protect from duplicate question.
                if (MainPage.Data.Any.IndexOf(item) != -1)
                {
                    var name = item.Name;
                    if (MessageBox.Show(string.Format(AppResources.DeleteCategoryMessage, name), AppResources.DeleteCategoryCaption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        MainPage.Data.DeleteCategory(item);
                    }
                }
            }
        }

        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteSelectedItem();
        }

        //  This allows to repeat the deleting procedure if user cancels it for the first time.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedItem();
        }
    }
}