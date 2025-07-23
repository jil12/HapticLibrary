using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HapticLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace HapticLibrary.ViewModels
{
    public partial class HapticEditorViewModel : ViewModelBase, IPageViewModel
    {
        [ObservableProperty]
        private string _hapticName = "";
        [ObservableProperty]
        private int _hapticRed = 0;
        [ObservableProperty]
        private int _hapticGreen = 0;
        [ObservableProperty]
        private int _hapticBlue = 0;
        [ObservableProperty]
        private float _hapticTemperature = 0f;
        [ObservableProperty]
        private float _hapticVibration = 0f;


        [ObservableProperty]
        private ObservableCollection<WordModel> words = new();

        [ObservableProperty]
        private ObservableCollection<HapticPattern> patterns = new();

        public HapticEditorViewModel()
        {
            // Split and initialize words
            var inputText = "This is a sample sentence with clickable words.\nC A M P F I R E Song.\n";
            Words = new ObservableCollection<WordModel>(
                inputText.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                         .Select(w => new WordModel(w))
            );
        }

        [RelayCommand]
        public void ShowDropdown(WordModel word)
        {

        }

        [RelayCommand]
        public void CreateHapticPattern()
        {
            if (VerifyValidInputs()) {
                HapticPattern pattern = new HapticPattern(HapticName, Color.FromArgb(HapticRed, HapticBlue, HapticGreen), HapticTemperature, HapticVibration);
                Patterns.Add(pattern);
            }
        }

        private bool VerifyValidInputs()
        {
            if (HapticName == "")
            {
                return false;
            }
            bool hasColor = !(HapticRed == 0 && HapticGreen == 0 && HapticBlue == 0);
            bool hasTemp = HapticTemperature != 0.0f;
            bool hasVibration = HapticVibration != 0.0f;
            return hasColor || hasTemp || hasVibration;
        }
    }
}
