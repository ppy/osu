// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync
{
    public class SpectatorCatchUpSlaveClock : ISpectatorSlaveClock
    {
        /// <summary>
        /// The catchup rate.
        /// </summary>
        public const double CATCHUP_RATE = 2;

        private readonly IFrameBasedClock masterClock;

        public SpectatorCatchUpSlaveClock(IFrameBasedClock masterClock)
        {
            this.masterClock = masterClock;
        }

        public double CurrentTime { get; private set; }

        public bool IsRunning { get; private set; }

        public void Reset() => CurrentTime = 0;

        public void Start() => IsRunning = true;

        public void Stop() => IsRunning = false;

        public bool Seek(double position) => true;

        public void ResetSpeedAdjustments()
        {
        }

        public double Rate => IsCatchingUp ? CATCHUP_RATE : 1;

        double IAdjustableClock.Rate
        {
            get => Rate;
            set => throw new NotSupportedException();
        }

        double IClock.Rate => Rate;

        public void ProcessFrame()
        {
            masterClock.ProcessFrame();

            ElapsedFrameTime = 0;
            FramesPerSecond = 0;

            if (IsRunning)
            {
                double elapsedSource = masterClock.ElapsedFrameTime;
                double elapsed = elapsedSource * Rate;

                CurrentTime += elapsed;
                ElapsedFrameTime = elapsed;
                FramesPerSecond = masterClock.FramesPerSecond;
            }
        }

        public double ElapsedFrameTime { get; private set; }

        public double FramesPerSecond { get; private set; }

        public FrameTimeInfo TimeInfo => new FrameTimeInfo { Elapsed = ElapsedFrameTime, Current = CurrentTime };

        public IBindable<bool> WaitingOnFrames { get; } = new Bindable<bool>();

        public bool IsCatchingUp { get; set; }
    }
}
