// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Play
{
    public enum LocalUserPlayingStates
    {
        /// <summary>
        /// The local player is not current in gameplay.
        /// </summary>
        NotPlaying,

        /// <summary>
        /// The local player is in a break, paused, or failed or passed but still at the gameplay screen.
        /// </summary>
        Break,

        /// <summary>
        /// The local user is in active gameplay.
        /// </summary>
        Playing,
    }
}
