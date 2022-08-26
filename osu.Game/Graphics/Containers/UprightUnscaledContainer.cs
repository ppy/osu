// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Graphics.Containers
{
    public enum ScaleMode
    {
        /// <summary>
        /// Prevent this container from scaling.
        /// </summary>
        NoScaling,

        /// <summary>
        /// Scale uniformly (maintaining aspect ratio) based on the vertical scale of the parent.
        /// </summary>
        Vertical,

        /// <summary>
        /// Scale uniformly (maintaining aspect ratio) based on the horizontal scale of the parent.
        /// </summary>
        Horizontal,
    }
}
