using System;
using CommunityToolkit.Mvvm.ComponentModel;
using HapticLibrary.Models;

namespace HapticLibrary.ViewModels
{
    public partial class EditorWordViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _word;
        [ObservableProperty]
        private HapticPattern? _hapticPattern;

        public EditorWordViewModel(string word)
        {
            Word = word;
            HapticPattern = null;
        }

        public EditorWordViewModel(string word, HapticPattern hapticPattern)
        {
            Word = word;
            HapticPattern = hapticPattern;
        }
    }

}
