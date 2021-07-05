// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Input;

namespace osu.Game.Input
{
    /// <summary>
    /// Determines the situations in which the mouse cursor should be confined to the window.
    /// Expands upon <see cref="ConfineMouseMode"/> by providing the option to confine during gameplay.
    /// </summary>
    public enum OsuConfineMouseMode
    {
        /// <summary>
        /// The mouse cursor will be free to move outside the game window.
        /// </summary>
        Never,

        /// <summary>
        /// The mouse cursor will be locked to the window bounds during gameplay,
        /// but may otherwise move freely.
        /// </summary>
        [Description("During Gameplay")]
        DuringGameplay,

        /// <summary>
        /// The mouse cursor will always be locked to the window bounds while the game has focus.
        /// </summary>
        Always
    }
}
