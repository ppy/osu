// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        private readonly IFrameBasedClock underlyingClock;

        public readonly BindableBool IsPaused = new BindableBool();

        /// <summary>
        /// All adjustments applied to this clock which don't come from gameplay or mods.
        /// </summary>
        public virtual IEnumerable<Bindable<double>> NonGameplayAdjustments => Enumerable.Empty<Bindable<double>>();

        public GameplayClock(IFrameBasedClock underlyingClock)
        {
            this.underlyingClock = underlyingClock;
        }

        public double CurrentTime => underlyingClock.CurrentTime;

        public double Rate => underlyingClock.Rate;

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

        public bool IsRunning => underlyingClock.IsRunning;

        public void ProcessFrame()
        {
            // intentionally not updating the underlying clock (handled externally).
        }

        public double ElapsedFrameTime => underlyingClock.ElapsedFrameTime;

        public double FramesPerSecond => underlyingClock.FramesPerSecond;

        public FrameTimeInfo TimeInfo => underlyingClock.TimeInfo;

        public IClock Source => underlyingClock;
    }
}
