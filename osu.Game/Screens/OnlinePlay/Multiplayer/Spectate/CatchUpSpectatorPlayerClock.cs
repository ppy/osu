// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// A <see cref="ISpectatorPlayerClock"/> which catches up using rate adjustment.
    /// </summary>
    public class CatchUpSpectatorPlayerClock : ISpectatorPlayerClock
    {
        /// <summary>
        /// The catch up rate.
        /// </summary>
        public const double CATCHUP_RATE = 2;

        public readonly IFrameBasedClock Source;

        public double CurrentTime { get; private set; }

        public bool IsRunning { get; private set; }

        public CatchUpSpectatorPlayerClock(IFrameBasedClock source)
        {
            Source = source;
        }

        public void Reset() => CurrentTime = 0;

        public void Start() => IsRunning = true;

        public void Stop() => IsRunning = false;

        void IAdjustableClock.Start()
        {
            // Our running state should only be managed by an ISyncManager, ignore calls from external sources.
        }

        void IAdjustableClock.Stop()
        {
            // Our running state should only be managed by an ISyncManager, ignore calls from external sources.
        }

        public bool Seek(double position)
        {
            CurrentTime = position;
            return true;
        }

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
            ElapsedFrameTime = 0;
            FramesPerSecond = 0;

            Source.ProcessFrame();

            if (IsRunning)
            {
                // When in catch-up mode, the source is usually not running.
                // In such a case, its elapsed time may be zero, which would cause catch-up to get stuck.
                // To avoid this, use a constant 16ms elapsed time for now. Probably not too correct, but this whole logic isn't too correct anyway.
                double elapsedSource = Source.IsRunning ? Source.ElapsedFrameTime : 16;
                double elapsed = elapsedSource * Rate;

                CurrentTime += elapsed;
                ElapsedFrameTime = elapsed;
                FramesPerSecond = Source.FramesPerSecond;
            }
        }

        public double ElapsedFrameTime { get; private set; }

        public double FramesPerSecond { get; private set; }

        public FrameTimeInfo TimeInfo => new FrameTimeInfo { Elapsed = ElapsedFrameTime, Current = CurrentTime };

        public Bindable<bool> WaitingOnFrames { get; } = new Bindable<bool>(true);

        public bool IsCatchingUp { get; set; }
    }
}
