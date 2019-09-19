// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Timing;

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

        public GameplayClock(IFrameBasedClock underlyingClock)
        {
            this.underlyingClock = underlyingClock;
        }

        public double CurrentTime => underlyingClock.CurrentTime;

        public double Rate => underlyingClock.Rate;

        public bool IsRunning => underlyingClock.IsRunning;

        public void ProcessFrame()
        {
            // we do not want to process the underlying clock.
            // this is handled by PauseContainer.
        }

        public double ElapsedFrameTime => underlyingClock.ElapsedFrameTime;

        public double FramesPerSecond => underlyingClock.FramesPerSecond;

        public FrameTimeInfo TimeInfo => underlyingClock.TimeInfo;

        public IClock Source => underlyingClock;
    }
}
