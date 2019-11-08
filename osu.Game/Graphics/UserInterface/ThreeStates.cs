// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// An on/off state with an extra indeterminate state.
    /// </summary>
    public enum ThreeStates
    {
        /// <summary>
        /// The current state is disabled.
        /// </summary>
        Disabled,

        /// <summary>
        /// The current state is a combination of <see cref="Disabled"/> and <see cref="Enabled"/>.
        /// The state becomes <see cref="Enabled"/> if the <see cref="ThreeStateMenuItem"/> is pressed.
        /// </summary>
        Indeterminate,

        /// <summary>
        /// The current state is enabled.
        /// </summary>
        Enabled
    }
}
