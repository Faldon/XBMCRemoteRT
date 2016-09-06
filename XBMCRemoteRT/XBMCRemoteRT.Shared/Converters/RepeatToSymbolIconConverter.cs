using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace XBMCRemoteRT.Converters
{
    class RepeatToSymbolIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) {
            string repeat = value == null ? "off" : (string)value;
            return repeat != "one" ? "RepeatAll" : "RepeatOne";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
