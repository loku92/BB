using BassBooster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace BassBooster.Converters
{
    public class TrackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Track t = value as Track;
            return t.Artist + " - " + t.Title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return true;
        }
    }
}
