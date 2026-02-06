using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace SillowApp.Styles
{
    public class Converters : IValueConverter
    {
        public Color TrueColor { get; set; } = Colors.White;
        public Color FalseColor { get; set; } = Colors.Black;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? TrueColor : FalseColor;
            return FalseColor;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}