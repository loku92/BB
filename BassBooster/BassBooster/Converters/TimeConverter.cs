using BassBooster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace BassBooster.Converters
{
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Track t = value as Track;
            return MillisecondsToMinute((long)t.Duration);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return true;
        }

        public string MillisecondsToMinute(long milliseconds)
        {
            int minute = (int)(milliseconds / (1000 * 60));
            int seconds = (int)((milliseconds / 1000) % 60);
            if (seconds < 10)
                return (minute + " : 0" + seconds);
            else
                return (minute + " : " + seconds);
        }
    }
}
