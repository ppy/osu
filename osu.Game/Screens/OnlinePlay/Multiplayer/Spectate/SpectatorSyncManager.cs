// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// Manages the synchronisation between one or more <see cref="SpectatorPlayerClock"/>s in relation to a master clock.
    /// </summary>
    public partial class SpectatorSyncManager : Component
    {
        /// <summary>
        /// The offset from the master clock to which player clocks should remain within to be considered in-sync.
        /// </summary>
        public const double SYNC_TARGET = 16;

        /// <summary>
        /// The offset from the master clock at which player clocks begin resynchronising.
        /// </summary>
        public const double MAX_SYNC_OFFSET = 50;

        /// <summary>
        /// The maximum delay to start gameplay, if any (but not all) player clocks are ready.
        /// </summary>
        public const double MAXIMUM_START_DELAY = 15000;

        /// <summary>
        /// An event which is invoked when gameplay is ready to start.
        /// </summary>
        public Action? ReadyToStart;

        public double CurrentMasterTime => masterClock.CurrentTime;

        /// <summary>
        /// The master clock which is used to control the timing of all player clocks clocks.
        /// </summary>
        private readonly GameplayClockContainer masterClock;

        /// <summary>
        /// The player clocks.
        /// </summary>
        private readonly List<SpectatorPlayerClock> playerClocks = new List<SpectatorPlayerClock>();

        private MasterClockState masterState = MasterClockState.Synchronised;

        private bool hasStarted;

        private double? firstStartAttemptTime;

        public SpectatorSyncManager(GameplayClockContainer master)
        {
            masterClock = master;
        }

        /// <summary>
        /// Create a new managed <see cref="SpectatorPlayerClock"/>.
        /// </summary>
        /// <returns>The newly created <see cref="SpectatorPlayerClock"/>.</returns>
        public SpectatorPlayerClock CreateManagedClock()
        {
            var clock = new SpectatorPlayerClock(masterClock);
            playerClocks.Add(clock);
            return clock;
        }

        /// <summary>
        /// Removes an <see cref="SpectatorPlayerClock"/>, stopping it from being managed by this <see cref="SpectatorSyncManager"/>.
        /// </summary>
        /// <param name="clock">The <see cref="SpectatorPlayerClock"/> to remove.</param>
        public void RemoveManagedClock(SpectatorPlayerClock clock)
        {
            playerClocks.Remove(clock);
            clock.IsRunning = false;
        }

        protected override void Update()
        {
            base.Update();

            if (!attemptStart())
            {
                // Ensure all player clocks are stopped until the start succeeds.
                foreach (var clock in playerClocks)
                    clock.IsRunning = false;
                return;
            }

            updatePlayerCatchup();
            updateMasterState();
        }

        /// <summary>
        /// Attempts to start playback. Waits for all player clocks to have available frames for up to <see cref="MAXIMUM_START_DELAY"/> milliseconds.
        /// </summary>
        /// <returns>Whether playback was started and syncing should occur.</returns>
        private bool attemptStart()
        {
            if (hasStarted)
                return true;

            if (playerClocks.Count == 0)
                return false;

            int readyCount = playerClocks.Count(s => !s.WaitingOnFrames);

            if (readyCount == playerClocks.Count)
                return performStart();

            if (readyCount > 0)
            {
                firstStartAttemptTime ??= Time.Current;

                if (Time.Current - firstStartAttemptTime > MAXIMUM_START_DELAY)
                    return performStart();
            }

            bool performStart()
            {
                ReadyToStart?.Invoke();
                return hasStarted = true;
            }

            return false;
        }

        /// <summary>
        /// Updates the catchup states of all player clocks clocks.
        /// </summary>
        private void updatePlayerCatchup()
        {
            for (int i = 0; i < playerClocks.Count; i++)
            {
                var clock = playerClocks[i];

                // How far this player's clock is out of sync, compared to the master clock.
                // A negative value means the player is running fast (ahead); a positive value means the player is running behind (catching up).
                double timeDelta = masterClock.CurrentTime - clock.CurrentTime;

                // Check that the player clock isn't too far ahead.
                // This is a quiet case in which the catchup is done by the master clock, so IsCatchingUp is not set on the player clock.
                if (timeDelta < -SYNC_TARGET)
                {
                    // Importantly, set the clock to a non-catchup state. if this isn't done, updateMasterState may incorrectly pause the master clock
                    // when it is required to be running (ie. if all players are ahead of the master).
                    clock.IsCatchingUp = false;
                    clock.IsRunning = false;
                    continue;
                }

                // Make sure the player clock is running if it can.
                clock.IsRunning = !clock.WaitingOnFrames;

                if (clock.IsCatchingUp)
                {
                    // Stop the player clock from catching up if it's within the sync target.
                    if (timeDelta <= SYNC_TARGET)
                        clock.IsCatchingUp = false;
                }
                else
                {
                    // Make the player clock start catching up if it's exceeded the maximum allowable sync offset.
                    if (timeDelta > MAX_SYNC_OFFSET)
                        clock.IsCatchingUp = true;
                }
            }
        }

        /// <summary>
        /// Updates the state of the master clock.
        /// </summary>
        private void updateMasterState()
        {
            MasterClockState newState = playerClocks.Any(s => !s.IsCatchingUp) ? MasterClockState.Synchronised : MasterClockState.TooFarAhead;

            if (masterState == newState)
                return;

            masterState = newState;
            Logger.Log($"{nameof(SpectatorSyncManager)}'s master clock became {masterState}");

            switch (masterState)
            {
                case MasterClockState.Synchronised:
                    if (hasStarted)
                        masterClock.Start();

                    break;

                case MasterClockState.TooFarAhead:
                    masterClock.Stop();
                    break;
            }
        }
    }
}
