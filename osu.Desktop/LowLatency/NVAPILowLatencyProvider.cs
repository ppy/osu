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

        /// <summary>
        /// Initialize the NVAPI low latency provider with a native device handle. Ensure NVAPI is available before calling this method.
        /// </summary>
        /// <param name="nativeDeviceHandle">An <see cref="IntPtr"/> to the handle of the D3D11 device.</param>
        /// <exception cref="InvalidOperationException">Throws an exception if NVAPI is unavailable, or the device handle provided was invalid.</exception>
        public void Initialize(IntPtr nativeDeviceHandle)
        {
            _deviceHandle = nativeDeviceHandle;
            IsAvailable = NVAPI.Available && _deviceHandle != IntPtr.Zero;

            if (!IsAvailable)
                throw new InvalidOperationException("NVAPI is not available or the provided device handle is invalid.");
        }

        /// <summary>
        /// Set the low latency mode.
        /// </summary>
        /// <param name="mode">The <see cref="LatencyMode"/> to use.</param>
        /// <exception cref="InvalidOperationException">Throws an exception if an attempt to set the low latency mode was unsuccessful.</exception>
        public void SetMode(LatencyMode mode)
        {
            if (!IsAvailable || _deviceHandle == IntPtr.Zero)
                return;

            bool enable = mode != LatencyMode.Off;
            bool boost = mode == LatencyMode.Boost;
            var status = NVAPI.SetSleepModeHelper(_deviceHandle, enable, boost, boost, 0);

            if (status != NvStatus.OK)
                throw new InvalidOperationException($"Failed to set NVAPI low latency (Sleep) mode: {status}");
        }

        /// <summary>
        /// Set a latency marker for the current frame.
        /// </summary>
        /// <remarks>WARNING: Do not log any errors that come from this method, they should be ignored as this method runs in a realtime environment.</remarks>
        /// <param name="marker">The <see cref="LatencyMarker"/> to set.</param>
        /// <param name="frameId">The frame number this marker is for.</param>
        /// <exception cref="InvalidOperationException">Throws an exception if the attempt to set the marker was unsuccessful. Please ensure this exception is ignored.</exception>
        public void SetMarker(LatencyMarker marker, ulong frameId)
        {
            if (!IsAvailable || _deviceHandle == IntPtr.Zero)
                return;

            var status = NVAPI.SetLatencyMarkerHelper(_deviceHandle, (uint)marker, frameId);

            if (status != NvStatus.OK)
                throw new InvalidOperationException($"Failed to set NVAPI latency marker: {status}");
        }

        /// <summary>
        /// Ensure this is called once per frame, at the start of the Update phase, to allow NVAPI to manage frame sleep timing.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws an exception if the Sleep attempt was unsuccessful.</exception>
        public void FrameSleep()
        {
            if (!IsAvailable || _deviceHandle == IntPtr.Zero)
                return;

            var status = NVAPI.FrameSleepHelper(_deviceHandle);

            if (status != NvStatus.OK)
                throw new InvalidOperationException($"Failed to perform NVAPI frame sleep: {status}");
        }
    }
}
