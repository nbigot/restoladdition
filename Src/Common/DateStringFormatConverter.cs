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
    public class DateStringFormatConverter : IValueConverter
    {
        /// <summary>
        /// injection de valeur depuis les ressources
        /// </summary>
        public static string PriceStrFormat = "{0:ddd d MMM yyyy}";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // http://www.e-naxos.com/Blog/post/StringFormat-une-simplification-Xaml-trop-peu-utilisee.aspx
            //return string.Format(CultureInfo.CurrentCulture, parameter as string, value);
            return string.Format(CultureInfo.CurrentCulture, PriceStrFormat, value);

            //title = string.Format("Resto {0} ({1})", now.ToString("ddd d MMM", CultureInfo.CurrentCulture), ++i);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
