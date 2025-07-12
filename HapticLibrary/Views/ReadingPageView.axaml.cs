using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HapticLibrary.ViewModels;

namespace HapticLibrary.Views;

public partial class ReadingPageView : UserControl
{
    public ReadingPageView()
    {
        InitializeComponent();

        this.AttachedToVisualTree += async (s, e) =>
        {
            if (DataContext is ReadingPageViewModel vm)
            {
                vm.LoadAsync(); // Your method
            }
        };
    }
}