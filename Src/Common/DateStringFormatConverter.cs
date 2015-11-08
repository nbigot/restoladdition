using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace RestoLAddition
{
    public class DateStringFormatConverter : IValueConverter
    {
        /// <summary>
        /// injection de valeur depuis les ressources
        /// </summary>
        public static string DateStrFormat = "{0:ddd d MMM yyyy}";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // http://www.e-naxos.com/Blog/post/StringFormat-une-simplification-Xaml-trop-peu-utilisee.aspx
            return string.Format(CultureInfo.CurrentCulture, DateStrFormat, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
