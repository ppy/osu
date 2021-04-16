// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync
{
    /// <summary>
    /// Manages the synchronisation between one or more <see cref="ISlaveClock"/>s in relation to a master clock.
    /// </summary>
    public interface ISyncManager
    {
        /// <summary>
        /// The master clock which slaves should synchronise to.
        /// </summary>
        IAdjustableClock Master { get; }

        /// <summary>
        /// Adds an <see cref="ISlaveClock"/> to manage.
        /// </summary>
        /// <param name="clock">The <see cref="ISlaveClock"/> to add.</param>
        void AddSlave(ISlaveClock clock);

        /// <summary>
        /// Removes an <see cref="ISlaveClock"/>, stopping it from being managed by this <see cref="ISyncManager"/>.
        /// </summary>
        /// <param name="clock">The <see cref="ISlaveClock"/> to remove.</param>
        void RemoveSlave(ISlaveClock clock);
    }
}
