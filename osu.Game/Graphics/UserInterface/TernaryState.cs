// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// An on/off state with an extra indeterminate state.
    /// </summary>
    public enum TernaryState
    {
        /// <summary>
        /// The current state is false.
        /// </summary>
        False,

        /// <summary>
        /// The current state is a combination of <see cref="False"/> and <see cref="True"/>.
        /// The state becomes <see cref="True"/> if the <see cref="TernaryStateMenuItem"/> is pressed.
        /// </summary>
        Indeterminate,

        /// <summary>
        /// The current state is true.
        /// </summary>
        True
    }
}
