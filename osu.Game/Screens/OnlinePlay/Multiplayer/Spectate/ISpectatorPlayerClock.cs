// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// A clock which is used by <see cref="MultiSpectatorPlayer"/>s and managed by an <see cref="ISyncManager"/>.
    /// </summary>
    public interface ISpectatorPlayerClock : IFrameBasedClock, IAdjustableClock
    {
        /// <summary>
        /// Whether this clock is waiting on frames to continue playback.
        /// </summary>
        Bindable<bool> WaitingOnFrames { get; }

        /// <summary>
        /// Whether this clock is resynchronising to the master clock.
        /// </summary>
        bool IsCatchingUp { get; set; }

        /// <summary>
        /// The source clock
        /// </summary>
        IFrameBasedClock Source { set; }
    }
}
