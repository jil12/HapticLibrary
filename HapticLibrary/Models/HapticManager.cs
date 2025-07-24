using Datafeel;
using Datafeel.NET.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HapticLibrary.Models
{
    /**
     * Singleton class, stores Datafeel DotManager
     */
    public sealed class HapticManager
    {
        private static HapticManager? _instance;
        private DotManager _dotManager;
        private DatafeelModbusClient _datafeelModbusClient;
        private bool _isStarted = false;
        private Dictionary<int, CancellationTokenSource> _heartbeatCancellationTokens = new();
        
        public DotManager DotManager { get { return _dotManager; } }
        public bool IsStarted => _isStarted;
        
        public bool IsConnected 
        { 
            get 
            { 
                return _isStarted && _dotManager?.Dots?.Any() == true; 
            } 
        }

        private HapticManager() 
        {
            _dotManager = new DotManagerConfiguration()
                .AddDot<Dot_63x_xxx>(1)  // Wrists (Combined)
                .AddDot<Dot_63x_xxx>(2)  // Chest (Combined)
                .CreateDotManager();

            _datafeelModbusClient = new DatafeelModbusClientConfiguration()
                .UseWindowsSerialPortTransceiver()
                //.UseSerialPort("COM3") // Uncomment this line to specify the serial port by name
                .CreateClient();
        }

        public static HapticManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new HapticManager();
            }
            return _instance;
        }

        public async Task<bool> StartManager()
        {
            try
            {
                using var cts = new CancellationTokenSource(10000); // 10 second timeout like original
                var result = await _dotManager.Start(_datafeelModbusClient, cts.Token);
                _isStarted = result;
                
                return result;
            }
            catch
            {
                _isStarted = false;
                return false;
            }
        }

        public async Task SetDotLED(int address, byte red, byte green, byte blue)
        {
            if (!_isStarted)
            {
                return;
            }

            try
            {
                var dot = _dotManager.Dots.FirstOrDefault(d => d.Address == address);
                if (dot != null)
                {
                    dot.LedMode = LedModes.GlobalManual;
                    dot.GlobalLed.Red = red;
                    dot.GlobalLed.Green = green;
                    dot.GlobalLed.Blue = blue;
                    
                    await dot.Write();
                }
            }
            catch
            {
                // Silent error handling
            }
        }

        public async Task SetDotVibration(int address, float frequency, float intensity, bool enable)
        {
            if (!_isStarted)
            {
                return;
            }

            try
            {
                var dot = _dotManager.Dots.FirstOrDefault(d => d.Address == address);
                if (dot != null)
                {
                    dot.VibrationMode = VibrationModes.Manual;
                    dot.VibrationFrequency = frequency;
                    dot.VibrationIntensity = intensity;
                    dot.VibrationGo = enable;
                    
                    await dot.Write();
                }
            }
            catch
            {
                // Silent error handling
            }
        }

        public async Task SetDotThermal(int address, float intensity)
        {
            if (!_isStarted)
            {
                return;
            }

            try
            {
                var dot = _dotManager.Dots.FirstOrDefault(d => d.Address == address);
                if (dot != null)
                {
                    dot.ThermalMode = ThermalModes.Manual;
                    dot.ThermalIntensity = intensity;
                    
                    await dot.Write();
                }
            }
            catch
            {
                // Silent error handling
            }
        }
        
        public async Task StopDotVibration(int address)
        {
            if (!_isStarted)
            {
                return;
            }

            try
            {
                var dot = _dotManager.Dots.FirstOrDefault(d => d.Address == address);
                if (dot != null)
                {
                    dot.VibrationMode = VibrationModes.Manual;
                    dot.VibrationFrequency = 0;
                    dot.VibrationIntensity = 0;
                    dot.VibrationGo = false;
                    
                    await dot.Write();
                }
            }
            catch
            {
                // Silent error handling
            }
        }

        public async Task StopAllEffects()
        {
            if (!_isStarted)
            {
                return;
            }

            try
            {
                foreach (var dot in _dotManager.Dots)
                {
                    // Reset thermal (must be first)
                    dot.ThermalMode = ThermalModes.Manual;
                    dot.ThermalIntensity = 0;
                    
                    // Turn off LEDs
                    dot.LedMode = LedModes.GlobalManual;
                    dot.GlobalLed.Red = 0;
                    dot.GlobalLed.Green = 0;
                    dot.GlobalLed.Blue = 0;
                    
                    // Turn off vibration (exactly like original resetDots)
                    dot.VibrationMode = VibrationModes.Manual;
                    dot.VibrationFrequency = 0;
                    dot.VibrationIntensity = 0;
                    dot.VibrationGo = false;
                    
                    try
                    {
                        await dot.Write();
                    }
                    catch
                    {
                        // Silent error handling
                    }
                }
            }
            catch
            {
                // Silent error handling
            }
        }

        public async Task StartHeartBeat(int address)
        {
            if (!_isStarted)
            {
                return;
            }

            try
            {
                var dot = _dotManager.Dots.FirstOrDefault(d => d.Address == address);
                if (dot != null)
                {
                    int bpm = 150;
                    double beatRate = 60.0 / bpm;
                    int singleBeatDelay = 100;
                    double betweenBeatOffset = beatRate - .1;

                    dot.VibrationMode = VibrationModes.Library;
                    dot.VibrationSequence[0].Waveforms = VibrationWaveforms.SoftBumpP100;
                    dot.VibrationSequence[1].RestDuration = singleBeatDelay; // Milliseconds
                    dot.VibrationSequence[2].Waveforms = VibrationWaveforms.SoftBumpP30;
                    dot.VibrationSequence[3].RestDuration = (int)(betweenBeatOffset * 1000); // Milliseconds
                    dot.VibrationSequence[4].Waveforms = VibrationWaveforms.EndSequence;

                    dot.LedMode = LedModes.GlobalManual;
                    dot.VibrationGo = true; // Actually start the vibration sequence

                    await dot.Write();
                }
            }
            catch
            {
                // Silent error handling
            }
        }

        public async Task RunHeartBeatLoop(int address, int beatCount = 76)
        {
            if (!_isStarted)
            {
                return;
            }

            try
            {
                var dot = _dotManager.Dots.FirstOrDefault(d => d.Address == address);
                if (dot != null)
                {
                    // Setup heartbeat sequence first
                    await StartHeartBeat(address);

                    // Setup cancellation
                    if (_heartbeatCancellationTokens.ContainsKey(address))
                    {
                        _heartbeatCancellationTokens[address].Cancel();
                        _heartbeatCancellationTokens.Remove(address);
                    }
                    var cts = new CancellationTokenSource();
                    _heartbeatCancellationTokens[address] = cts;
                    var token = cts.Token;

                    for (int i = 0; i < beatCount; i++)
                    {
                        if (token.IsCancellationRequested) break;
                        dot.VibrationGo = false;
                        for (byte brightness = 241; brightness > 20; brightness -= 20)
                        {
                            dot.GlobalLed.Red = brightness;
                            await dot.Write();
                        }
                        dot.VibrationGo = true;
                        await dot.Write();
                        await Task.Delay(500);
                    }
                    // Remove token after loop
                    _heartbeatCancellationTokens.Remove(address);
                }
            }
            catch
            {
                // Silent error handling
            }
        }

        public void CancelHeartBeat(int address)
        {
            if (_heartbeatCancellationTokens.TryGetValue(address, out var cts))
            {
                cts.Cancel();
                _heartbeatCancellationTokens.Remove(address);
            }
        }
    }
}
