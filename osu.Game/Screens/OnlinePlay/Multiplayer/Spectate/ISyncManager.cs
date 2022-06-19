// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Bindables;
using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// Manages the synchronisation between one or more <see cref="ISpectatorPlayerClock"/>s in relation to a master clock.
    /// </summary>
    public interface ISyncManager
    {
        /// <summary>
        /// An event which is invoked when gameplay is ready to start.
        /// </summary>
        event Action ReadyToStart;

        /// <summary>
        /// The master clock which player clocks should synchronise to.
        /// </summary>
        IAdjustableClock MasterClock { get; }

        /// <summary>
        /// An event which is invoked when the state of <see cref="MasterClock"/> is changed.
        /// </summary>
        IBindable<MasterClockState> MasterState { get; }

        /// <summary>
        /// Adds an <see cref="ISpectatorPlayerClock"/> to manage.
        /// </summary>
        /// <param name="clock">The <see cref="ISpectatorPlayerClock"/> to add.</param>
        void AddPlayerClock(ISpectatorPlayerClock clock);

        /// <summary>
        /// Removes an <see cref="ISpectatorPlayerClock"/>, stopping it from being managed by this <see cref="ISyncManager"/>.
        /// </summary>
        /// <param name="clock">The <see cref="ISpectatorPlayerClock"/> to remove.</param>
        void RemovePlayerClock(ISpectatorPlayerClock clock);
    }
}
