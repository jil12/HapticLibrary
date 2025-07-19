using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HapticLibrary.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HapticLibrary.ViewModels
{
    public partial class ReadingPageViewModel : ViewModelBase, IPageViewModel
    {
        [ObservableProperty]
        private string? _bookLine;
        
        [ObservableProperty]
        private string? _bookLine2;
        
        [ObservableProperty]
        private string? _bookLine3;
        
        [ObservableProperty]
        private string _bookText;
        
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

        // Read-aloud properties
        [ObservableProperty]
        private bool _isReadAloudMode = false;

        [ObservableProperty]
        private bool _isRecording = false;

        [ObservableProperty]
        private string _recordingStatus = "Ready to record";

        // Text selection properties
        [ObservableProperty]
        private string _selectedText = "";

        [ObservableProperty]
        private bool _hasTextSelected = false;

        [ObservableProperty]
        private int _selectionStart = 0;

        [ObservableProperty]
        private int _selectionEnd = 0;

        private ReadingBook _readingBook = new ReadingBook();
        private ReadingModeAudioStream _audioStream = ReadingModeAudioStream.Instance;

        public ReadingPageViewModel()
        {
            LoadBookContent();
            InitializeAudioStream();
        }

        private void LoadBookContent()
        {
            try
            {
                _readingBook.LoadBook("HapticReadingBookExample");
                UpdatePageContent();
                TotalPages = _readingBook.GetLength();
                CurrentPage = 1;
                CurrentBookTitle = _readingBook.BookName;
            }
            catch
            {
                // Use default content if loading fails
                TotalPages = 5;
                CurrentPage = 1;
                CurrentBookTitle = "Error Loading Book";
            }
        }

        private async void InitializeAudioStream()
        {
            try
            {
                await _audioStream.Connect();
                await _audioStream.SendHapticInteractions(_readingBook.GetHaptics());
                
                // Subscribe to status changes
                _audioStream.StatusChanged += OnAudioStreamStatusChanged;
            }
            catch (Exception ex)
            {
                RecordingStatus = $"Connection error: {ex.Message}";
            }
        }

        private void OnAudioStreamStatusChanged(string status)
        {
            RecordingStatus = status;
        }

        private async void UpdatePageContent()
        {
            string fullText = _readingBook.GetText();
            BookText = fullText;
            CurrentPage = _readingBook.PageIndex + 1;
            
            // Update book title with page info
            CurrentBookTitle = $"{_readingBook.BookName} - Page {CurrentPage}";
            
            // Send haptic interactions for current page
            try
            {
                await _audioStream.SendHapticInteractions(_readingBook.GetHaptics());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending haptic interactions: {ex.Message}");
            }
        }

        [RelayCommand]
        public async void PreviousPage()
        {
            _readingBook.PreviousPage();
            UpdatePageContent();
        }

        [RelayCommand]
        public async void NextPage()
        {
            _readingBook.NextPage();
            UpdatePageContent();
        }

        [RelayCommand]
        public void ToggleSidebar()
        {
            SidebarOpen = !SidebarOpen;
        }

        // Text selection commands
        [RelayCommand]
        public void OnTextSelectionChanged(string selectedText)
        {
            SelectedText = selectedText ?? "";
            HasTextSelected = !string.IsNullOrWhiteSpace(SelectedText);
            
            // For debugging - you can see the selected text in the console
            if (HasTextSelected)
            {
                Console.WriteLine($"Selected text: '{SelectedText}'");
            }
        }

        public void OnSelectionChanged(int start, int end)
        {
            SelectionStart = start;
            SelectionEnd = end;
            Console.WriteLine($"Selection changed: {start} to {end}");
        }

        // Read-aloud commands
        [RelayCommand]
        public void SetAudioMode()
        {
            IsReadAloudMode = false;
            ReadingMode = "audio";
            // Stop recording if active
            if (IsRecording)
            {
                StopRecording();
            }
        }

        [RelayCommand]
        public void SetReadAloudMode()
        {
            IsReadAloudMode = true;
            ReadingMode = "read-aloud";
            // Stop audio if playing
            if (IsPlaying)
            {
                IsPlaying = false;
            }
        }

        [RelayCommand]
        public void ToggleRecording()
        {
            IsRecording = !IsRecording;
            if (IsRecording)
            {
                RecordingStatus = "Recording...";
                // TODO: Start recording backend call
            }
            else
            {
                RecordingStatus = "Paused";
                // TODO: Pause recording backend call
            }
        }

        [RelayCommand]
        public void StopRecording()
        {
            IsRecording = false;
            RecordingStatus = "Ready to record";
            // TODO: Stop recording backend call
        }

        // Enhanced microphone toggle with backend integration
        [RelayCommand]
        public void ToggleMicrophone()
        {
            _audioStream.ToggleRecording();
            IsRecording = _audioStream.Recording;
            RecordingStatus = IsRecording ? "🎤 Recording..." : "Ready to record";
            
            // Update read-aloud mode based on recording state
            IsReadAloudMode = IsRecording;
            
            // Notify UI of changes
            OnPropertyChanged(nameof(IsRecording));
            OnPropertyChanged(nameof(RecordingStatus));
        }

        // Async initialization method for external use
        public async Task LoadAsync()
        {
            _readingBook.LoadBook("");
            BookText = _readingBook.GetText();
            CurrentPage = _readingBook.PageIndex + 1;
            CurrentBookTitle = _readingBook.BookName;
            
            await _audioStream.Connect();
            await _audioStream.SendHapticInteractions(_readingBook.GetHaptics());
        }
    }
} 