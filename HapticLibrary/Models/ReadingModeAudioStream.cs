using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace HapticLibrary.Models
{
    public class ReadingModeAudioStream
    {
        private static readonly Lazy<ReadingModeAudioStream> _instance = new(() => new ReadingModeAudioStream());
        public static ReadingModeAudioStream Instance => _instance.Value;

        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private WaveInEvent _waveIn;

        private ReadingModeAudioStream() { }

        public string Status { get; private set; } = "Disconnected";

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
                await _webSocket.ConnectAsync(new Uri("ws://localhost:8001"), _cancellationTokenSource.Token);
                SetStatus("Connected");

                StartStreaming();
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
            SetStatus("Streaming audio...");
        }

        private void StopStreaming()
        {
            _waveIn?.StopRecording();
            _waveIn?.Dispose();
            _waveIn = null;
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
    }
}
