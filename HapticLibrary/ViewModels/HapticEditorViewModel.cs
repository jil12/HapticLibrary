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
        [ObservableProperty]
        private bool isPatternEditing = true;
        [ObservableProperty]
        private bool isTextEditing = false;
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
        
        private int _selectedPatternIndex = -1;
        private bool _selectedPattern = false;
        private ReadingBook _readingBook = new();


        public HapticEditorViewModel()
        {
            if (_readingBook != null)
            {
                _readingBook.LoadBook("Assets/PropSampleBook.json");    //TODO: Create page to select what to load.
                PopulateWordsPanel();                                     // Split and initialize words
            }
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
        public void SelectWord(EditorWordViewModel editorWord)
        {
            //TODO: add haptic to the word in _readingBook, then populate to view
            //TODO: Need a way to visually convey multiple effects on a word.
            int index = words.IndexOf(editorWord);
            if (_selectedPattern)
            {
                string word = editorWord.Word;
                Dictionary<string, HapticEffect> effect = _readingBook.GetHaptics();
                //if (!effect.ContainsKey(word))
                //{
                effect[word] = new HapticEffect();  //TODO: Support multiple effect per word. Don't recreate everytime.
                effect[word].Props = new List<DotPropsJson>();
                //}
                DotPropsJson dotJson = Patterns[_selectedPatternIndex].ConvertToJson();
                effect[word].Props.Add(dotJson);

                PopulateWordsPanel();
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
                    editorWord.HapticPattern = new HapticPattern(triggerWords[editorWord.Word].Props[0]);
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
            IsPatternEditing = true;
            IsTextEditing = false;
        }
        [RelayCommand]
        private void EnableTextEditing()
        {
            IsTextEditing = true;
            IsPatternEditing = false;
        }

        [RelayCommand]
        private void NewPage()
        {
            _readingBook.AddPage();
            _readingBook.NextPage();
            PopulateWordsPanel();
        }

        [RelayCommand]
        private void ExportBook()
        {

        }
    }
}
