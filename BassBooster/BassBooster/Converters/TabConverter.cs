using BassBooster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace BassBooster.Converters
{

    /// <summary>
    /// Class that converts tab's(frame's) name and is used in UI 
    /// </summary>
    public class TabConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Tab t = value as Tab;
            return t.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return true;
        }
    }
}
