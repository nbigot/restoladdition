using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace RestoLAddition
{
    // http://stackoverflow.com/questions/24127262/windows-phone-8-1-xaml-stringformat
    public class PriceStringFormatConverter : IValueConverter
    {

        /// <summary>
        /// injection de valeur depuis les ressources
        /// </summary>
        public static string PriceStrFormat = "{0:C}";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // http://www.e-naxos.com/Blog/post/StringFormat-une-simplification-Xaml-trop-peu-utilisee.aspx
            //return string.Format(CultureInfo.CurrentCulture, parameter as string, value);
            return string.Format(CultureInfo.CurrentCulture, PriceStrFormat, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
