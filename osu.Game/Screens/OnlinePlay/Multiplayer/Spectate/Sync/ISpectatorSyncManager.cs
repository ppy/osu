// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync
{
    /// <summary>
    /// Manages the synchronisation between one or more slave clocks in relation to a master clock.
    /// </summary>
    public interface ISpectatorSyncManager
    {
        /// <summary>
        /// The master clock which slaves should synchronise to.
        /// </summary>
        IAdjustableClock Master { get; }

        /// <summary>
        /// Adds a slave clock.
        /// </summary>
        /// <param name="clock">The clock to add.</param>
        void AddSlave(ISpectatorSlaveClock clock);

        /// <summary>
        /// Removes a slave clock.
        /// </summary>
        /// <param name="clock">The clock to remove.</param>
        void RemoveSlave(ISpectatorSlaveClock clock);
    }
}
