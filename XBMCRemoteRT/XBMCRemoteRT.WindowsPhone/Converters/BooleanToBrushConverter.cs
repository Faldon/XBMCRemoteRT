using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace XBMCRemoteRT.Converters
{
    class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) {
            bool active = (bool)value;
            SolidColorBrush foreground = active ? (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"] : null;

            return foreground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
