using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HapticLibrary.Models;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Datafeel;

namespace HapticLibrary.ViewModels
{
    public partial class ReadingPageViewModel : ViewModelBase, IPageViewModel
    {
        [ObservableProperty]
        private string _bookText = "";
        
        [ObservableProperty]
        private int _currentPage = 1;
        
        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private string _currentBookTitle = "Fahrenheit 451";

        [ObservableProperty]
        private string _currentChapter = "Chapter: The Chase";

        [ObservableProperty]
        private string _readingMode = "audio";

        [ObservableProperty]
        private bool _sidebarOpen = false;

        [ObservableProperty]
        private bool _isPlaying = false;

        [ObservableProperty]
        private string _playButtonText = "▶";

        [ObservableProperty]
        private string _playButtonIcon = "\uE768"; // Play icon (Segoe MDL2 Assets)

        [ObservableProperty]
        private TimeSpan _currentTime = TimeSpan.Zero;

        [ObservableProperty]
        private TimeSpan _totalTime = TimeSpan.Zero;

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
        
        // Audio playback properties
        private WaveOutEvent? _waveOut;
        private AudioFileReader? _audioFileReader;
        private System.Timers.Timer? _positionTimer;

        // Haptic sequence properties
        private HapticSequenceManager _hapticSequenceManager = new();
        private HashSet<string> _triggeredEvents = new();
        private bool _isAutoPageFlipEnabled = true;
        private HapticManager _hapticManager = HapticManager.GetInstance();
        private bool _isExplicitlyStopped = false; // Track if user stopped playback
        
        [ObservableProperty]
        private bool _autoPageFlipEnabled = true;
        
        [ObservableProperty]
        private string _currentHapticEvent = "";
        
        [ObservableProperty]
        private bool _hapticSequenceLoaded = false;
        
        [ObservableProperty]
        private bool _hapticHardwareConnected = false;
        
        [ObservableProperty]
        private string _debugOutput = "Debug: Ready...";
        
        private void UpdateDebugOutput(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            DebugOutput = $"[{timestamp}] {message}";
            System.Diagnostics.Debug.WriteLine(message);
        }

        public ReadingPageViewModel()
        {
            LoadBookContent();
            InitializeAudioStream();
            InitializeAudio();
            InitializeHapticSequence();
            InitializeHapticHardware();
        }

        private void LoadBookContent()
        {
            try
            {
                            // Load F451 content from F451_BookPages.json
            _readingBook.LoadBook("F451_BookPages");
                UpdatePageContent();
                TotalPages = _readingBook.GetLength();
                CurrentPage = 1;
                CurrentBookTitle = _readingBook.BookName;
                CurrentChapter = "Chapter: The Chase";
            }
            catch (Exception ex)
            {
                // Use default content if loading fails
                BookText = "Error loading F451 content: " + ex.Message;
                TotalPages = 1;
                CurrentPage = 1;
                CurrentBookTitle = "Error Loading Book";
                CurrentChapter = "Error";
            }
        }
        
        private string? FindFileInDirectory(string fileName, string directory)
        {
            try
            {
                string fullPath = Path.Combine(directory, fileName);
                if (File.Exists(fullPath))
                    return fullPath;

                // Search in subdirectories
                foreach (string subDir in Directory.GetDirectories(directory))
                {
                    string? found = FindFileInDirectory(fileName, subDir);
                    if (found != null)
                        return found;
                }
            }
            catch
            {
                // Ignore access errors
            }
            return null;
        }

