using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HapticLibrary.Models;
using System;

namespace HapticLibrary.ViewModels
{
    public partial class ReadingPageViewModel : ViewModelBase, IPageViewModel
    {
        [ObservableProperty]
        private string? _bookLine = "The old library stood silent in the moonlight, its ancient stone walls holding centuries of knowledge within.";
        
        [ObservableProperty]
        private string? _bookLine2 = "Sarah approached the heavy oak doors with a mixture of excitement and trepidation. She had waited years for this moment.";
        
        [ObservableProperty]
        private string? _bookLine3 = "The brass doorknob felt cold against her palm as she turned it slowly, the mechanism clicking softly in the quiet night.";
        
        [ObservableProperty]
        private int _currentPage = 1;
        
        [ObservableProperty]
        private int _totalPages = 5;

        [ObservableProperty]
        private string _currentBookTitle = "The Adventure Begins";

        [ObservableProperty]
        private string _currentChapter = "Chapter 1: The Beginning";

        [ObservableProperty]
        private string _readingMode = "audio";

        [ObservableProperty]
        private bool _sidebarOpen = false;

        [ObservableProperty]
        private bool _isPlaying = false;

        [ObservableProperty]
        private TimeSpan _currentTime = TimeSpan.FromSeconds(135);

        [ObservableProperty]
        private TimeSpan _totalTime = TimeSpan.FromSeconds(330);

        [ObservableProperty]
        private double _playbackSpeed = 1.0;

        [ObservableProperty]
        private bool _vibrationEnabled = true;

        [ObservableProperty]
        private double _vibrationIntensity = 70;

        private ReadingBook _readingBook = new ReadingBook();

        public ReadingPageViewModel()
        {
            LoadBookContent();
        }

        private void LoadBookContent()
        {
            try
            {
                _readingBook.LoadBook("HapticReadingBookExample");
                UpdatePageContent();
                TotalPages = _readingBook.GetLength();
                CurrentPage = 1;
            }
            catch
            {
                // Use default content if loading fails
                TotalPages = 5;
                CurrentPage = 1;
            }
        }

        private void UpdatePageContent()
        {
            string fullText = _readingBook.GetText();
            
            // Split the text into lines for display
            string[] lines = fullText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            BookLine = lines.Length > 0 ? lines[0] : "";
            BookLine2 = lines.Length > 1 ? lines[1] : "";
            BookLine3 = lines.Length > 2 ? lines[2] : "";
            
            CurrentPage = _readingBook.PageIndex + 1;
        }

        [RelayCommand]
        public void PreviousPage()
        {
            _readingBook.PreviousPage();
            UpdatePageContent();
        }

        [RelayCommand]
        public void NextPage()
        {
            _readingBook.NextPage();
            UpdatePageContent();
        }

        [RelayCommand]
        public void ToggleSidebar()
        {
            SidebarOpen = !SidebarOpen;
        }
    }
} 