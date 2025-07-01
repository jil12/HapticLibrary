using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Datafeel;
using HapticLibrary.Models;
using System.Threading.Tasks;

namespace HapticLibrary.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    private string _demoTest = "This test worked!";

    [RelayCommand]
    private async Task AddAudioTextHapticsAsync()
    {

    }


    [RelayCommand]
    private async Task TestHapticsAsync()
    {
        HapticManager hapticManager = HapticManager.GetInstance();
        
        foreach (var d in hapticManager.DotManager.Dots)
        {
            d.LedMode = LedModes.GlobalManual;
            d.GlobalLed.Red = 255;
            d.GlobalLed.Green = 0;
            d.GlobalLed.Blue = 255;
            d.Write();
        }
    }
}
