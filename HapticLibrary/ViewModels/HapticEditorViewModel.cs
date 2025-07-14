using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HapticLibrary.ViewModels
{
    public partial class HapticEditorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _title;
        [ObservableProperty]
        private string _author;
        [ObservableProperty]
        private string _audioFile;

        [ObservableProperty]
        private PageDraft _pageDraft;
    }

    public partial class PageDraft : ObservableObject
    {
        public string Text = "";
        public string StartTime = "";
        public string EndTime = "";
    }

    public partial class HapticDraft : ObservableObject
    {
        public string Type = "";
        public string Time = "";
        public string Intensity = "";
    }
}
