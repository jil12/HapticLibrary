using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using NAudio.Wave;

namespace HapticLibrary.ViewModels
{
    public partial class AudioStreamViewModel : ViewModelBase, IPageViewModel
    {
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private WaveInEvent _waveIn;

        [ObservableProperty]
        private string _status = "Disconnected";

        [RelayCommand]
        public async Task Connect()
        {
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                Status = "Connecting...";
                await _webSocket.ConnectAsync(new Uri("ws://localhost:8001"), _cancellationTokenSource.Token);
                Status = "Connected";

                StartStreaming(); // ⏺ Start streaming immediately
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
            }
        }

        private void StartStreaming()
        {
            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(16000, 16, 1), // 16kHz, 16-bit mono PCM
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
            Status = "Streaming audio...";
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
                Status = $"Send error: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task Disconnect()
        {
            StopStreaming();

            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
            }

            _webSocket?.Dispose();
            _webSocket = null;

            Status = "Disconnected";
        }
    }
}
