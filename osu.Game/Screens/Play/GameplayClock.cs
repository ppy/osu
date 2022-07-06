// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Timing;
using osu.Framework.Utils;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A clock which is used for gameplay elements that need to follow audio time 1:1.
    /// Exposed via DI by <see cref="GameplayClockContainer"/>.
    /// <remarks>
    /// The main purpose of this clock is to stop components using it from accidentally processing the main
    /// <see cref="IFrameBasedClock"/>, as this should only be done once to ensure accuracy.
    /// </remarks>
    /// </summary>
    public class GameplayClock : IFrameBasedClock
    {
        internal readonly IFrameBasedClock UnderlyingClock;

        public readonly BindableBool IsPaused = new BindableBool();

        /// <summary>
        /// All adjustments applied to this clock which don't come from gameplay or mods.
        /// </summary>
        public virtual IEnumerable<Bindable<double>> NonGameplayAdjustments => Enumerable.Empty<Bindable<double>>();

        public GameplayClock(IFrameBasedClock underlyingClock)
        {
            UnderlyingClock = underlyingClock;
        }

        public double CurrentTime => UnderlyingClock.CurrentTime;

        public double Rate => UnderlyingClock.Rate;

        /// <summary>
        /// The rate of gameplay when playback is at 100%.
        /// This excludes any seeking / user adjustments.
        /// </summary>
        public double TrueGameplayRate
        {
            get
            {
                double baseRate = Rate;

                foreach (var adjustment in NonGameplayAdjustments)
                {
                    if (Precision.AlmostEquals(adjustment.Value, 0))
                        return 0;

                    baseRate /= adjustment.Value;
                }

                return baseRate;
            }
        }

        public bool IsRunning => UnderlyingClock.IsRunning;

        public void ProcessFrame()
        {
            // intentionally not updating the underlying clock (handled externally).
        }

        public double ElapsedFrameTime => UnderlyingClock.ElapsedFrameTime;

        public double FramesPerSecond => UnderlyingClock.FramesPerSecond;

        public FrameTimeInfo TimeInfo => UnderlyingClock.TimeInfo;

        public IClock Source => UnderlyingClock;
    }
}
