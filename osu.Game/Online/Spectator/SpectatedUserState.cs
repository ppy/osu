// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Spectator
{
    public enum SpectatedUserState
    {
        /// <summary>
        /// The spectated user is not yet playing.
        /// </summary>
        Idle,

        /// <summary>
        /// The spectated user is currently playing.
        /// </summary>
        Playing,

        /// <summary>
        /// The spectated user is currently paused. Unused for the time being.
        /// </summary>
        Paused,

        /// <summary>
        /// The spectated user has passed gameplay.
        /// </summary>
        Passed,

        /// <summary>
        /// The spectated user has failed gameplay.
        /// </summary>
        Failed,

        /// <summary>
        /// The spectated user has quit gameplay.
        /// </summary>
        Quit
    }
}
