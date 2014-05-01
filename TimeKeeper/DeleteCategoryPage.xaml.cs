using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Windows.Data;
using TimeKeeper.Resources;

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
            CategoryList.ItemsSource = MainPage.Data.categories;
        }

        private void DeleteSelectedItem()
        {
            var item = (Category)CategoryList.SelectedItem;
            if (item != null)
            {
                var name = item.Name;
                if (MessageBox.Show(string.Format(AppResources.DeleteCategoryMessage, name), AppResources.DeleteCategoryCaption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    MainPage.Data.DeleteCategory(item);
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