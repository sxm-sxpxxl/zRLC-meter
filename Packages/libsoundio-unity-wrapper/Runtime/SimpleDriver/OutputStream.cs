// Simple driver for libsoundio
// https://github.com/keijiro/jp.keijiro.libsoundio

using System;
using System.Runtime.InteropServices;
using InvalidOp = System.InvalidOperationException;

namespace SoundIO.SimpleDriver
{
    //
    // High-level wrapper class for SoundIoOutStream
    //
    // - Manages an OutStream object.
    // - Implements callback functions for the OutStream object.
    //
    public sealed class OutputStream : IDisposable
    {
        #region Public properties and methods

        public int ChannelCount => _stream.Layout.ChannelCount;
        public int SampleRate => _stream.SampleRate;
        public float Latency => (float)_stream.SoftwareLatency;

        public bool IsValid =>
            _stream != null && !_stream.IsInvalid && !_stream.IsClosed;
        
        public void Dispose()
        {
            _stream?.Dispose();
            _device?.Dispose();
            
            _offsetInSeconds = 0d;
            _sampleReadCallback = null;
        }
        
        #endregion

        #region Constructor

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double SampleReadDelegate(int frameIndex, double offsetInSeconds, double secondsPerFrame);
        
        public OutputStream(Device deviceToOwn, int sampleRate, SampleReadDelegate sampleReadCallback)
        {
            _device = deviceToOwn;
            _stream = OutStream.Create(_device);

            try
            {
                if (_stream.IsInvalid)
                    throw new InvalidOp("Stream allocation error");

                if (_device.Layouts.Length == 0)
                    throw new InvalidOp("No channel layout");

                // Calculate the best latency.
                // TODO: Should we use the target frame rate instead of 1/60?
                var bestLatency = Math.Max(1.0 / 60, _device.SoftwareLatencyMin);

                // Stream properties
                _stream.Format = Format.Float32LE;
                _stream.Layout = _device.Layouts[0];
                _stream.SampleRate = sampleRate;
                _stream.SoftwareLatency = bestLatency;
                _stream.WriteCallback = _writeCallback;
                _stream.UnderflowCallback = _underflowCallback;
                _stream.ErrorCallback = _errorCallback;
                _stream.UserData = IntPtr.Zero;

                var err = _stream.Open();

                if (err != Error.None)
                    throw new InvalidOp($"Stream initialization error ({err})");

                _sampleReadCallback = sampleReadCallback;
                
                // Start streaming.
                _stream.Start();
            }
            catch
            {
                _stream.Dispose();
                _device.Dispose();
                _stream = null;
                _device = null;
                throw;
            }
        }

        public void SetVolume(float volume)
        {
            var error = _stream.SetVolume(volume);
            if (error != Error.None)
                throw new InvalidOperationException($"Output stream set volume error ({error})");
        }

        #endregion

        #region Internal objects

        // SoundIO objects
        Device _device;
        OutStream _stream;

        #endregion

        #region SoundIO callback delegates

        static OutStream.WriteCallbackDelegate _writeCallback = OnWriteOutStream;
        static OutStream.UnderflowCallbackDelegate _underflowCallback = OnUnderflowOutStream;
        static OutStream.ErrorCallbackDelegate _errorCallback = OnErrorOutStream;

        static SampleReadDelegate _sampleReadCallback = null;
        static double _offsetInSeconds = 0.0;
        
        unsafe static void WriteSample(IntPtr ptr, double sample)
        {
            float* buf = (float*) ptr;
            *buf = (float) sample;
        }
        
        [AOT.MonoPInvokeCallback(typeof(OutStream.WriteCallbackDelegate))]
        unsafe static void OnWriteOutStream(ref OutStreamData stream, int min, int left)
        {
            double secondsPerFrame = 1.0 / stream.SampleRate;
            
            while (left > 0)
            {
                var count = left;
                ChannelArea* areas;
                stream.BeginWrite(out areas, ref count);
                
                // When getting count == 0, we must stop writing
                // immediately without calling OutStream.EndWrite.
                if (count == 0) break;

                for (int frame = 0; frame < count; frame++)
                {
                    double sample = _sampleReadCallback.Invoke(frame, _offsetInSeconds, secondsPerFrame);
                    for (int channel = 0; channel < stream.Layout.ChannelCount; channel++)
                    {
                        WriteSample(areas[channel].Pointer, sample);
                        areas[channel].Pointer += areas[channel].Step;
                    }
                }
                _offsetInSeconds = (_offsetInSeconds + secondsPerFrame * count) % 1.0;
                
                stream.EndWrite();
                left -= count;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(OutStream.UnderflowCallbackDelegate))]
        static void OnUnderflowOutStream(ref OutStreamData stream)
        {
            UnityEngine.Debug.LogWarning("OutStream underflow");
        }

        [AOT.MonoPInvokeCallback(typeof(OutStream.ErrorCallbackDelegate))]
        static void OnErrorOutStream(ref OutStreamData stream, Error error)
        {
            UnityEngine.Debug.LogError($"OutStream error ({error})");
        }

        #endregion
    }
}
