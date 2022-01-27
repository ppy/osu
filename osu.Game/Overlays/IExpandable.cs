// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays
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

        /// <summary>
        /// Whether this drawable should be/stay expanded by a parenting <see cref="IExpandingContainer"/>.
        /// By default, this is <see langword="true"/> when this drawable is in a hovered or dragged state.
        /// </summary>
        /// <remarks>
        /// This is defined for certain controls which may have a child handling dragging instead.
        /// (e.g. <see cref="ExpandableSlider{T,TSlider}"/> in which dragging is handled by their underlying <see cref="OsuSliderBar{T}"/> control).
        /// </remarks>
        bool ShouldBeExpanded => IsHovered || IsDragged;
    }
}