        private void InitializeAudio()
        {
            try
            {
                // Find F451 Script Pt2.wav audio file
                string[] possibleAudioPaths = {
                    Path.Combine("Assets", "F451 Script Pt 2.wav"),
                    Path.Combine("HapticLibrary", "Assets", "F451 Script Pt 2.wav"),
                    Path.Combine("Assets", "F451 Script Pt2.wav"),
                    Path.Combine("HapticLibrary", "Assets", "F451 Script Pt2.wav"),
                    "F451 Script Pt 2.wav",
                    "F451 Script Pt2.wav"
                };
                
                string? foundAudioPath = null;
                foreach (string path in possibleAudioPaths)
                {
                    if (File.Exists(path))
                    {
                        foundAudioPath = path;
                        break;
                    }
                }
                
                // Search in subdirectories if not found
                if (foundAudioPath == null)
                {
                    string[] fileNames = { "F451 Script Pt 2.wav", "F451 Script Pt2.wav" };
                    foreach (string fileName in fileNames)
                    {
                        foundAudioPath = FindFileInDirectory(fileName, Directory.GetCurrentDirectory());
                        if (foundAudioPath != null) break;
                    }
                }
                
                if (foundAudioPath != null)
                {
                    _audioFileReader = new AudioFileReader(foundAudioPath);
                    _waveOut = new WaveOutEvent();
                    _waveOut.Init(_audioFileReader);
                    
                    TotalTime = _audioFileReader.TotalTime;
                    
                    // Setup timer to update position
                    _positionTimer = new System.Timers.Timer(100); // Update every 100ms
                    _positionTimer.Elapsed += (s, e) =>
                    {
                        if (_audioFileReader != null && _waveOut != null)
                        {
                            CurrentTime = _audioFileReader.CurrentTime;
                            
                            // Only process haptics if playing and not explicitly stopped
                            if (_waveOut.PlaybackState == PlaybackState.Playing && !_isExplicitlyStopped)
                            {
                                // Process haptic sequence and auto page flipping
                                if (HapticSequenceLoaded)
                                {
                                    _hapticSequenceManager.ProcessAudioPosition(CurrentTime);
                                    System.Diagnostics.Debug.WriteLine($"Processing haptic at time: {CurrentTime}");
                                }
                            }
                            else if (_waveOut.PlaybackState == PlaybackState.Stopped && !_isExplicitlyStopped)
                            {
                                // Audio ended naturally - stop haptics
                                UpdateDebugOutput("🔚 Audio ended - stopping haptics");
                                StopHapticSequence();
                                _isExplicitlyStopped = true; // Prevent further processing
                            }
                        }
                    };
                    _positionTimer.Start();
                }
                else
                {
                    Console.WriteLine("F451 Script Pt2.wav not found in expected locations");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing audio: {ex.Message}");
            }
        }
        
        private async void InitializeHapticHardware()
        {
            try
            {
                UpdateDebugOutput("🔌 Starting Datafeel haptic hardware...");
                bool success = await _hapticManager.StartManager();
                
                HapticHardwareConnected = _hapticManager.IsConnected;
                
                if (success && HapticHardwareConnected)
                {
                    UpdateDebugOutput($"✅ Hardware connected: {_hapticManager.DotManager.Dots.Count()} dots");
                    
                    // Test all dots with a brief flash
                    await TestAllDots();
                }
                else
                {
                    UpdateDebugOutput("❌ Failed to initialize haptic hardware");
                }
            }
            catch (Exception ex)
            {
                UpdateDebugOutput($"ERROR initializing hardware: {ex.Message}");
                HapticHardwareConnected = false;
            }
        }
        
        private async Task TestAllDots()
        {
            try
            {
                Console.WriteLine("Testing all dots (2-dot configuration)...");
                
                // Flash each dot briefly to confirm they're working
                for (int i = 1; i <= 2; i++)
                {
                    string deviceName = i == 1 ? "Wrists" : "Chest";
                    Console.WriteLine($"Testing Dot {i} ({deviceName})...");
                    
                    await _hapticManager.SetDotLED(i, 100, 100, 100); // White flash
                    await Task.Delay(200);
                    await _hapticManager.SetDotLED(i, 0, 0, 0); // Turn off
                    await Task.Delay(100);
                }
                
                Console.WriteLine("2-dot test completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing dots: {ex.Message}");
            }
        }
        
        private void InitializeHapticSequence()
        {
            try
            {
                // Load F451 haptic sequence
                string[] possiblePaths = {
                    Path.Combine("Assets", "F451_HapticSequence.json"),
                    Path.Combine("HapticLibrary", "Assets", "F451_HapticSequence.json"),
                    "F451_HapticSequence.json"
                };
                
                string? foundPath = null;
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        foundPath = path;
                        break;
                    }
                }
                
                if (foundPath == null)
                {
                    foundPath = FindFileInDirectory("F451_HapticSequence.json", Directory.GetCurrentDirectory());
                }
                
                if (foundPath != null)
                {
                    HapticSequenceLoaded = _hapticSequenceManager.LoadSequence(foundPath);
                    
                    if (HapticSequenceLoaded)
                    {
                        // Subscribe to haptic events
                        _hapticSequenceManager.HapticEventTriggered += OnHapticEventTriggered;
                        _hapticSequenceManager.PageShouldChange += OnPageShouldChange;
                        _hapticSequenceManager.ContinuousHapticTriggered += OnContinuousHapticTriggered;
                        
                        UpdateDebugOutput("✅ F451 sequence loaded with loop support");
                    }
                }
                else
                {
                    Console.WriteLine("F451_HapticSequence.json not found");
                    HapticSequenceLoaded = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing haptic sequence: {ex.Message}");
                HapticSequenceLoaded = false;
            }
        }
        
