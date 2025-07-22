using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Datafeel;

namespace HapticLibrary.Models
{
    public class HapticSequenceData
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("DeviceMapping")]
        public Dictionary<string, string> DeviceMapping { get; set; } = new();
        
        [JsonPropertyName("AudioFile")]
        public string AudioFile { get; set; } = string.Empty;
        
        [JsonPropertyName("TotalDuration")]
        public string TotalDuration { get; set; } = string.Empty;
        
        [JsonPropertyName("HapticSequence")]
        public List<HapticEvent> HapticSequence { get; set; } = new();
        
        [JsonPropertyName("ResetSequence")]
        public JsonElement ResetSequence { get; set; }
    }

    public class HapticEvent
    {
        [JsonPropertyName("EventName")]
        public string EventName { get; set; } = string.Empty;
        
        [JsonPropertyName("StartTime")]
        public string StartTime { get; set; } = string.Empty;
        
        [JsonPropertyName("EndTime")]
        public string EndTime { get; set; } = string.Empty;
        
        [JsonPropertyName("Duration")]
        public int Duration { get; set; }
        
        [JsonPropertyName("TextSection")]
        public string TextSection { get; set; } = string.Empty;
        
        [JsonPropertyName("HapticEffects")]
        public JsonElement HapticEffects { get; set; }
        
        // Helper properties for easier access
        public TimeSpan StartTimeSpan => ParseTimeString(StartTime);
        public TimeSpan EndTimeSpan => ParseTimeString(EndTime);
        
        private static TimeSpan ParseTimeString(string timeStr)
        {
            // Handle decimal seconds format (m:ss.f or mm:ss.f)
            if (TimeSpan.TryParseExact(timeStr, @"m\:ss\.f", null, out TimeSpan result))
                return result;
            if (TimeSpan.TryParseExact(timeStr, @"mm\:ss\.f", null, out result))
                return result;
            // Handle standard format (m:ss or mm:ss)
            if (TimeSpan.TryParseExact(timeStr, @"m\:ss", null, out result))
                return result;
            if (TimeSpan.TryParseExact(timeStr, @"mm\:ss", null, out result))
                return result;
            
            // Debug output for parsing issues
            System.Diagnostics.Debug.WriteLine($"Failed to parse time string: '{timeStr}'");
            return TimeSpan.Zero;
        }
    }

    public class HapticSequenceManager
    {
        private HapticSequenceData? _sequenceData;
        private List<HapticEvent> _events = new();
        private Dictionary<TimeSpan, int> _pageTransitions = new();
        private Dictionary<string, DateTime> _activeLoops = new(); // Track active looping events
        private Dictionary<string, CancellationTokenSource> _loopCancellationTokens = new();
        // Track triggered one-time events with timestamps to prevent duplicate triggers
        private Dictionary<string, TimeSpan> _triggeredOneTimeEvents = new();
        private bool _hasStoppedAt112 = false;

        public event Action<HapticEvent>? HapticEventTriggered;
        public event Action<int>? PageShouldChange;
        public event Action<string, string>? ContinuousHapticTriggered; // For looping effects

        public bool LoadSequence(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    // Try to find the file in subdirectories
                    string? foundPath = FindFileInDirectory(Path.GetFileName(filePath), Directory.GetCurrentDirectory());
                    if (foundPath != null)
                        filePath = foundPath;
                    else
                        return false;
                }

                string jsonContent = File.ReadAllText(filePath);
                _sequenceData = JsonSerializer.Deserialize<HapticSequenceData>(jsonContent);
                
                if (_sequenceData?.HapticSequence != null)
                {
                    _events = _sequenceData.HapticSequence;
                    SetupPageTransitions();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading haptic sequence: {ex.Message}");
            }
            
            return false;
        }

        private void SetupPageTransitions()
        {
            // Map audio timing to page transitions based on F451 content structure (2-dot configuration)
            _pageTransitions.Clear();
            
            // Page 1: Police Announcement (0:00 - 0:16)
            _pageTransitions[TimeSpan.Zero] = 1;
            
            // Page 2: Realization & Countdown Start (0:16 - 0:45)
            _pageTransitions[TimeSpan.FromSeconds(28)] = 2;
            
            // Page 3: Escape & Countdown End (0:45 - 1:12)
            _pageTransitions[TimeSpan.FromSeconds(46)] = 3;
            
            // Page 4: River & Final Escape (1:12 - end)
            _pageTransitions[TimeSpan.FromSeconds(72)] = 4;
        }

        private void TurnOffAllLEDs()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Turning off all LEDs");
                var dotManager = HapticManager.GetInstance().DotManager;
                foreach (var dot in dotManager.Dots)
                {
                    dot.LedMode = LedModes.GlobalManual;
                    dot.GlobalLed.Red = 0;
                    dot.GlobalLed.Green = 0;
                    dot.GlobalLed.Blue = 0;
                    dot.Write();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error turning off LEDs: {ex.Message}");
            }
        }

        public void ProcessAudioPosition(TimeSpan currentTime)
        {
            // Turn off all LEDs at 112.4 seconds, but keep other effects running
            if (!_hasStoppedAt112 && currentTime >= TimeSpan.FromSeconds(112.4))
            {
                TurnOffAllLEDs();
                _hasStoppedAt112 = true;
                System.Diagnostics.Debug.WriteLine("🛑 All LEDs turned off at 112.4s");
            }

            // After 112.4s, do not trigger any further continuous haptic events (blue/gray swells)
            bool allowLedEvents = currentTime < TimeSpan.FromSeconds(112.4);

            // Check for haptic events to trigger
            foreach (var hapticEvent in _events)
            {
                var startTime = hapticEvent.StartTimeSpan;
                var endTime = hapticEvent.EndTimeSpan;
                
                // Handle EventCounting events as one-time triggers at start time
                if (hapticEvent.EventName.StartsWith("EventCounting_"))
                {
                    // Check if we're at the start time (within 150ms window to account for timer precision)
                    if (currentTime >= startTime && currentTime <= startTime.Add(TimeSpan.FromMilliseconds(150)))
                    {
                        // Check if we've already triggered this specific event at this time
                        if (!_triggeredOneTimeEvents.ContainsKey(hapticEvent.EventName) || 
                            Math.Abs((_triggeredOneTimeEvents[hapticEvent.EventName] - startTime).TotalMilliseconds) > 500)
                        {
                            _triggeredOneTimeEvents[hapticEvent.EventName] = startTime;
                            System.Diagnostics.Debug.WriteLine($"🔢 Triggering one-time event: {hapticEvent.EventName} at {currentTime}");
                            
                            // Trigger the event for immediate effect
                            HapticEventTriggered?.Invoke(hapticEvent);
                        }
                    }
                }
                // Handle other events as continuous effects
                else if (currentTime >= startTime && currentTime <= endTime)
                {
                    // Only allow LED events before 112.4s
                    if (!allowLedEvents && (hapticEvent.EventName == "Event5_GreyFaces" || hapticEvent.EventName == "Event6_River"))
                        continue;
                    // Check if this is a new event that needs to start
                    if (!_activeLoops.ContainsKey(hapticEvent.EventName))
                    {
                        _activeLoops[hapticEvent.EventName] = DateTime.Now;
                        System.Diagnostics.Debug.WriteLine($"🔄 Starting continuous effect: {hapticEvent.EventName}");
                        
                        // Start continuous/looping effects
                        StartContinuousHapticEffect(hapticEvent, currentTime);
                    }
                }
                else if (currentTime > endTime && _activeLoops.ContainsKey(hapticEvent.EventName))
                {
                    // Event has ended, stop continuous effects
                    StopContinuousHapticEffect(hapticEvent.EventName);
                    _activeLoops.Remove(hapticEvent.EventName);
                    System.Diagnostics.Debug.WriteLine($"⏹️ Stopped continuous effect: {hapticEvent.EventName}");
                }
            }

            // Check for page transitions
            foreach (var transition in _pageTransitions)
            {
                if (currentTime >= transition.Key && currentTime < transition.Key.Add(TimeSpan.FromSeconds(1)))
                {
                    PageShouldChange?.Invoke(transition.Value);
                    break;
                }
            }
        }

        private void StartContinuousHapticEffect(HapticEvent hapticEvent, TimeSpan currentTime)
        {
            try
            {
                // Cancel any existing loop for this event
                StopContinuousHapticEffect(hapticEvent.EventName);
                
                var cts = new CancellationTokenSource();
                _loopCancellationTokens[hapticEvent.EventName] = cts;
                
                // Trigger the event to start the effect
                HapticEventTriggered?.Invoke(hapticEvent);
                
                // Start continuous effect based on event type
                Task.Run(async () =>
                {
                    var duration = hapticEvent.EndTimeSpan - hapticEvent.StartTimeSpan;
                    System.Diagnostics.Debug.WriteLine($"Running continuous effect {hapticEvent.EventName} for {duration.TotalSeconds}s");
                    
                    // Run continuous patterns like original code
                    await RunContinuousPattern(hapticEvent, duration, cts.Token);
                }, cts.Token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting continuous haptic effect: {ex.Message}");
            }
        }

        private void StopContinuousHapticEffect(string eventName)
        {
            try
            {
                if (_loopCancellationTokens.TryGetValue(eventName, out var cts))
                {
                    cts.Cancel();
                    _loopCancellationTokens.Remove(eventName);
                    System.Diagnostics.Debug.WriteLine($"Cancelled continuous effect: {eventName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping continuous effect {eventName}: {ex.Message}");
            }
        }

        private async Task RunContinuousPattern(HapticEvent hapticEvent, TimeSpan duration, CancellationToken cancellationToken)
        {
            try
            {
                // This method will run the continuous patterns like the original code
                switch (hapticEvent.EventName)
                {
                    case "Event1_PoliceAnnouncement":
                        // Run ambulance siren pattern (30 loops of 250ms)
                        await RunAmbulanceSirenPattern(duration, cancellationToken);
                        break;
                        
                    case "Event2_Realization":
                        // Run heartbeat pattern (78 loops)
                        await RunHeartBeatPattern(duration, cancellationToken);
                        break;
                        
                    case "Event5_GreyFaces":
                        // Run harsh gray pulsing and shaking hands
                        await RunHarshGrayPattern(duration, cancellationToken);
                        break;
                        
                    case "Event6_River":
                        // Run blue shades pulsing
                        await RunBlueShadesPattern(duration, cancellationToken);
                        break;
                        
                    default:
                        // For simple events, just wait the duration
                        await Task.Delay(duration, cancellationToken);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine($"Continuous pattern {hapticEvent.EventName} was cancelled");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in continuous pattern {hapticEvent.EventName}: {ex.Message}");
            }
        }

        // Placeholder methods for different patterns - these will trigger specific hardware commands
        private async Task RunAmbulanceSirenPattern(TimeSpan duration, CancellationToken cancellationToken)
        {
            var endTime = DateTime.Now + duration;
            while (DateTime.Now < endTime && !cancellationToken.IsCancellationRequested)
            {
                ContinuousHapticTriggered?.Invoke("AmbulanceSiren", "Red");
                await Task.Delay(250, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
                
                ContinuousHapticTriggered?.Invoke("AmbulanceSiren", "White");
                await Task.Delay(250, cancellationToken);
            }
        }

        private async Task RunHeartBeatPattern(TimeSpan duration, CancellationToken cancellationToken)
        {
            var endTime = DateTime.Now + duration;
            while (DateTime.Now < endTime && !cancellationToken.IsCancellationRequested)
            {
                ContinuousHapticTriggered?.Invoke("HeartBeat", "Pulse");
                await Task.Delay(500, cancellationToken); // 150 BPM like original
            }
        }

        private async Task RunHarshGrayPattern(TimeSpan duration, CancellationToken cancellationToken)
        {
            var endTime = DateTime.Now + duration;
            while (DateTime.Now < endTime && !cancellationToken.IsCancellationRequested)
            {
                // Complete one full swelling cycle (up + down) like original HarshGray() 
                ContinuousHapticTriggered?.Invoke("HarshGray", "PulseUp");
                // Wait for swelling up to complete (41 steps * 50ms = ~2050ms) 
                await Task.Delay(2100, cancellationToken); 
                if (cancellationToken.IsCancellationRequested) break;
                
                ContinuousHapticTriggered?.Invoke("HarshGray", "PulseDown");
                // Wait for swelling down to complete (41 steps * 50ms = ~2050ms)
                await Task.Delay(2100, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
                
                // Brief pause between cycles like original (for 2 loops)
                await Task.Delay(500, cancellationToken);
            }
        }

        private async Task RunBlueShadesPattern(TimeSpan duration, CancellationToken cancellationToken)
        {
            var endTime = DateTime.Now + duration;
            while (DateTime.Now < endTime && !cancellationToken.IsCancellationRequested)
            {
                // Complete one full swelling cycle (up + down) like original BlueShades()
                ContinuousHapticTriggered?.Invoke("BlueShades", "PulseUp");
                // Wait for swelling up to complete (41 steps * 50ms = ~2050ms)
                await Task.Delay(2100, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
                
                ContinuousHapticTriggered?.Invoke("BlueShades", "PulseDown");  
                // Wait for swelling down to complete (41 steps * 50ms = ~2050ms)
                await Task.Delay(2100, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
                
                // Brief pause between cycles like original (for 2 loops)
                await Task.Delay(50, cancellationToken);
            }
        }

        public void ResetTriggeredEvents()
        {
            try
            {
                _triggeredOneTimeEvents.Clear();
                _hasStoppedAt112 = false;
                System.Diagnostics.Debug.WriteLine("🔄 Reset triggered one-time events for retrigger");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting triggered events: {ex.Message}");
            }
        }

        public void StopAllHaptics()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🛑 Stopping all haptic loops and effects");
                
                // Cancel all active loops
                foreach (var kvp in _loopCancellationTokens.ToArray())
                {
                    StopContinuousHapticEffect(kvp.Key);
                }
                
                _activeLoops.Clear();
                _loopCancellationTokens.Clear();
                
                // Clear triggered one-time events so they can be retriggered if needed
                _triggeredOneTimeEvents.Clear();
                System.Diagnostics.Debug.WriteLine("🔄 Cleared triggered one-time events for potential retrigger");
                
                if (_sequenceData?.ResetSequence != null)
                {
                    System.Diagnostics.Debug.WriteLine("Resetting all haptic devices");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping haptics: {ex.Message}");
            }
        }

        private string? FindFileInDirectory(string fileName, string directory)
        {
            try
            {
                string fullPath = Path.Combine(directory, fileName);
                if (File.Exists(fullPath))
                    return fullPath;

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

        public HapticSequenceData? SequenceData => _sequenceData;
        public List<HapticEvent> Events => _events;
    }
} 