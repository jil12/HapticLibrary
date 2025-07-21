using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia;

namespace HapticLibrary.Converters
{
    public class PlayPauseIconConverter : IValueConverter
    {
        public static readonly PlayPauseIconConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isPlaying = value is bool b && b;
            string resourceKey = isPlaying ? "icon_pause" : "icon_play";
            
            // Try to get the resource from the application resources
            if (Application.Current?.Resources.TryGetResource(resourceKey, null, out var resource) == true)
            {
                return resource;
            }
            
            // Fallback to resource key string if not found
            return resourceKey;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 