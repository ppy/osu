// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Extensions;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that adds the ability to shake its contents.
    /// </summary>
    public partial class ShakeContainer : Container
    {
        /// <summary>
        /// The length of a single shake.
        /// </summary>
        public float ShakeDuration = 80;

        /// <summary>
        /// Shake the contents of this container.
        /// </summary>
        /// <param name="maximumLength">The maximum length the shake should last.</param>
        public void Shake(double? maximumLength = null) => this.Shake(ShakeDuration, maximumLength: maximumLength);
    }
}
