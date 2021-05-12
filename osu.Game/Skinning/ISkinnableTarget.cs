// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Denotes a container which can house <see cref="ISkinnableComponent"/>s.
    /// </summary>
    public interface ISkinnableTarget : IDrawable
    {
        public SkinnableTarget Target { get; }

        /// <summary>
        /// Reload this target from the current skin.
        /// </summary>
        public void Reload();

        /// <summary>
        /// Add the provided item to this target.
        /// </summary>
        public void Add(ISkinnableComponent drawable);
    }
}
