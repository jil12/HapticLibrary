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
                //.UseSerialPort("COM6") // Uncomment this line to specify the serial port by name
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
                System.Diagnostics.Debug.WriteLine("Starting Datafeel DotManager...");
                using var cts = new CancellationTokenSource(10000); // 10 second timeout like original
                var result = await _dotManager.Start(_datafeelModbusClient, cts.Token);
                _isStarted = result;
                
                if (result)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Datafeel DotManager started successfully");
                    System.Diagnostics.Debug.WriteLine($"Connected to {_dotManager.Dots.Count()} dots");
                    
                    // Verify dots are responsive like in original
                    foreach (var dot in _dotManager.Dots)
                    {
                        System.Diagnostics.Debug.WriteLine($"Dot {dot.Address} connected");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Failed to start Datafeel DotManager - no hardware connection");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR starting Datafeel DotManager: {ex.Message}");
                _isStarted = false;
                return false;
            }
        }

        public async Task SetDotLED(int address, byte red, byte green, byte blue)
        {
            if (!_isStarted)
            {
                Console.WriteLine("DotManager not started - cannot set LED");
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
                    Console.WriteLine($"Set Dot {address} LED to RGB({red}, {green}, {blue})");
                }
                else
                {
                    Console.WriteLine($"Dot with address {address} not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting LED for dot {address}: {ex.Message}");
            }
        }

        public async Task SetDotVibration(int address, float frequency, float intensity, bool enable)
        {
            if (!_isStarted)
            {
                Console.WriteLine("DotManager not started - cannot set vibration");
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
                    Console.WriteLine($"Set Dot {address} vibration: {frequency}Hz, intensity {intensity}, {(enable ? "ON" : "OFF")}");
                }
                else
                {
                    Console.WriteLine($"Dot with address {address} not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting vibration for dot {address}: {ex.Message}");
            }
        }

        public async Task SetDotThermal(int address, float intensity)
        {
            if (!_isStarted)
            {
                Console.WriteLine("DotManager not started - cannot set thermal");
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
                    Console.WriteLine($"Set Dot {address} thermal intensity: {intensity}");
                }
                else
                {
                    Console.WriteLine($"Dot with address {address} not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting thermal for dot {address}: {ex.Message}");
            }
        }
        
        public async Task StopDotVibration(int address)
        {
            if (!_isStarted)
            {
                System.Diagnostics.Debug.WriteLine("DotManager not started - cannot stop vibration");
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
                    System.Diagnostics.Debug.WriteLine($"Stopped vibration for Dot {address}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Dot with address {address} not found");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping vibration for dot {address}: {ex.Message}");
            }
        }

        public async Task StopAllEffects()
        {
            if (!_isStarted)
            {
                System.Diagnostics.Debug.WriteLine("DotManager not started - cannot stop effects");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("Resetting all dots...");
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
                        System.Diagnostics.Debug.WriteLine($"Reset Dot {dot.Address}");
                    }
                    catch (Exception writeEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error writing to dot {dot.Address}: {writeEx.Message}");
                    }
                }
                System.Diagnostics.Debug.WriteLine("All haptic effects stopped and reset");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping haptic effects: {ex.Message}");
            }
        }
    }
}
