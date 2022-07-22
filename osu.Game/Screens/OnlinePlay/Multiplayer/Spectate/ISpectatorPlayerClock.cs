// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
        /// Starts this <see cref="ISpectatorPlayerClock"/>.
        /// </summary>
        new void Start();

        /// <summary>
        /// Stops this <see cref="ISpectatorPlayerClock"/>.
        /// </summary>
        new void Stop();

        /// <summary>
        /// Whether this clock is waiting on frames to continue playback.
        /// </summary>
        Bindable<bool> WaitingOnFrames { get; }

        /// <summary>
        /// Whether this clock is behind the master clock and running at a higher rate to catch up to it.
        /// </summary>
        /// <remarks>
        /// Of note, this will be false if this clock is *ahead* of the master clock.
        /// </remarks>
        bool IsCatchingUp { get; set; }

        /// <summary>
        /// The source clock
        /// </summary>
        IFrameBasedClock Source { set; }
    }
}
