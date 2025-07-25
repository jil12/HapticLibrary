using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using HapticLibrary.ViewModels;
using System;
using System.Threading;

namespace HapticLibrary.Views
{
    public partial class ReadingPageView : UserControl
    {
        private ProgressBar? _audioProgressBar;
        private bool _isDragging = false;

        public ReadingPageView()
        {
            InitializeComponent();
            SetupProgressBarDrag();
        }

        private void SetupProgressBarDrag()
        {
            _audioProgressBar = this.FindControl<ProgressBar>("AudioProgressBar");
            if (_audioProgressBar != null)
            {
                _audioProgressBar.PointerPressed += OnProgressBarPointerPressed;
                _audioProgressBar.PointerMoved += OnProgressBarPointerMoved;
                _audioProgressBar.PointerReleased += OnProgressBarPointerReleased;
            }
        }

        private void OnProgressBarPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_audioProgressBar != null && DataContext is ReadingPageViewModel viewModel)
            {
                _isDragging = true;
                UpdateProgressFromPointer(e, viewModel);
                e.Pointer.Capture(_audioProgressBar);
            }
        }

        private void OnProgressBarPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging && _audioProgressBar != null && DataContext is ReadingPageViewModel viewModel)
            {
                UpdateProgressFromPointer(e, viewModel);
            }
        }

        private void OnProgressBarPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_audioProgressBar != null)
            {
                _isDragging = false;
                e.Pointer.Capture(null);
            }
        }

        private void UpdateProgressFromPointer(PointerEventArgs e, ReadingPageViewModel viewModel)
        {
            if (_audioProgressBar == null) return;

            var point = e.GetPosition(_audioProgressBar);
            var progress = point.X / _audioProgressBar.Bounds.Width;
            
            // Clamp progress between 0 and 1
            progress = Math.Max(0, Math.Min(1, progress));
            
            // Calculate new time based on progress and seek to it
            var newTimeSeconds = progress * viewModel.TotalTime.TotalSeconds;
            var newTime = TimeSpan.FromSeconds(newTimeSeconds);
            viewModel.SeekToPosition(newTime);
        }
    }
}