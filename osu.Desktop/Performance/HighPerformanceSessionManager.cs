// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Game.Performance;

namespace osu.Desktop.Performance
{
    public class HighPerformanceSessionManager : IHighPerformanceSessionManager
    {
        private GCLatencyMode originalGCMode;

        public IDisposable BeginSession()
        {
            enableHighPerformanceSession();
            return new InvokeOnDisposal<HighPerformanceSessionManager>(this, static m => m.disableHighPerformanceSession());
        }

        private void enableHighPerformanceSession()
        {
            Logger.Log("Starting high performance session");

            originalGCMode = GCSettings.LatencyMode;
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;

            // Without doing this, the new GC mode won't kick in until the next GC, which could be at a more noticeable point in time.
            GC.Collect(0);
        }

        private void disableHighPerformanceSession()
        {
            Logger.Log("Ending high performance session");

            if (GCSettings.LatencyMode == GCLatencyMode.LowLatency)
                GCSettings.LatencyMode = originalGCMode;

            // No GC.Collect() as we were already collecting at a higher frequency in the old mode.
        }
    }
}
