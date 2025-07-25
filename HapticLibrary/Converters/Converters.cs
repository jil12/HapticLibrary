using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Media;
using HapticLibrary.Models;
using HapticLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HapticLibrary.Converters
{
    public class DrawingColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Drawing.Color color)
            {
                return new SolidColorBrush(Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class EditorWordToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EditorWordViewModel editorWord)
            {
                if (editorWord.HapticPattern != null)
                {
                    System.Drawing.Color color = editorWord.HapticPattern.Color;
                    return new SolidColorBrush(Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                }
                return Brushes.Transparent;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class EditorWordToContrastForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EditorWordViewModel editorWord && editorWord.HapticPattern != null)
            {
                // Convert to linear RGB values (0-1)
                System.Drawing.Color color = editorWord.HapticPattern.Color;
                double r = color.R / 255.0;
                double g = color.G / 255.0;
                double b = color.B / 255.0;

                // Calculate luminance (perceived brightness)
                double luminance = 0.299 * r + 0.587 * g + 0.114 * b;

                // If the color is dark, return white; otherwise, black
                return luminance < 0.5 ? Brushes.White : Brushes.Black;
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ReferenceEqualsConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count >= 2)
            {
                return Equals(values[0], values[1]);
            }
            return false;
        }

        public object ConvertBack(IList<object> values, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException(); // Not needed for OneWay binding
    }


}
