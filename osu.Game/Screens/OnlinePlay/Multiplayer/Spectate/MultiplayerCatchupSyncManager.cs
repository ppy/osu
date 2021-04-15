// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiplayerCatchupSyncManager : Component, IMultiplayerSpectatorSyncManager
    {
        /// <summary>
        /// The offset from the master clock to which slaves should be synchronised to.
        /// </summary>
        public const double SYNC_TARGET = 16;

        /// <summary>
        /// The offset from the master clock at which slaves begin resynchronising.
        /// </summary>
        public const double MAX_SYNC_OFFSET = 50;

        /// <summary>
        /// The maximum delay to start gameplay, if any (but not all) slaves are ready.
        /// </summary>
        public const double MAXIMUM_START_DELAY = 15000;

        /// <summary>
        /// The master clock which is used to control the timing of all slave clocks.
        /// </summary>
        public IAdjustableClock Master { get; }

        /// <summary>
        /// The slave clocks.
        /// </summary>
        private readonly List<IMultiplayerSpectatorSlaveClock> slaves = new List<IMultiplayerSpectatorSlaveClock>();

        private bool hasStarted;
        private double? firstStartAttemptTime;

        public MultiplayerCatchupSyncManager(IAdjustableClock master)
        {
            Master = master;
        }

        public void AddSlave(IMultiplayerSpectatorSlaveClock clock) => slaves.Add(clock);

        public void RemoveSlave(IMultiplayerSpectatorSlaveClock clock) => slaves.Remove(clock);

        protected override void Update()
        {
            base.Update();

            if (!attemptStart())
            {
                // Ensure all slaves are stopped until the start succeeds.
                foreach (var slave in slaves)
                    slave.Stop();
                return;
            }

            updateCatchup();
            updateMasterClock();
        }

        /// <summary>
        /// Attempts to start playback. Awaits for all slaves to have available frames for up to <see cref="MAXIMUM_START_DELAY"/> milliseconds.
        /// </summary>
        /// <returns>Whether playback was started and syncing should occur.</returns>
        private bool attemptStart()
        {
            if (hasStarted)
                return true;

            if (slaves.Count == 0)
                return false;

            firstStartAttemptTime ??= Time.Current;

            int readyCount = slaves.Count(s => !s.WaitingOnFrames.Value);

            if (readyCount == slaves.Count)
            {
                Logger.Log("Gameplay started (all ready).");
                return hasStarted = true;
            }

            if (readyCount > 0 && (Time.Current - firstStartAttemptTime) > MAXIMUM_START_DELAY)
            {
                Logger.Log($"Gameplay started (maximum delay exceeded, {readyCount}/{slaves.Count} ready).");
                return hasStarted = true;
            }

            return false;
        }

        /// <summary>
        /// Updates the catchup states of all slave clocks.
        /// </summary>
        private void updateCatchup()
        {
            for (int i = 0; i < slaves.Count; i++)
            {
                var slave = slaves[i];
                double timeDelta = Master.CurrentTime - slave.CurrentTime;

                // Check that the slave isn't too far ahead.
                // This is a quiet case in which the catchup is done by the master clock, so IsCatchingUp is not set on the slave.
                if (timeDelta < -SYNC_TARGET)
                {
                    slave.Stop();
                    continue;
                }

                // Make sure the slave is running if it can.
                if (!slave.WaitingOnFrames.Value)
                    slave.Start();

                if (slave.IsCatchingUp)
                {
                    // Stop the slave from catching up if it's within the sync target.
                    if (timeDelta <= SYNC_TARGET)
                    {
                        slave.IsCatchingUp = false;
                        Logger.Log($"Slave {i} catchup finished (delta = {timeDelta})");
                    }
                }
                else
                {
                    // Make the slave start catching up if it's exceeded the maximum allowable sync offset.
                    if (timeDelta > MAX_SYNC_OFFSET)
                    {
                        slave.IsCatchingUp = true;
                        Logger.Log($"Slave {i} catchup started (too far behind, delta = {timeDelta})");
                    }
                }
            }
        }

        /// <summary>
        /// Updates the master clock's running state.
        /// </summary>
        private void updateMasterClock()
        {
            bool anyInSync = slaves.Any(s => !s.IsCatchingUp);

            if (Master.IsRunning != anyInSync)
            {
                if (anyInSync)
                    Master.Start();
                else
                    Master.Stop();
            }
        }
    }
}
