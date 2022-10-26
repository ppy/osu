// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    /// <summary>
    /// Provides access to the device's keyboard data. Eg Hight.
    /// </summary>
    public abstract class KeyboardService
    {
        /// <summary>
        /// The height of the keyboard in pixels.
        /// </summary>
        public abstract double? Height { get; }
    }
}
