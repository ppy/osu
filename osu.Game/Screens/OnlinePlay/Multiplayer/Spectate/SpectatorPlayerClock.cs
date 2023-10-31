// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// A clock which catches up using rate adjustment.
    /// </summary>
    public class SpectatorPlayerClock : IFrameBasedClock, IAdjustableClock
    {
        /// <summary>
        /// The catch up rate.
        /// </summary>
        private const double catchup_rate = 2;

        private readonly GameplayClockContainer masterClock;

        public double CurrentTime { get; private set; }

        /// <summary>
        /// Whether this clock is waiting on frames to continue playback.
        /// </summary>
        public bool WaitingOnFrames { get; set; } = true;

        /// <summary>
        /// Whether this clock is behind the master clock and running at a higher rate to catch up to it.
        /// </summary>
        /// <remarks>
        /// Of note, this will be false if this clock is *ahead* of the master clock.
        /// </remarks>
        public bool IsCatchingUp { get; set; }

        /// <summary>
        /// Whether this spectator clock should be running.
        /// Use instead of <see cref="Start"/> / <see cref="Stop"/> to control time.
        /// </summary>
        public bool IsRunning { get; set; }

        public SpectatorPlayerClock(GameplayClockContainer masterClock)
        {
            this.masterClock = masterClock;
        }

        public void Reset() => CurrentTime = 0;

        public void Start()
        {
            // Our running state should only be managed by SpectatorSyncManager via IsRunning.
        }

        public void Stop()
        {
            // Our running state should only be managed by an SpectatorSyncManager via IsRunning.
        }

        public bool Seek(double position)
        {
            Logger.Log($"{nameof(SpectatorPlayerClock)} seeked to {position}");
            CurrentTime = position;
            return true;
        }

        public void ResetSpeedAdjustments()
        {
        }

        public double Rate
        {
            get => IsCatchingUp ? catchup_rate : 1;
            set => throw new NotImplementedException();
        }

        public void ProcessFrame()
        {
            if (IsRunning)
            {
                // When in catch-up mode, the source is usually not running.
                // In such a case, its elapsed time may be zero, which would cause catch-up to get stuck.
                // To avoid this, use a constant 16ms elapsed time for now. Probably not too correct, but this whole logic isn't too correct anyway.
                // Clamping is required to ensure that player clocks don't get too far ahead if ProcessFrame is run multiple times.
                double elapsedSource = masterClock.ElapsedFrameTime != 0 ? masterClock.ElapsedFrameTime : Math.Clamp(masterClock.CurrentTime - CurrentTime, 0, 16);
                double elapsed = elapsedSource * Rate;

                CurrentTime += elapsed;
                ElapsedFrameTime = elapsed;
                FramesPerSecond = masterClock.FramesPerSecond;
            }
            else
            {
                ElapsedFrameTime = 0;
                FramesPerSecond = 0;
            }
        }

        public double ElapsedFrameTime { get; private set; }

        public double FramesPerSecond { get; private set; }

        public FrameTimeInfo TimeInfo => new FrameTimeInfo { Elapsed = ElapsedFrameTime, Current = CurrentTime };
    }
}
