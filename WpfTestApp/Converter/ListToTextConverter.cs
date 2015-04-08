using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace WpfTestApp.Converter
{
    internal class ListToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var sb = new StringBuilder();
            foreach (var s in (ConcurrentBag<string>)value) sb.AppendLine(s);
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string[] lines = ((string)value).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            return lines.ToList();
        }
    }
}
