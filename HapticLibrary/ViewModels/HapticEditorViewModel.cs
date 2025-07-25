using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HapticLibrary.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Datafeel;
using Newtonsoft.Json.Bson;

namespace HapticLibrary.ViewModels
{
    public partial class HapticEditorViewModel : ViewModelBase, IPageViewModel
    {
        [ObservableProperty]    //TODO: Convert to enum
        private bool isPatternEditing = true;
        [ObservableProperty]
        private bool isTextEditing = false;
        [ObservableProperty]
        private bool isPatternSelected = false;
        [ObservableProperty]
        private string editableText = "";

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

        [ObservableProperty]
        private int _selectedPatternIndex = -1;
        [ObservableProperty]
        private HapticPattern _selectedPattern = null;
        [ObservableProperty]
        private bool _eraseSelected = false;
        private ReadingBook _readingBook = new();


        public HapticEditorViewModel()
        {
            if (_readingBook != null)
            {
                _readingBook.LoadBook("Assets/PropSampleBook.json");    //TODO: Create page to select what to load.
                PopulateWordsPanel();                                     // Split and initialize words
            }

            patterns.Add(new HapticPattern("green", Color.FromArgb(106, 159, 58), 0, 0));
            patterns.Add(new HapticPattern("water", Color.FromArgb(56, 173, 250), -1, 0));
            patterns.Add(new HapticPattern("white", Color.FromArgb(255, 255, 255), 0, 0));
            patterns.Add(new HapticPattern("vibrate", Color.FromArgb(0, 0, 0), 0, 1));
            patterns.Add(new HapticPattern("rang", Color.FromArgb(106, 159, 58), 0, 1));
            patterns.Add(new HapticPattern("morning", Color.FromArgb(255, 254, 133), 0.5f, 0));
            patterns.Add(new HapticPattern("breeze", Color.FromArgb(0, 0, 0), 0, 1));
            patterns.Add(new HapticPattern("brown", Color.FromArgb(139, 69, 19), 0, 0));
            patterns.Add(new HapticPattern("shade", Color.FromArgb(50, 50, 50), -1, 0));
            patterns.Add(new HapticPattern("splashing", Color.FromArgb(56, 173, 250), 0, 1));
            patterns.Add(new HapticPattern("lorax", Color.FromArgb(139, 69, 19), 0, 0));
            patterns.Add(new HapticPattern("trees", Color.FromArgb(106, 159, 58), 0, 0));
            patterns.Add(new HapticPattern("fruits", Color.FromArgb(106, 159, 58), 0, 0));

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
            EraseSelected = false;
            int index = Patterns.IndexOf(pattern);

            if (SelectedPatternIndex != -1 && index == SelectedPatternIndex)
            {
                SelectedPatternIndex = -1;
                SelectedPattern = null;
            } else
            {
                SelectedPatternIndex = index;
                SelectedPattern = pattern;
                PopulateHapticPropertiesPanel(pattern);
            }
        }

        [RelayCommand]
        public void SelectWord(EditorWordViewModel editorWord)
        {
            //TODO: add haptic to the word in _readingBook, then populate to view
            //TODO: Need a way to visually convey multiple effects on a word.
            int index = words.IndexOf(editorWord);
            if (_selectedPatternIndex != -1)
            {
                string word = editorWord.Word;
                editorWord.HapticPattern = SelectedPattern;
                Dictionary<string, HapticEffect> effect = _readingBook.GetHaptics();
                //if (!effect.ContainsKey(word))
                //{
                effect[word] = new HapticEffect();  //TODO: Support multiple effect per word. Don't recreate everytime.
                effect[word].Props = new List<DotPropsJson>();
                //}
                DotPropsJson dotJson = Patterns[_selectedPatternIndex].ConvertToJson();
                effect[word].Props.Add(dotJson);

                PopulateWordsPanel();
            } else if (_eraseSelected)
            {
                string word = editorWord.Word;
                editorWord.HapticPattern = null;
                Dictionary<string, HapticEffect> effect = _readingBook.GetHaptics();
                effect.Remove(word);
                PopulateWordsPanel();
            } else if (editorWord.HapticPattern != null)
            {
                SelectPattern(editorWord.HapticPattern);
            }
        }

        [RelayCommand]
        public void NextPage()
        {
            _readingBook.NextPage();
            PopulateWordsPanel();
        }

        [RelayCommand]
        public void PrevPage()
        {
            _readingBook.PreviousPage();
            PopulateWordsPanel();

            //TODO: need to process haptics assigned.
        }

        private void PopulateWordsPanel()
        {
            Words = new ObservableCollection<EditorWordViewModel>(
                    _readingBook.GetText().Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                             .Select(w => new EditorWordViewModel(w))
                );
            Dictionary<string, HapticEffect> triggerWords = _readingBook.GetHaptics();
            foreach (var editorWord in Words)
            {
                if (triggerWords.ContainsKey(editorWord.Word))
                {
                    editorWord.HapticPattern = new HapticPattern(triggerWords[editorWord.Word].Props[0]);   //TODO: Since this overrides hapticPattern, the name is lost.
                }
            }
            EditableText = _readingBook.GetText();
        }

        [RelayCommand]
        private void SaveText()
        {
            _readingBook.SetText(EditableText);
            _readingBook.SetHaptics(new Dictionary<string, HapticEffect>());
            PopulateWordsPanel();
        }

        [RelayCommand]
        private void EnablePatternEditing()
        {
            ResetHapticProperties();
            IsPatternEditing = true;
            IsTextEditing = false;
            IsPatternSelected = false;
        }
        [RelayCommand]
        private void EnableTextEditing()
        {
            ResetHapticProperties();
            IsTextEditing = true;
            IsPatternEditing = false;
            IsPatternSelected = false;
        }

        [RelayCommand]
        private void NewPage()
        {
            _readingBook.AddPage();
            _readingBook.NextPage();
            PopulateWordsPanel();
        }

        [RelayCommand]
        private void SelectErase()
        {
            EraseSelected = true;
            SelectedPattern = null;
            SelectedPatternIndex = -1;
        }

        [RelayCommand]
        private void ExportBook()
        {

        }

        private void ResetHapticProperties()
        {
            HapticName = "";
            HapticRed = 0;
            HapticGreen = 0;
            HapticBlue = 0;
            HapticTemperature = 0f;
            HapticVibration = 0f;
        }

        private void PopulateHapticPropertiesPanel(HapticPattern hapticPattern)
        {
            IsTextEditing = false;
            IsPatternEditing = false;
            IsPatternSelected = true;
            HapticName = hapticPattern.Name;
            HapticRed = hapticPattern.Color.R;
            HapticGreen = hapticPattern.Color.G;
            HapticBlue = hapticPattern.Color.B;
            HapticTemperature = hapticPattern.Temperature;
            HapticVibration = hapticPattern.Vibration;
        }
    }
}
