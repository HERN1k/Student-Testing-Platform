using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

using TestingPlatform.Domain.Interfaces;

namespace TestingPlatform.Services.Audio
{
    public sealed partial class AudioCaptureService : IAudioCaptureService
    {
        [LibraryImport("winmm.dll", EntryPoint = "waveInOpen")]
        private static partial int WaveInOpen(out IntPtr hWaveIn, int uDeviceID, WaveFormat lpFormat, WaveDelegate? dwCallback, int dwInstance, int dwFlags);

        [LibraryImport("winmm.dll", EntryPoint = "waveInPrepareHeader")]
        private static partial int WaveInPrepareHeader(IntPtr hWaveIn, ref WaveHeader pwh, int cbwh);

        [LibraryImport("winmm.dll", EntryPoint = "waveInAddBuffer")]
        private static partial int WaveInAddBuffer(IntPtr hWaveIn, ref WaveHeader pwh, int cbwh);

        [LibraryImport("winmm.dll", EntryPoint = "waveInStart")]
        private static partial int WaveInStart(IntPtr hWaveIn);

        [LibraryImport("winmm.dll", EntryPoint = "waveInStop")]
        private static partial int WaveInStop(IntPtr hWaveIn);

        [LibraryImport("winmm.dll", EntryPoint = "waveInReset")]
        private static partial int WaveInReset(IntPtr hWaveIn);

        [LibraryImport("winmm.dll", EntryPoint = "waveInClose")]
        private static partial int WaveInClose(IntPtr hWaveIn);

        private delegate void WaveDelegate(IntPtr hdrvr, int uMsg, int dwUser, int dwParam1, int dwParam2);

        private readonly ILogger<AudioCaptureService> _logger;

        private readonly WaveFormat _waveFormat = new()
        {
            wFormatTag = 1,
            nChannels = 1,
            nSamplesPerSec = 44100,
            nAvgBytesPerSec = 44100 * 2,
            nBlockAlign = 2,
            wBitsPerSample = 16
        };

        private readonly WavHeader _wavHeader = new()
        {
            HeaderSize = 44,
            SubChunk1Size = 16,
            AudioFormat = 1,
            NumChannels = 1,
            SampleRate = 44100,
            BitsPerSample = 16,
            ByteRate = 44100 * 1 * 16 / 8,
            BlockAlign = 1 * 16 / 8
        };

        private readonly string _tempFilePath = Path.Combine(Path.GetTempPath(), "AudioData.wav");

        private const int _recordingTime = 3;
#pragma warning disable IDE0290
        public AudioCaptureService(ILogger<AudioCaptureService> logger)
#pragma warning restore
        {
            ValidateConstructorArguments(logger);
            _logger = logger;
        }

        private static void ValidateConstructorArguments(ILogger<AudioCaptureService> logger)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        }

        private async Task<string> WindowsRecordImplAsync()
        {
            IntPtr hWaveIn = IntPtr.Zero;
            GCHandle handle = default;

            try
            {
                if (WaveInOpen(out hWaveIn, -1, _waveFormat, null, 0, 0) != 0)
                {
                    var ex = new InvalidOperationException("Error opening microphone");
                    _logger.LogError(ex, "Error opening microphone");
                    throw ex;
                }

                var buffer = new byte[44100 * 2 * _recordingTime];
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var header = new WaveHeader
                {
                    lpData = handle.AddrOfPinnedObject(),
                    dwBufferLength = buffer.Length,
                    dwFlags = 0,
                    dwLoops = 1
                };
#pragma warning disable CA1806
                WaveInPrepareHeader(hWaveIn, ref header, Marshal.SizeOf(header));
                WaveInAddBuffer(hWaveIn, ref header, Marshal.SizeOf(header));
                if (WaveInStart(hWaveIn) != 0)
                {
                    var ex = new InvalidOperationException("Error record audio");
                    _logger.LogError(ex, "Error record audio");
                    throw ex;
                }

                _logger.LogInformation("Record...");
                await Task.Delay(1000 * _recordingTime);

                WaveInStop(hWaveIn);
                WaveInReset(hWaveIn);
                WaveInClose(hWaveIn);

                await SaveToWavFileAsync(buffer);

                handle.Free();

                return _tempFilePath;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
                _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
                return string.Empty;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }

                if (hWaveIn != IntPtr.Zero)
                {
                    WaveInClose(hWaveIn);
#pragma warning restore
                }
            }
        }

        private async Task SaveToWavFileAsync(byte[] audioData)
        {
            ArgumentNullException.ThrowIfNull(audioData, nameof(audioData));
            ArgumentException.ThrowIfNullOrEmpty(_tempFilePath, nameof(_tempFilePath));

            await using var fs = new FileStream(_tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            using var writer = new BinaryWriter(fs);

            // Writing the header of the WAV file
            await fs.WriteAsync("RIFF"u8.ToArray());
            await fs.WriteAsync(BitConverter.GetBytes(_wavHeader.HeaderSize - 8 + audioData.Length));
            await fs.WriteAsync("WAVE"u8.ToArray());

            // We write the subordinate block "fmt "
            await fs.WriteAsync("fmt "u8.ToArray());
            await fs.WriteAsync(BitConverter.GetBytes(_wavHeader.SubChunk1Size));
            await fs.WriteAsync(BitConverter.GetBytes(_wavHeader.AudioFormat));
            await fs.WriteAsync(BitConverter.GetBytes(_wavHeader.NumChannels));
            await fs.WriteAsync(BitConverter.GetBytes(_wavHeader.SampleRate));
            await fs.WriteAsync(BitConverter.GetBytes(_wavHeader.ByteRate));
            await fs.WriteAsync(BitConverter.GetBytes(_wavHeader.BlockAlign));
            await fs.WriteAsync(BitConverter.GetBytes(_wavHeader.BitsPerSample));

            // We write the data block "data"
            await fs.WriteAsync("data"u8.ToArray());
            await fs.WriteAsync(BitConverter.GetBytes(audioData.Length));
            await fs.WriteAsync(audioData);
        }
    }
}