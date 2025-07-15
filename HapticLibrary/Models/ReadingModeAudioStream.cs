using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Datafeel;
using NAudio.Wave;

namespace HapticLibrary.Models
{
    public class ReadingModeAudioStream
    {
        private static List<int> ConvertFlagsToAddresses(int flags)
        {
            var addresses = new List<int>();
            int position = 1;

            while (flags > 0)
            {
                if ((flags & 1) == 1)
                {
                    addresses.Add(position);
                }

                flags >>= 1;
                position++;
            }

            return addresses;
        }

        private static readonly Lazy<ReadingModeAudioStream> _instance = new(() => new ReadingModeAudioStream());
        public static ReadingModeAudioStream Instance => _instance.Value;

        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private WaveInEvent _waveIn;

        private ReadingModeAudioStream() { }

        public string Status { get; private set; } = "Disconnected";
        private bool _recording = false;
        public bool Recording 
        {
            get
            {
                return _recording;
            }
        }

        public event Action<string> StatusChanged;

        private void SetStatus(string status)
        {
            Status = status;
            StatusChanged?.Invoke(status);
        }

        public async Task Connect()
        {
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                SetStatus("Connecting...");
                await _webSocket.ConnectAsync(new Uri("ws://localhost:8080"), _cancellationTokenSource.Token);
                SetStatus("Connected");

                //StartStreaming();
                _ = ReceiveLoop(); // Fire-and-forget (or await if you prefer blocking)
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
            }
        }

        private void StartStreaming()
        {
            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(16000, 16, 1),
                BufferMilliseconds = 100
            };

            _waveIn.DataAvailable += async (s, a) =>
            {
                if (_webSocket?.State == WebSocketState.Open)
                {
                    byte[] chunk = a.Buffer[..a.BytesRecorded];
                    await SendAudioChunk(chunk, 16000);
                }
            };
            
            _waveIn.StartRecording();
            SetStatus("Recording");
        }

        private void StopStreaming()
        {
            _waveIn?.StopRecording();
            _waveIn?.Dispose();
            _waveIn = null;
            SetStatus("Recording Stopped");
        }

        private async Task SendAudioChunk(byte[] audioData, int sampleRate)
        {
            try
            {
                var metadata = new { sampleRate };
                var metadataJson = JsonSerializer.Serialize(metadata);
                var metadataBytes = Encoding.UTF8.GetBytes(metadataJson);
                var metadataLength = BitConverter.GetBytes(metadataBytes.Length);

                byte[] message = new byte[4 + metadataBytes.Length + audioData.Length];
                Buffer.BlockCopy(metadataLength, 0, message, 0, 4);
                Buffer.BlockCopy(metadataBytes, 0, message, 4, metadataBytes.Length);
                Buffer.BlockCopy(audioData, 0, message, 4 + metadataBytes.Length, audioData.Length);

                await _webSocket.SendAsync(
                    new ArraySegment<byte>(message),
                    WebSocketMessageType.Binary,
                    true,
                    _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                SetStatus($"Send error: {ex.Message}");
            }
        }

        public async Task SendHapticInteractions(Dictionary<string, HapticEffect> data, CancellationToken cancellationToken = default)
        {
            if (_webSocket == null)
                throw new ArgumentNullException(nameof(_webSocket));

            if (_webSocket.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket is not open.");

            string json = JsonSerializer.Serialize(data);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            await _webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                cancellationToken);
        }

        public async Task Disconnect()
        {
            StopStreaming();

            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
            }

            _webSocket?.Dispose();
            _webSocket = null;

            SetStatus("Disconnected");
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[4096];

            try
            {
                while (_webSocket?.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        try
                        {
                            HapticEffect command = JsonSerializer.Deserialize<HapticEffect>(json);
                            Console.WriteLine($"Received command: {command.Props[0].Address}");
                            HapticManager hapticManager = HapticManager.GetInstance();

                            List<int> addresses = ConvertFlagsToAddresses(command.Props[0].Address);
                            foreach (int address in addresses)
                            {
                                DotPropsWritable dotProps = new DotPropsWritable();
                                dotProps.CopyFrom(command.Props[0]);
                                dotProps.Address = (byte) address;
                                await hapticManager.DotManager.Write(dotProps);
                            }
                            // Optionally: raise an event or trigger haptic behavior
                        }
                        catch (JsonException ex)
                        {
                            SetStatus($"Invalid JSON: {ex.Message}");
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        SetStatus("Server closed connection.");
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed", CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Receive error: {ex.Message}");
            }
        }

        public void ToggleRecording()
        {
            if (_recording)
            {
                StopStreaming();
            }
            else
            {
                StartStreaming();
            }
            _recording = !_recording;
        }
    }
}
