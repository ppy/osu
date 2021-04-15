// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync
{
    /// <summary>
    /// Manages the synchronisation between one or more <see cref="ISpectatorSlaveClock"/>s in relation to a master clock.
    /// </summary>
    public interface ISpectatorSyncManager
    {
        /// <summary>
        /// The master clock which slaves should synchronise to.
        /// </summary>
        IAdjustableClock Master { get; }

        /// <summary>
        /// Adds an <see cref="ISpectatorSlaveClock"/> to manage.
        /// </summary>
        /// <param name="clock">The <see cref="ISpectatorSlaveClock"/> to add.</param>
        void AddSlave(ISpectatorSlaveClock clock);

        /// <summary>
        /// Removes an <see cref="ISpectatorSlaveClock"/>, stopping it from being managed by this <see cref="ISpectatorSyncManager"/>.
        /// </summary>
        /// <param name="clock">The <see cref="ISpectatorSlaveClock"/> to remove.</param>
        void RemoveSlave(ISpectatorSlaveClock clock);
    }
}