        private void OnHapticEventTriggered(HapticEvent hapticEvent)
        {
            try
            {
                // Avoid triggering the same event multiple times
                if (_triggeredEvents.Contains(hapticEvent.EventName))
                {
                    return;
                }
                
                _triggeredEvents.Add(hapticEvent.EventName);
                CurrentHapticEvent = hapticEvent.EventName;
                
                UpdateDebugOutput($"🎯 HAPTIC: {hapticEvent.EventName}");
                
                // Send haptic commands to hardware
                SendHapticEffects(hapticEvent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR processing haptic event: {ex.Message}");
            }
        }

        private void OnContinuousHapticTriggered(string effectType, string pattern)
        {
            try
            {
                if (!_hapticManager.IsStarted)
                {
                    return;
                }

                // Handle continuous patterns like the original code
                Task.Run(async () =>
                {
                    switch (effectType)
                    {
                        case "AmbulanceSiren":
                            if (pattern == "Red")
                            {
                                await _hapticManager.SetDotLED(1, 255, 0, 0); // Wrists red
                            }
                            else if (pattern == "White")
                            {
                                await _hapticManager.SetDotLED(1, 255, 255, 255); // Wrists white
                            }
                            break;
                            
                        case "HeartBeat":
                            // Chest heartbeat with red pulse like original
                            await _hapticManager.SetDotLED(2, 255, 100, 100);
                            await _hapticManager.SetDotVibration(2, 2.5f, 0.8f, true);
                            break;
                            
                        case "HarshGray":
                            if (pattern == "PulseUp")
                            {
                                await _hapticManager.SetDotLED(1, 128, 128, 128); // Gray wrists
                                await _hapticManager.SetDotLED(2, 128, 128, 128); // Gray chest
                                await _hapticManager.SetDotVibration(1, 30f, 0.3f, true); // Shaking hands
                            }
                            break;
                            
                        case "BlueShades":
                            if (pattern == "PulseUp")
                            {
                                await _hapticManager.SetDotLED(1, 0, 100, 255); // Blue wrists
                                await _hapticManager.SetDotLED(2, 0, 100, 255); // Blue chest
                            }
                            break;
                    }
                });
                
                UpdateDebugOutput($"🔄 {effectType}: {pattern}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in continuous haptic: {ex.Message}");
            }
        }
        
        private async void SendHapticEffects(HapticEvent hapticEvent)
        {
            try
            {
                if (!_hapticManager.IsStarted)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Haptic hardware not started - cannot send effects");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"🔨 Sending haptic effects for: {hapticEvent.EventName}");

                // IMPORTANT: Stop previous effects first (like original sequencer resetDots)
                await _hapticManager.StopAllEffects();
                await Task.Delay(50); // Small delay to ensure stop commands are processed

                // Map F451 haptic events to 2-dot configuration (Dot 1: Wrists, Dot 2: Chest)
                switch (hapticEvent.EventName)
                {
                    case "Event1_PoliceAnnouncement":
                        // Alternating red/white siren on wrists + chest vibration (16 second duration in original)
                        await _hapticManager.SetDotLED(1, 255, 0, 0); // Wrists red/white siren effect
                        await _hapticManager.SetDotVibration(2, 41.2f, 1.0f, true); // Chest unease vibration
                        break;
                        
                    case "Event2_Realization":
                        // Heartbeat effect on chest + thermal heating on wrists
                        await _hapticManager.SetDotLED(2, 255, 100, 100); // Chest red pulse
                        await _hapticManager.SetDotVibration(2, 2.5f, 0.8f, true); // Heartbeat frequency
                        await _hapticManager.SetDotThermal(1, 0.5f); // Wrists heating
                        break;
                        
                    // Individual countdown events - only flash device 1 (wrists) as requested
                    case "EventCounting_One":
                    case "EventCounting_Two":
                    case "EventCounting_Three":
                    case "EventCounting_Four":
                    case "EventCounting_Five":
                    case "EventCounting_Six":
                    case "EventCounting_Seven":
                    case "EventCounting_Eight":
                    case "EventCounting_Nine":
                    case "EventCounting_Ten":
                        // Random red flash on wrists only with proper intensity like original: dot1.VibrationIntensity = 1.0f
                        await _hapticManager.SetDotLED(1, 255, 0, 0); // Wrists red flash
                        await _hapticManager.SetDotVibration(1, 100f, 1.0f, true); // Strong click with intensity 1.0f
                        UpdateDebugOutput($"🔢 {hapticEvent.EventName.Replace("EventCounting_", "")}!");
                        break;
                        
                    case "Event5_GreyFaces":
                        // Gray pulsing + hand shaking + unease vibration
                        await _hapticManager.SetDotLED(1, 128, 128, 128); // Wrists gray
                        await _hapticManager.SetDotLED(2, 128, 128, 128); // Chest gray
                        await _hapticManager.SetDotVibration(1, 30f, 0.3f, true); // Wrists shaking
                        await _hapticManager.SetDotVibration(2, 41.2f, 1.0f, true); // Chest unease
                        break;
                        
                    case "Event6_River":
                        // Blue pulsing + cold thermal + relief vibration
                        await _hapticManager.SetDotLED(1, 0, 100, 255); // Wrists blue
                        await _hapticManager.SetDotLED(2, 0, 100, 255); // Chest blue
                        await _hapticManager.SetDotThermal(1, -1.0f); // Cold wrists
                        await _hapticManager.SetDotVibration(2, 52.13f, 0.3f, true); // Chest relief
                        break;
                        
                    case "Event7_FinalEscape":
                        // Just cold wrists, everything else stops
                        await _hapticManager.SetDotThermal(1, -1.0f); // Keep wrists cold
                        // Turn off everything else
                        await _hapticManager.SetDotLED(1, 0, 0, 0);
                        await _hapticManager.SetDotLED(2, 0, 0, 0);
                        await _hapticManager.SetDotVibration(2, 0f, 0f, false);
                        break;
                        
                    default:
                        System.Diagnostics.Debug.WriteLine($"❌ Unknown haptic event: {hapticEvent.EventName}");
                        break;
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ Haptic effect sent for event: {hapticEvent.EventName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR sending haptic effects: {ex.Message}");
            }
        }
        
        private void OnPageShouldChange(int newPage)
        {
            try
            {
                if (_isAutoPageFlipEnabled && newPage >= 1 && newPage <= TotalPages && newPage != CurrentPage)
                {
                    // Update the page in the reading book model
                    while (_readingBook.PageIndex + 1 < newPage && _readingBook.PageIndex < TotalPages - 1)
                    {
                        _readingBook.NextPage();
                    }
                    while (_readingBook.PageIndex + 1 > newPage && _readingBook.PageIndex > 0)
                    {
                        _readingBook.PreviousPage();
                    }
                    
                    // Update the UI
                    UpdatePageContent();
                    
                    Console.WriteLine($"Auto-flipped to page {newPage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing page: {ex.Message}");
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
        public void TogglePlayback()
        {
            if (_waveOut == null || _audioFileReader == null)
            {
                System.Diagnostics.Debug.WriteLine("Audio not initialized");
                return;
            }

            if (_waveOut.PlaybackState == PlaybackState.Playing)
            {
                System.Diagnostics.Debug.WriteLine("=== PAUSING PLAYBACK ===");
                _waveOut.Pause();
                IsPlaying = false;
                PlayButtonText = "▶";
                PlayButtonIcon = "\uE768"; // Play icon
            }
            else
            {
                UpdateDebugOutput("▶️ STARTING PLAYBACK");
                _isExplicitlyStopped = false; // Reset stop flag when starting
                _waveOut.Play();
                IsPlaying = true;
                PlayButtonText = "⏸";
                PlayButtonIcon = "\uE769"; // Pause icon
                
                // Reset triggered events when starting playback to allow re-triggering
                if (_audioFileReader.Position == 0)
                {
                    _triggeredEvents.Clear();
                    CurrentHapticEvent = "";
                    System.Diagnostics.Debug.WriteLine("Reset triggered events for fresh playback");
                }
            }
        }
        
        [RelayCommand]
        public void ToggleAutoPageFlip()
        {
            AutoPageFlipEnabled = !AutoPageFlipEnabled;
            _isAutoPageFlipEnabled = AutoPageFlipEnabled;
            Console.WriteLine($"Auto page flip: {(AutoPageFlipEnabled ? "Enabled" : "Disabled")}");
        }
        
        [RelayCommand]
        public async Task TestHapticDots()
        {
            if (!_hapticManager.IsStarted)
            {
                Console.WriteLine("Haptic hardware not connected - cannot test");
                return;
            }
            
            Console.WriteLine("Testing haptic dots manually...");
            
            try
            {
                // Test sequence: Each dot lights up and vibrates briefly (2-dot configuration)
                string[] deviceNames = { "", "Wrists", "Chest" }; // Index 0 unused
                
                for (int i = 1; i <= 2; i++)
                {
                    Console.WriteLine($"Testing Dot {i} ({deviceNames[i]})...");
                    
                    // Flash LED and vibrate
                    await _hapticManager.SetDotLED(i, 255, 100, 0); // Orange
                    await _hapticManager.SetDotVibration(i, 50f, 0.8f, true); // Medium vibration
                    
                    await Task.Delay(800); // Hold for 800ms
                    
                    // Turn off
                    await _hapticManager.SetDotLED(i, 0, 0, 0);
                    await _hapticManager.SetDotVibration(i, 0f, 0f, false);
                    
                    await Task.Delay(200); // Brief pause
                }
                
                Console.WriteLine("2-dot haptic test completed");
                
                // Final flash - both dots green briefly
                await _hapticManager.SetDotLED(1, 0, 255, 0);
                await _hapticManager.SetDotLED(2, 0, 255, 0);
                
                await Task.Delay(500);
                
                await _hapticManager.StopAllEffects();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during haptic test: {ex.Message}");
            }
        }

        [RelayCommand]
        public void StopPlayback()
        {
            if (_waveOut != null && _audioFileReader != null)
            {
                UpdateDebugOutput("🛑 STOP BUTTON PRESSED");
                _isExplicitlyStopped = true; // Set flag BEFORE stopping audio
                
                // Update connection status
                HapticHardwareConnected = _hapticManager.IsConnected;
                
                _waveOut.Stop();
                _audioFileReader.Position = 0;
                CurrentTime = TimeSpan.Zero;
                IsPlaying = false;
                PlayButtonText = "▶";
                PlayButtonIcon = "\uE768"; // Play icon
                
                // Stop haptic sequence and reset
                StopHapticSequence();
                System.Diagnostics.Debug.WriteLine("=== STOP COMPLETED ===");
            }
        }
        
        private async void StopHapticSequence()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("--- Stopping Haptic Sequence ---");
                
                // Reset triggered events so they can be triggered again on next playback
                _triggeredEvents.Clear();
                CurrentHapticEvent = "";
                System.Diagnostics.Debug.WriteLine("Cleared triggered events");
                
                // Stop all haptic effects on physical hardware
                if (_hapticManager.IsStarted)
                {
                    await _hapticManager.StopAllEffects();
                    System.Diagnostics.Debug.WriteLine("Stopped all physical haptic effects");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Haptic manager not started - no physical effects to stop");
                }
                
                // Also stop the sequence manager
                if (HapticSequenceLoaded)
                {
                    _hapticSequenceManager.StopAllHaptics();
                    System.Diagnostics.Debug.WriteLine("Stopped haptic sequence manager");
                }
                
                System.Diagnostics.Debug.WriteLine("--- Haptic sequence stopped and reset ---");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR stopping haptic sequence: {ex.Message}");
            }
        }

        [RelayCommand]
        public void SeekBackward()
        {
            if (_audioFileReader != null)
            {
                var newTime = _audioFileReader.CurrentTime - TimeSpan.FromSeconds(10);
                if (newTime < TimeSpan.Zero) newTime = TimeSpan.Zero;
                _audioFileReader.CurrentTime = newTime;
                CurrentTime = newTime;
            }
        }

        [RelayCommand]
        public void SeekForward()
        {
            if (_audioFileReader != null)
            {
                var newTime = _audioFileReader.CurrentTime + TimeSpan.FromSeconds(10);
                if (newTime > _audioFileReader.TotalTime) newTime = _audioFileReader.TotalTime;
                _audioFileReader.CurrentTime = newTime;
                CurrentTime = newTime;
            }
        }

        [RelayCommand]
        public void ResetAudio()
        {
            try
            {
                UpdateDebugOutput("🔄 RESETTING audio and sequencer...");
                
                // Stop playback if playing
                if (IsPlaying)
                {
                    _waveOut?.Stop();
                    IsPlaying = false;
                    PlayButtonText = "▶";
                    PlayButtonIcon = "\uE768"; // Play icon
                }
                
                // Reset audio position to beginning
                if (_audioFileReader != null)
                {
                    _audioFileReader.Position = 0;
                    CurrentTime = TimeSpan.Zero;
                }
                
                // Reset haptic sequencer and stop all effects
                _isExplicitlyStopped = true; // Set flag to stop any running effects
                StopHapticSequence();
                
                // Clear all triggered events so they can be triggered again
                _triggeredEvents.Clear();
                CurrentHapticEvent = "";
                
                // Reset page to first page
                if (_readingBook != null)
                {
                    // Navigate to first page using PreviousPage method
                    while (_readingBook.PageIndex > 0)
                    {
                        _readingBook.PreviousPage();
                    }
                    CurrentPage = _readingBook.PageIndex + 1;
                    UpdatePageContent();
                }
                
                // Update connection status
                HapticHardwareConnected = _hapticManager.IsConnected;
                
                UpdateDebugOutput("✅ RESET complete - ready to start");
            }
            catch (Exception ex)
            {
                UpdateDebugOutput($"ERROR during reset: {ex.Message}");
            }
        }

        public void SeekToPosition(TimeSpan position)
        {
            if (_audioFileReader != null)
            {
                if (position < TimeSpan.Zero) position = TimeSpan.Zero;
                if (position > _audioFileReader.TotalTime) position = _audioFileReader.TotalTime;
                _audioFileReader.CurrentTime = position;
                CurrentTime = position;
            }
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

        // Text selection commands
        [RelayCommand]
        public void OnTextSelectionChanged(string selectedText)
        {
            SelectedText = selectedText ?? "";
            HasTextSelected = !string.IsNullOrWhiteSpace(SelectedText);
        }

        public void OnSelectionChanged(int start, int end)
        {
            SelectionStart = start;
            SelectionEnd = end;
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
                StopPlayback();
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

        // Cleanup resources
        public void Dispose()
        {
            _positionTimer?.Stop();
            _positionTimer?.Dispose();
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _audioFileReader?.Dispose();
        }
    }
} 