using System;
using System.Globalization;
using System.Windows.Data;
using Shouter.Models;

namespace Shouter.Converters
{
    class DoubleToVolumeLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double volume = (double)value / 100.0;
            if(volume > 0.40)
            {
                return VolumeLevel.Normal;
            }
            else if(volume > 0)
            {
                return VolumeLevel.Low;
            }
            return VolumeLevel.Muted;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
