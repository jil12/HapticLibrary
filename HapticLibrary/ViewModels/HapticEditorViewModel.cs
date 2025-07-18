using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HapticLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HapticLibrary.ViewModels
{
    public partial class HapticEditorViewModel : ViewModelBase, IPageViewModel
    {
        [ObservableProperty]
        private ObservableCollection<WordModel> words;

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
    }
}
