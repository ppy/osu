// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#pragma warning disable IDE1006 // Naming rule violation

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using osu.Framework.Graphics.Rendering.LowLatency;

namespace osu.Desktop.LowLatency
{
    /// <summary>
    /// Provider for NVIDIA's NVAPI (Reflex) low latency features.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SupportedOSPlatform("windows")]
    internal sealed class NVAPILowLatencyProvider : ILowLatencyProvider
    {
        public bool IsAvailable { get; private set; }

        private IntPtr _deviceHandle;

        public void Initialize(IntPtr nativeDeviceHandle)
        {
            _deviceHandle = nativeDeviceHandle;
            IsAvailable = NVAPI.Available && _deviceHandle != IntPtr.Zero;
        }

        public void SetMode(LatencyMode mode)
        {
            if (!IsAvailable || _deviceHandle == IntPtr.Zero)
                return;

            bool enable = mode != LatencyMode.Off;
            bool boost = mode == LatencyMode.Boost;
            var status = NVAPI.SetSleepModeHelper(_deviceHandle, enable, boost, boost, 1000);
            Console.WriteLine("NVAPI SetSleepMode status: " + status);
        }

        public void SetMarker(LatencyMarker marker, ulong frameId)
        {
            if (!IsAvailable || _deviceHandle == IntPtr.Zero)
                return;

            var status = NVAPI.SetLatencyMarkerHelper(_deviceHandle, (uint)marker, frameId);
            Console.WriteLine("NVAPI SetMarker status: " + status);
        }

        public void FrameSleep()
        {
            if (!IsAvailable || _deviceHandle == IntPtr.Zero)
                return;

            var status = NVAPI.FrameSleepHelper(_deviceHandle);
            Console.WriteLine("NVAPI FrameSleep status: " + status);
        }
    }
}
