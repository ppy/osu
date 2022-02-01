// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Spectator
{
    public enum SpectatingUserState
    {
        /// <summary>
        /// The spectated user has not yet played.
        /// </summary>
        Idle,

        /// <summary>
        /// The spectated user is currently playing.
        /// </summary>
        Playing,

        /// <summary>
        /// The spectated user has successfully completed gameplay.
        /// </summary>
        Completed,

        /// <summary>
        /// The spectator user has failed during gameplay.
        /// </summary>
        Failed,

        /// <summary>
        /// The spectated user has quit during gameplay.
        /// </summary>
        Quit
    }
}
