// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// A <see cref="ISyncManager"/> which synchronises de-synced player clocks through catchup.
    /// </summary>
    public class CatchUpSyncManager : Component, ISyncManager
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

        public event Action ReadyToStart;

        /// <summary>
        /// The master clock which is used to control the timing of all player clocks clocks.
        /// </summary>
        public IAdjustableClock MasterClock { get; }

        public IBindable<MasterClockState> MasterState => masterState;

        /// <summary>
        /// The player clocks.
        /// </summary>
        private readonly List<ISpectatorPlayerClock> playerClocks = new List<ISpectatorPlayerClock>();

        private readonly Bindable<MasterClockState> masterState = new Bindable<MasterClockState>();

        private bool hasStarted;
        private double? firstStartAttemptTime;

        public CatchUpSyncManager(IAdjustableClock master)
        {
            MasterClock = master;
        }

        public void AddPlayerClock(ISpectatorPlayerClock clock)
        {
            Debug.Assert(!playerClocks.Contains(clock));
            playerClocks.Add(clock);
        }

        public void RemovePlayerClock(ISpectatorPlayerClock clock)
        {
            playerClocks.Remove(clock);
            clock.Stop();
        }

        protected override void Update()
        {
            base.Update();

            if (!attemptStart())
            {
                // Ensure all player clocks are stopped until the start succeeds.
                foreach (var clock in playerClocks)
                    clock.Stop();
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

            int readyCount = playerClocks.Count(s => !s.WaitingOnFrames.Value);

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
                double timeDelta = MasterClock.CurrentTime - clock.CurrentTime;

                // Check that the player clock isn't too far ahead.
                // This is a quiet case in which the catchup is done by the master clock, so IsCatchingUp is not set on the player clock.
                if (timeDelta < -SYNC_TARGET)
                {
                    // Importantly, set the clock to a non-catchup state. if this isn't done, updateMasterState may incorrectly pause the master clock
                    // when it is required to be running (ie. if all players are ahead of the master).
                    clock.IsCatchingUp = false;
                    clock.Stop();
                    continue;
                }

                // Make sure the player clock is running if it can.
                if (!clock.WaitingOnFrames.Value)
                    clock.Start();

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
            bool anyInSync = playerClocks.Any(s => !s.IsCatchingUp);
            masterState.Value = anyInSync ? MasterClockState.Synchronised : MasterClockState.TooFarAhead;
        }
    }
}
