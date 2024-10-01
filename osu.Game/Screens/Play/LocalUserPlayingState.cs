// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Play
{
    public enum LocalUserPlayingState
    {
        /// <summary>
        /// The local player is not current in gameplay. If watching a replay, gameplay always remains in this state.
        /// </summary>
        NotPlaying,

        /// <summary>
        /// The local player is in a break, paused, or failed but still at the gameplay screen.
        /// </summary>
        Break,

        /// <summary>
        /// The local user is in active gameplay.
        /// </summary>
        Playing,
    }
}
