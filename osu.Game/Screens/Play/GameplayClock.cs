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
    public class GameplayClock : IGameplayClock
    {
        internal readonly IFrameBasedClock UnderlyingClock;

        public readonly BindableBool IsPaused = new BindableBool();

        IBindable<bool> IGameplayClock.IsPaused => IsPaused;

        public virtual IEnumerable<double> NonGameplayAdjustments => Enumerable.Empty<double>();

        public GameplayClock(IFrameBasedClock underlyingClock)
        {
            UnderlyingClock = underlyingClock;
        }

        public double? StartTime { get; internal set; }

        public double CurrentTime => UnderlyingClock.CurrentTime;

        public double Rate => UnderlyingClock.Rate;

        public double TrueGameplayRate
        {
            get
            {
                double baseRate = Rate;

                foreach (double adjustment in NonGameplayAdjustments)
                {
                    if (Precision.AlmostEquals(adjustment, 0))
                        return 0;

                    baseRate /= adjustment;
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
