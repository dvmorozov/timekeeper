using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TimeKeeper
{
    public partial class AddCategory : PhoneApplicationPage
    {
        public AddCategory()
        {
            InitializeComponent();
        }

        private void ButtonAddUrgentImportant_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Data.AddCategory(TextBoxCategoryName.Text, true, true);
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ButtonAddNotUrgentImportant_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Data.AddCategory(TextBoxCategoryName.Text, false, true);
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ButtonAddUrgentNotImportant_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Data.AddCategory(TextBoxCategoryName.Text, true, false);
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ButtonAddNotUrgentNotImportant_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Data.AddCategory(TextBoxCategoryName.Text, false, false);
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}