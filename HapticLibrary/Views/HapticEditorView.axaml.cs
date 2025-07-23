using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HapticLibrary.ViewModels;
using System;
using System.Reflection;

namespace HapticLibrary.Views;

public partial class HapticEditorView : UserControl
{

    private HapticEditorViewModel ViewModel => DataContext as HapticEditorViewModel ?? throw new InvalidOperationException();

    public HapticEditorView()
    {
        InitializeComponent();
    }

    private void IntBox_TextInput(object? sender, TextInputEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private void IntBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // Parse min and max from Tag
            int min = 0, max = 100;
            PropertyInfo prop = null;
            if (textBox.Tag is string tag && tag.Contains(","))
            {
                var parts = tag.Split(',');
                if (int.TryParse(parts[0], out var parsedMin)) min = parsedMin;
                if (int.TryParse(parts[1], out var parsedMax)) max = parsedMax;

                if (parts.Length > 2)
                {
                    string propertyName = parts[2];
                    prop = ViewModel.GetType().GetProperty(propertyName);
                }
            }
            if (int.TryParse(textBox.Text, out int value))
            {
                value = Math.Clamp(value, min, max);
                textBox.Text = value.ToString();
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(ViewModel, value);
                }
            } else
            {
                textBox.Text = "0";
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(ViewModel, 0);
                }
            }
        }
    }

    private void FloatBox_TextInput(object? sender, TextInputEventArgs e)
    {
        e.Handled = !Single.TryParse(e.Text, out _);
    }

    private void FloatBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // Parse min and max from Tag
            float min = 0f, max = 1f;
            PropertyInfo prop = null;
            if (textBox.Tag is string tag && tag.Contains(","))
            {
                var parts = tag.Split(',');
                if (Single.TryParse(parts[0], out float parsedMin)) min = parsedMin;
                if (Single.TryParse(parts[1], out float parsedMax)) max = parsedMax;

                if (parts.Length > 2)
                {
                    string propertyName = parts[2];
                    prop = ViewModel.GetType().GetProperty(propertyName);
                }
            }
            if (Single.TryParse(textBox.Text, out float value))
            {
                value = Math.Clamp(value, min, max);
                textBox.Text = value.ToString();
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(ViewModel, value);
                }
            }
            else
            {
                textBox.Text = "0";
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(ViewModel, 0f);
                }
            }
        }
    }
}