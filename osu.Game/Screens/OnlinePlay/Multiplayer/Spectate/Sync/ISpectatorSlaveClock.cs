// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync
{
    /// <summary>
    /// A clock which is used by <see cref="MultiplayerSpectatorPlayer"/>s and managed by an <see cref="ISpectatorSyncManager"/>.
    /// </summary>
    public interface ISpectatorSlaveClock : IFrameBasedClock, IAdjustableClock
    {
        /// <summary>
        /// Whether this clock is waiting on frames to continue playback.
        /// </summary>
        IBindable<bool> WaitingOnFrames { get; }

        /// <summary>
        /// Whether this clock is resynchronising to the master clock.
        /// </summary>
        bool IsCatchingUp { get; set; }
    }
}
