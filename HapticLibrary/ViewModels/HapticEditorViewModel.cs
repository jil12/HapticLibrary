using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HapticLibrary.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Drawing;

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
        private ObservableCollection<EditorWordViewModel> words = new();
        [ObservableProperty]
        private ObservableCollection<HapticPattern> patterns = new();
        
        private int _selectedPatternIndex = -1;
        private bool _selectedPattern = false;


        public HapticEditorViewModel()
        {
            // Split and initialize words
            var inputText = "This is a sample sentence with clickable words.\nC A M P F I R E Song.\n";
            Words = new ObservableCollection<EditorWordViewModel>(
                inputText.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                         .Select(w => new EditorWordViewModel(w))
            );
        }

        [RelayCommand]
        public void CreateHapticPattern()
        {
            if (VerifyValidInputs()) {
                HapticPattern pattern = new HapticPattern(HapticName, Color.FromArgb(HapticRed, HapticGreen, HapticBlue), HapticTemperature, HapticVibration);
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

        [RelayCommand]
        public void SelectPattern(HapticPattern pattern)
        {
            _selectedPatternIndex = Patterns.IndexOf(pattern);
            _selectedPattern = _selectedPatternIndex != -1;
        }

        [RelayCommand]
        public void SelectWord(EditorWordViewModel word)
        {
            int index = words.IndexOf(word);
            if (_selectedPattern)
            {
                word.HapticPattern = Patterns[_selectedPatternIndex];
                Words[index] = word;
            }
        }
    }
}
