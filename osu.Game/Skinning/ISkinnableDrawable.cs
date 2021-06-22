// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Denotes a drawable which, as a drawable, can be adjusted via skinning specifications.
    /// </summary>
    public interface ISkinnableDrawable : IDrawable
    {
        /// <summary>
        /// Whether this component should be editable by an end user.
        /// </summary>
        bool IsEditable => true;

        /// <summary>
        /// In the context of the skin layout editor, whether this <see cref="ISkinnableDrawable"/> has a permanent anchor defined.
        /// If <see langword="false"/>, this <see cref="ISkinnableDrawable"/>'s <see cref="Drawable.Anchor"/> is automatically determined by proximity,
        /// If <see langword="true"/>, a fixed anchor point has been defined.
        /// </summary>
        bool UsesFixedAnchor { get; set; }
    }
}
