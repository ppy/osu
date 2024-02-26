// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Performance
{
    public partial class HighPerformanceSessionManager : Component
    {
        private GCLatencyMode originalGCMode;

        public IDisposable BeginSession()
        {
            EnableHighPerformanceSession();
            return new InvokeOnDisposal<HighPerformanceSessionManager>(this, static m => m.DisableHighPerformanceSession());
        }

        protected virtual void EnableHighPerformanceSession()
        {
            originalGCMode = GCSettings.LatencyMode;
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            GC.Collect(0);
        }

        protected virtual void DisableHighPerformanceSession()
        {
            if (GCSettings.LatencyMode == GCLatencyMode.LowLatency)
                GCSettings.LatencyMode = originalGCMode;
        }
    }
}
