using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SillowApp.Styles
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                string normalizedStatus = status.ToLowerInvariant();

                switch (normalizedStatus)
                {
                    case "job booked":
                        return Color.FromArgb("#4CAF50"); // Green

                    case "come back later":
                        return Color.FromArgb("#FFC107"); // Yellow

                    case "do not return":
                        return Color.FromArgb("#F44336"); // Red

                    default:
                        return Color.FromArgb("#64748B"); // Gray/Default
                }
            }

            return Color.FromArgb("#64748B");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}