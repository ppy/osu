// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// An interface for drawables with ability to expand/contract.
    /// </summary>
    public interface IExpandable : IDrawable
    {
        /// <summary>
        /// Whether this drawable is in an expanded state.
        /// </summary>
        BindableBool Expanded { get; }
    }
}
