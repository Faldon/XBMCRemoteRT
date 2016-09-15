using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace XBMCRemoteRT.Converters
{
    class RepeatToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) {
            string repeat = value == null ? "off" : (string)value;
            SolidColorBrush foreground = repeat == "off" ? null : (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];

            return foreground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
