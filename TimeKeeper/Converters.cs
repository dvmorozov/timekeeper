using System;
using System.Globalization;
using System.Windows.Data;

namespace TimeKeeper
{
    public class CategoryListIconConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = value as Category;
            if (c != null)
            {
                if (c.Active) return "/Assets/AppBar/transport.pause.png";
                else return "/Assets/AppBar/transport.play.png";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CategoryListTileColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = value as Category;
            if (c != null)
            {
                if (c.Active) return "PhoneBackgroundColor";
                if (c.Important && c.Urgent) return "BlueViolet";
                if (c.Important && !c.Urgent) return "Green";
                if (!c.Important && c.Urgent) return "Orange";
                if (!c.Important && !c.Urgent) return "Red";
            }
            return "Red";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CategoryActiveListTileColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = value as Category;
            if (c != null)
            {
                if (c.Important && c.Urgent) return "BlueViolet";
                if (c.Important && !c.Urgent) return "Green";
                if (!c.Important && c.Urgent) return "Orange";
                if (!c.Important && !c.Urgent) return "Red";
            }
            return "Red";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
