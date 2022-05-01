// libsoundio C# thin wrapper class library
// https://github.com/keijiro/jp.keijiro.libsoundio

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace SoundIO
{
    // SoundIoOutStream struct representation (used in write-callback)
    [StructLayout(LayoutKind.Sequential)]
    public struct OutStreamData
    {
        #region Struct data members

        internal IntPtr device;
        internal Format format;
        internal int sampleRate;
        internal ChannelLayout layout;
        internal double softwareLatency;
        internal float volume;
        internal IntPtr userData;
        internal IntPtr writeCallback;
        internal IntPtr underflowCallback;
        internal IntPtr errorCallback;
        internal IntPtr name;
        internal byte nonTerminalHint;
        internal int bytesPerFrame;
        internal int bytesPerSample;
        internal Error layoutError;

        #endregion
        
        #region Struct member accessors

        public Format Format => format;
        public int SampleRate => sampleRate;
        public ChannelLayout Layout => layout;
        public double SoftwareLatency => softwareLatency;
        public float Volume => volume;
        public IntPtr UserData => userData;
        public string Name => Marshal.PtrToStringAnsi(name);
        public bool NonTerminalHint => nonTerminalHint != 0;
        public int BytesPerFrame => bytesPerFrame;
        public int BytesPerSample => bytesPerSample;

        #endregion

        #region Data reader methods

        public unsafe Error BeginWrite(out ChannelArea* areas, ref int frameCount)
        {
            return _BeginWrite(ref this, out areas, ref frameCount);
        }

        public Error EndWrite()
        {
            return _EndWrite(ref this);
        }

        #endregion

        #region Unmanaged functions

        [DllImport(Config.DllName, EntryPoint="soundio_outstream_begin_write")]
        unsafe extern static Error _BeginWrite
            (ref OutStreamData stream, out ChannelArea* areas, ref int frameCount);

        [DllImport(Config.DllName, EntryPoint="soundio_outstream_end_write")]
        extern static Error _EndWrite(ref OutStreamData stream);

        #endregion
    }
    
    // SoundIoOutStream struct wrapper class
    public class OutStream : SafeHandleZeroOrMinusOneIsInvalid
    {
        #region SafeHandle implementation

        OutStream() : base(true) {}

        protected override bool ReleaseHandle()
        {
            _Destroy(this.handle);
            return true;
        }

        unsafe ref OutStreamData Data => ref Unsafe.AsRef<OutStreamData>((void*)handle);

        #endregion

        #region Struct member accessors

        public Format Format
        {
            get => Data.format;
            set => Data.format = value;
        }

        public int SampleRate
        {
            get => Data.sampleRate;
            set => Data.sampleRate = value;
        }

        public ChannelLayout Layout
        {
            get => Data.layout;
            set => Data.layout = value;
        }

        public double SoftwareLatency
        {
            get => Data.softwareLatency;
            set => Data.softwareLatency = value;
        }

        public float Volume
        {
            get => Data.volume;
            set => Data.volume = value;
        }

        public IntPtr UserData
        {
            get => Data.userData;
            set => Data.userData = value;
        }

        public delegate void WriteCallbackDelegate(ref OutStreamData stream, int frameCountMin, int frameCountMax);

        public WriteCallbackDelegate WriteCallback
        {
            get => Marshal.GetDelegateForFunctionPointer<WriteCallbackDelegate>(Data.writeCallback);
            set => Data.writeCallback = Marshal.GetFunctionPointerForDelegate(value);
        }

        public delegate void UnderflowCallbackDelegate(ref OutStreamData stream);

        public UnderflowCallbackDelegate UnderflowCallback
        {
            get => Marshal.GetDelegateForFunctionPointer<UnderflowCallbackDelegate>(Data.underflowCallback);
            set => Data.underflowCallback = Marshal.GetFunctionPointerForDelegate(value);
        }

        public delegate void ErrorCallbackDelegate(ref OutStreamData stream, Error error);

        public ErrorCallbackDelegate ErrorCallback
        {
            get => Marshal.GetDelegateForFunctionPointer<ErrorCallbackDelegate>(Data.errorCallback);
            set => Data.errorCallback = Marshal.GetFunctionPointerForDelegate(value);
        }

        public string Name => Marshal.PtrToStringAnsi(Data.name);

        public bool NonTerminalHint
        {
            get => Data.nonTerminalHint != 0;
            set => Data.nonTerminalHint = value ? (byte)1 : (byte)0;
        }

        public int BytesPerFrame => Data.bytesPerFrame;
        public int BytesPerSample => Data.bytesPerSample;

        public Error LayoutError => Data.layoutError;

        #endregion

        #region Public properties and methods

        static public OutStream Create(Device device) => _Create(device);

        public Error Open() => _Open(this);
        public Error Start() => _Start(this);
        public Error Pause(bool pause) => _Pause(this, pause ? (byte)1 : (byte)0);
        public Error GetLatency(out double latency) => _GetLatency(this, out latency);

        public Error ClearBuffer() => _ClearBuffer(this);
        public Error SetVolume(double volume) => _SetVolume(this, volume);

        #endregion

        #region Unmanaged functions

        [DllImport(Config.DllName, EntryPoint="soundio_outstream_destroy")]
        extern static void _Destroy(IntPtr stream);

        [DllImport(Config.DllName, EntryPoint="soundio_outstream_create")]
        extern static OutStream _Create(Device device);

        [DllImport(Config.DllName, EntryPoint="soundio_outstream_open")]
        extern static Error _Open(OutStream stream);

        [DllImport(Config.DllName, EntryPoint="soundio_outstream_start")]
        extern static Error _Start(OutStream stream);

        [DllImport(Config.DllName, EntryPoint="soundio_outstream_pause")]
        extern static Error _Pause(OutStream stream, byte pause);

        [DllImport(Config.DllName, EntryPoint="soundio_outstream_get_latency")]
        extern static Error _GetLatency(OutStream stream, out double latency);

        [DllImport(Config.DllName, EntryPoint="soundio_outstream_clear_buffer")]
        extern static Error _ClearBuffer(OutStream stream);

        [DllImport(Config.DllName, EntryPoint="soundio_outstream_set_volume")]
        extern static Error _SetVolume(OutStream stream, double volume);

        #endregion
    }
}
