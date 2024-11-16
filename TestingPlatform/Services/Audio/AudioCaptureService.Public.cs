using System.Runtime.InteropServices;

namespace TestingPlatform.Services.Audio
{
    public sealed partial class AudioCaptureService
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct WaveFormat
        {
            public Int16 wFormatTag;
            public Int16 nChannels;
            public Int32 nSamplesPerSec;
            public Int32 nAvgBytesPerSec;
            public Int16 nBlockAlign;
            public Int16 wBitsPerSample;
            public Int16 cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WaveHeader
        {
            public IntPtr lpData;
            public Int32 dwBufferLength;
            public Int32 dwBytesRecorded;
            public IntPtr dwUser;
            public Int32 dwFlags;
            public Int32 dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
        }

        public struct WavHeader
        {
            public Int32 HeaderSize;
            public Int32 SubChunk1Size;
            public Int16 AudioFormat;
            public Int16 NumChannels;
            public Int32 SampleRate;
            public Int16 BitsPerSample;
            public Int32 ByteRate;
            public Int16 BlockAlign;
        }

        public async Task<string> RecordAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                return await WindowsRecordImplAsync();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}