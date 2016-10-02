using System;
using Windows.UI.Xaml.Data;

namespace XBMCRemoteRT.Converters
{
    class ToLowerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string str = value as string;
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            else return str.ToLower();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
